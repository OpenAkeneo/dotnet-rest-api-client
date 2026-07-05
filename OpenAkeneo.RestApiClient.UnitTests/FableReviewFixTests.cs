using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// Locking tests for the fixes applied from the 2026-07 Fable review (see fable-review.md).
/// Each region names the finding it locks.
/// </summary>
public class FableReviewFixTests
{
    private static readonly CancellationToken CT = TestContext.Current.CancellationToken;

    // -------------------------------------------------------------------------
    // Finding 1.1 — retry policy is verb-aware
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Post_On500_IsNotRetried()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.InternalServerError, "boom"));

        var svc = Helpers.BuildService(handler);
        await Assert.ThrowsAsync<AkeneoApiException>(() => svc.HttpPostAsync("/api/rest/v1/products", "{}", CT));

        // token + exactly one POST attempt — a 500 must never replay a non-idempotent verb.
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task Post_On429_IsRetriedUntilSuccess()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.TooManyRequests, ""),
            FakeHttpHandler.Ok("""{"ok":true}"""));

        var svc = Helpers.BuildService(handler);
        var result = await svc.HttpPostAsync("/api/rest/v1/products", "{}", CT);

        Assert.Contains("ok", result);
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task Get_On500_IsStillRetried()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.InternalServerError, "boom"),
            FakeHttpHandler.Ok("""{"ok":true}"""));

        var svc = Helpers.BuildService(handler);
        var result = await svc.HttpGetAsync("/api/rest/v1/products", CT);

        Assert.Contains("ok", result);
        Assert.Equal(3, handler.CallCount);
    }

    // -------------------------------------------------------------------------
    // 401 refresh path (coverage gap 3.1)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Get_On401_RefreshesTokenOnce_AndRetries()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse("token-1"),
            FakeHttpHandler.Text(HttpStatusCode.Unauthorized, ""),
            FakeHttpHandler.TokenResponse("token-2"),
            FakeHttpHandler.Ok("""{"ok":true}"""));

        var svc = Helpers.BuildService(handler);
        var result = await svc.HttpGetAsync("/api/rest/v1/products", CT);

        Assert.Contains("ok", result);
        Assert.Equal(2, handler.Captured.Count(r => r.RequestUri.Contains("/oauth/")));
        // The refreshed token is now the cached one.
        Assert.Equal("token-2", await svc.GetTokenAsync(ct: CT));
    }

    [Fact]
    public async Task Get_On401Twice_ThrowsAkeneoApiException_NoInfiniteLoop()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse("token-1"),
            FakeHttpHandler.Text(HttpStatusCode.Unauthorized, ""),
            FakeHttpHandler.TokenResponse("token-2"),
            FakeHttpHandler.Text(HttpStatusCode.Unauthorized, ""));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() => svc.HttpGetAsync("/api/rest/v1/products", CT));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal(4, handler.CallCount);
    }

    // -------------------------------------------------------------------------
    // Finding 1.4 — token endpoint failures surface as AkeneoApiException
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TokenEndpointFailure_ThrowsAkeneoApiException_WithStatusAndBody()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.Json(HttpStatusCode.Unauthorized, """{"message":"Client not found"}"""));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() => svc.GetTokenAsync(ct: CT));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal("Client not found", ex.ApiMessage);
        Assert.Contains("/api/oauth/v1/token", ex.RequestUrl);
    }

    [Fact]
    public async Task TokenEndpointGarbageBody_ThrowsAkeneoApiException()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.Json(HttpStatusCode.OK, """{"unexpected":"shape"}"""));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() => svc.GetTokenAsync(ct: CT));

        Assert.Contains("unusable", ex.ApiMessage);
    }

    // -------------------------------------------------------------------------
    // Finding 1.20 — one shared token across DI-resolved services
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAkeneoClient_ResolvedServicesShareOneToken()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse("shared-token"));

        var services = new ServiceCollection();
        services.AddAkeneoClient(Helpers.Settings());
        services.AddHttpClient<AkeneoRestApiService>()
                .ConfigurePrimaryHttpMessageHandler(() => handler);

        using var provider = services.BuildServiceProvider();
        var svc1 = provider.GetRequiredService<IAkeneoRestApiService>();
        var svc2 = provider.GetRequiredService<IAkeneoRestApiService>();
        Assert.NotSame(svc1, svc2); // transient typed clients …

        var token1 = await svc1.GetTokenAsync(ct: CT);
        var token2 = await svc2.GetTokenAsync(ct: CT);

        // … but exactly one token request went over the wire.
        Assert.Equal("shared-token", token1);
        Assert.Equal(token1, token2);
        Assert.Equal(1, handler.CallCount);
    }

    // -------------------------------------------------------------------------
    // Finding 1.3 — token-file persistence: reload across services, corrupt file,
    // and write failures never fail the API call
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TokenFile_PersistedAndReloadedByFreshService()
    {
        var dir = Directory.CreateTempSubdirectory("oa-token-test");
        try
        {
            var path = Path.Combine(dir.FullName, "token-{0}.json");
            var settings = SettingsWithTokenFile(path);

            var svc1 = new AkeneoRestApiService(new HttpClient(new FakeHttpHandler(FakeHttpHandler.TokenResponse("persisted-token"))), settings);
            Assert.Equal("persisted-token", await svc1.GetTokenAsync(ct: CT));

            // Fresh service + empty handler queue: the only valid token source is the file.
            var emptyHandler = new FakeHttpHandler();
            var svc2 = new AkeneoRestApiService(new HttpClient(emptyHandler), settings);
            Assert.Equal("persisted-token", await svc2.GetTokenAsync(ct: CT));
            Assert.Equal(0, emptyHandler.CallCount);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task TokenFile_CorruptFile_FallsBackToFetch()
    {
        var dir = Directory.CreateTempSubdirectory("oa-token-test");
        try
        {
            var path = Path.Combine(dir.FullName, "token-{0}.json");
            File.WriteAllText(string.Format(path, "client-id"), "{not json");

            var svc = new AkeneoRestApiService(new HttpClient(new FakeHttpHandler(FakeHttpHandler.TokenResponse("fresh-token"))), SettingsWithTokenFile(path));
            Assert.Equal("fresh-token", await svc.GetTokenAsync(ct: CT));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task TokenFile_UnwritablePath_DoesNotFailTheCall()
    {
        var dir = Directory.CreateTempSubdirectory("oa-token-test");
        try
        {
            // TokenFilePath points at an existing DIRECTORY → the persist write fails,
            // but the token was acquired and the call must succeed anyway.
            var svc = new AkeneoRestApiService(new HttpClient(new FakeHttpHandler(FakeHttpHandler.TokenResponse("in-memory-token"))), SettingsWithTokenFile(dir.FullName));
            Assert.Equal("in-memory-token", await svc.GetTokenAsync(ct: CT));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    private static AkeneoRestApiSettings SettingsWithTokenFile(string tokenFilePath) => new()
    {
        ClientId = "client-id",
        ClientSecret = "client-secret",
        Username = "user",
        Password = "pass",
        RestApiUrl = "https://akeneo.test",
        TokenFilePath = tokenFilePath
    };

    // -------------------------------------------------------------------------
    // Finding 2.1 — media upload sends the spec-required product part
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UploadProductMediaFileAsync_SendsProductPart()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation("/api/rest/v1/media-files/1/a/2/b/1a2b_photo.jpg"));

        var ctx = Helpers.BuildContext(handler);
        var productJson = """{"identifier":"my-sku","attribute":"picture","scope":null,"locale":null}""";
        var code = await ctx.UploadProductMediaFileAsync("data"u8.ToArray(), "photo.jpg", "image/jpeg", productJson, ct: CT);

        var req = handler.LastApiRequest!;
        Assert.StartsWith("multipart/form-data", req.ContentType);
        Assert.Contains("name=product", req.Body);
        Assert.Contains("\"identifier\":\"my-sku\"", req.Body);
        Assert.Equal("1/a/2/b/1a2b_photo.jpg", code);
    }

    [Fact]
    public async Task UploadProductMediaFileAsync_SendsProductModelPart()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation("/api/rest/v1/media-files/1/a/2/b/1a2b_photo.jpg"));

        var ctx = Helpers.BuildContext(handler);
        var modelJson = """{"code":"my-model","attribute":"picture","scope":null,"locale":null}""";
        await ctx.UploadProductMediaFileAsync("data"u8.ToArray(), "photo.jpg", "image/jpeg", productModelJson: modelJson, ct: CT);

        Assert.Contains("name=product_model", handler.LastApiRequest!.Body);
    }

    [Fact]
    public async Task UploadProductMediaFileAsync_BothTargets_Throws()
    {
        var ctx = Helpers.BuildContext(new FakeHttpHandler());
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ctx.UploadProductMediaFileAsync("data"u8.ToArray(), "photo.jpg", "image/jpeg", "{}", "{}", CT));
    }

    // -------------------------------------------------------------------------
    // Finding 1.8 — server-generated UUID resolved from the Location header
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProductUuidAsync_NullUuid_ResolvesUuidFromLocation()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation("https://akeneo.test/api/rest/v1/products-uuid/gen-uuid-42"),
            FakeHttpHandler.Ok("""{"uuid":"gen-uuid-42","enabled":true}"""));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateProductUuidAsync(new ProductUuid { Uuid = null }, CT);

        Assert.Equal("gen-uuid-42", result.Uuid);
        Assert.EndsWith("/products-uuid/gen-uuid-42", handler.LastApiRequest!.RequestUri);
    }

    [Fact]
    public async Task CreateProductUuidAsync_NullUuid_NoLocation_ThrowsWithContext()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var ctx = Helpers.BuildContext(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() =>
            ctx.CreateProductUuidAsync(new ProductUuid { Uuid = null }, CT));

        Assert.Contains("Location", ex.ApiMessage);
    }

    // -------------------------------------------------------------------------
    // Finding 1.7 — asset media-file codes keep their '/' separators
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DownloadAssetMediaFileAsync_KeepsSlashesInCode()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok("binary"));

        var ctx = Helpers.BuildContext(handler);
        await ctx.DownloadAssetMediaFileAsync("a/b/2/f/abc_file.png", CT);

        // The old code escaped the whole code, producing a%2Fb%2F… — slashes must survive.
        var uri = handler.LastApiRequest!.RequestUri;
        Assert.Contains("/asset-media-files/a/b/2/f/abc_file.png", uri);
        Assert.DoesNotContain("%2F", uri);
    }

    // -------------------------------------------------------------------------
    // Finding 2.4 — DeleteAssetAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAssetAsync_DeletesCorrectUrl()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.NoContent());

        var ctx = Helpers.BuildContext(handler);
        await ctx.DeleteAssetAsync("packshots", "asset-1", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.EndsWith("/asset-families/packshots/assets/asset-1", req.RequestUri);
    }

    // -------------------------------------------------------------------------
    // Finding 2.2 — product streamers use search_after keyset pagination
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StreamProductUuidsAsync_UsesSearchAfterPagination_AndTerminates()
    {
        var page1 = """
            {"_links":{"next":{"href":"https://akeneo.test/api/rest/v1/products-uuid?pagination_type=search_after&limit=100&search_after=cursor-1"}},
             "_embedded":{"items":[{"uuid":"u1","enabled":true}]}}
            """;
        var page2 = """
            {"_links":{},"_embedded":{"items":[{"uuid":"u2","enabled":true}]}}
            """;
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(page1),
            FakeHttpHandler.Ok(page2));

        var ctx = Helpers.BuildContext(handler);
        var items = new List<ProductUuid>();
        await foreach (var p in ctx.StreamProductUuidsAsync(ct: CT))
            items.Add(p);

        Assert.Equal(["u1", "u2"], items.Select(x => x.Uuid));

        var apiRequests = handler.Captured.Where(r => !r.RequestUri.Contains("/oauth/")).ToList();
        Assert.All(apiRequests, r => Assert.Contains("pagination_type=search_after", r.RequestUri));
        Assert.All(apiRequests, r => Assert.DoesNotContain("page=", r.RequestUri));
        Assert.Contains("search_after=cursor-1", apiRequests[1].RequestUri);
    }

    // -------------------------------------------------------------------------
    // Finding 2.10 — StreamAssetFamiliesAsync exists and follows the cursor
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StreamAssetFamiliesAsync_FollowsCursorAndTerminates()
    {
        var page1 = """
            {"_links":{"next":{"href":"/api/rest/v1/asset-families?search_after=fam-cursor"}},
             "_embedded":{"items":[{"code":"fam1"}]}}
            """;
        var page2 = """{"_links":{},"_embedded":{"items":[{"code":"fam2"}]}}""";
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(page1),
            FakeHttpHandler.Ok(page2));

        var ctx = Helpers.BuildContext(handler);
        var codes = new List<string>();
        await foreach (var fam in ctx.StreamAssetFamiliesAsync(CT))
            codes.Add(fam.Code);

        Assert.Equal(["fam1", "fam2"], codes);
        Assert.Contains("search_after=fam-cursor", handler.LastApiRequest!.RequestUri);
    }

    // -------------------------------------------------------------------------
    // Finding 1.5 — GetStringData is culture-invariant
    // -------------------------------------------------------------------------

    [Fact]
    public void GetStringData_IsCultureInvariant()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var value = JsonSerializer.Deserialize<ProductValue>("""{"data": 12.5}""")!;
            Assert.Equal("12.5", value.GetStringData());
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    // -------------------------------------------------------------------------
    // Finding 1.12 — bool? converter accepts JSON numbers 0/1
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("""{"decimals_allowed":1}""", true)]
    [InlineData("""{"decimals_allowed":0}""", false)]
    [InlineData("""{"decimals_allowed":"1"}""", true)]
    [InlineData("""{"decimals_allowed":"false"}""", false)]
    [InlineData("""{"decimals_allowed":null}""", null)]
    public void NullableBoolConverter_AcceptsNumbersStringsAndNull(string json, bool? expected)
    {
        var attribute = JsonSerializer.Deserialize<AkeneoAttribute>(json)!;
        Assert.Equal(expected, attribute.DecimalsAllowed);
    }

    // -------------------------------------------------------------------------
    // Finding 1.13 — catalogs domain validates pagination like everyone else
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetCatalogListAsync_InvalidLimit_Throws()
    {
        var ctx = Helpers.BuildContext(new FakeHttpHandler());
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ctx.GetCatalogListAsync(1, 101, CT));
    }

    [Fact]
    public async Task GetCatalogProductUuidListAsync_InvalidLimit_Throws()
    {
        var ctx = Helpers.BuildContext(new FakeHttpHandler());
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ctx.GetCatalogProductUuidListAsync("cat-1", 0, null, CT));
    }

    // -------------------------------------------------------------------------
    // Finding 1.10 — the settings-based context disposes what it owns
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AkeneoContext_SettingsCtor_DisposeReleasesOwnedClient()
    {
        var ctx = new AkeneoContext(Helpers.Settings());
        ctx.Dispose();

        // The owned HttpClient is disposed → any use must fail with ObjectDisposedException.
        await Assert.ThrowsAsync<ObjectDisposedException>(() => ctx.Service.GetTokenAsync(ct: CT));
    }

    [Fact]
    public async Task AkeneoContext_ServiceCtor_DisposeLeavesServiceUsable()
    {
        var handler = new FakeHttpHandler(FakeHttpHandler.TokenResponse("still-alive"));
        var svc = Helpers.BuildService(handler);

        var ctx = new AkeneoContext(svc);
        ctx.Dispose();

        Assert.Equal("still-alive", await svc.GetTokenAsync(ct: CT));
    }
}

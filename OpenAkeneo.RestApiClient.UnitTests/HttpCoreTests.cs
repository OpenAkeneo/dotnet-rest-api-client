using System.Net;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// WP1 — HTTP core hardening (see test-plan.md). Covers the 401-refresh flow for the binary and
/// multipart request variants, token-file expiry, 408 retries, cancellation propagation, and
/// settings validation. The string-variant 401 flow, verb-aware retry policy, and token endpoint
/// failures are locked in <see cref="FableReviewFixTests"/>.
/// </summary>
public class HttpCoreTests
{
    private static readonly CancellationToken CT = TestContext.Current.CancellationToken;

    // -------------------------------------------------------------------------
    // 401 refresh — binary and multipart variants
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HttpGetBytesAsync_On401_RefreshesTokenAndRetries()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse("token-1"),
            FakeHttpHandler.Text(HttpStatusCode.Unauthorized, ""),
            FakeHttpHandler.TokenResponse("token-2"),
            FakeHttpHandler.Ok("binary-payload"));

        var svc = Helpers.BuildService(handler);
        var bytes = await svc.HttpGetBytesAsync("/api/rest/v1/media-files/a/b/file.png/download", CT);

        Assert.Equal("binary-payload"u8.ToArray(), bytes);
        Assert.Equal(2, handler.Captured.Count(r => r.RequestUri.Contains("/oauth/")));
    }

    [Fact]
    public async Task HttpGetBytesAsync_On401Twice_ThrowsAkeneoApiException()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse("token-1"),
            FakeHttpHandler.Text(HttpStatusCode.Unauthorized, ""),
            FakeHttpHandler.TokenResponse("token-2"),
            FakeHttpHandler.Text(HttpStatusCode.Unauthorized, ""));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() =>
            svc.HttpGetBytesAsync("/api/rest/v1/media-files/x/download", CT));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal(4, handler.CallCount);
    }

    [Fact]
    public async Task HttpPostMultipartAsync_On401_RefreshesTokenAndRetries()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse("token-1"),
            FakeHttpHandler.Text(HttpStatusCode.Unauthorized, ""),
            FakeHttpHandler.TokenResponse("token-2"),
            FakeHttpHandler.CreatedWithLocation("/api/rest/v1/asset-media-files/a/b/img.png"));

        var svc = Helpers.BuildService(handler);
        var code = await svc.HttpPostMultipartAsync(
            "/api/rest/v1/asset-media-files", "file", "data"u8.ToArray(), "img.png", "image/png", ct: CT);

        Assert.Equal("a/b/img.png", code);
        Assert.Equal(2, handler.Captured.Count(r => r.RequestUri.Contains("/oauth/")));
        // The multipart body must have been rebuilt and re-sent on the retry.
        Assert.StartsWith("multipart/form-data", handler.LastApiRequest!.ContentType);
    }

    // -------------------------------------------------------------------------
    // Token file — expired persisted token triggers a fresh fetch
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TokenFile_ExpiredToken_TriggersFreshFetch()
    {
        var dir = Directory.CreateTempSubdirectory("oa-token-test");
        try
        {
            var pathTemplate = Path.Combine(dir.FullName, "token-{0}.json");
            File.WriteAllText(string.Format(pathTemplate, "client-id"),
                """{"access_token":"stale-token","expires_in":3600,"expires_at":"2020-01-01T00:00:00+00:00"}""");

            var handler = new FakeHttpHandler(FakeHttpHandler.TokenResponse("fresh-token"));
            var settings = new AkeneoRestApiSettings
            {
                ClientId = "client-id",
                ClientSecret = "client-secret",
                Username = "user",
                Password = "pass",
                RestApiUrl = "https://akeneo.test",
                TokenFilePath = pathTemplate
            };

            var svc = new AkeneoRestApiService(new HttpClient(handler), settings);
            Assert.Equal("fresh-token", await svc.GetTokenAsync(ct: CT));
            Assert.Equal(1, handler.CallCount);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    // -------------------------------------------------------------------------
    // Retry — 408 is retried for every verb (server did not process the request)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    public async Task RequestTimeout408_IsRetried_ForAllVerbs(string verb)
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.RequestTimeout, ""),
            FakeHttpHandler.Ok("""{"ok":true}"""));

        var svc = Helpers.BuildService(handler);
        var result = verb == "GET"
            ? await svc.HttpGetAsync("/api/rest/v1/products", CT)
            : await svc.HttpPostAsync("/api/rest/v1/products", "{}", CT);

        Assert.Contains("ok", result);
        Assert.Equal(3, handler.CallCount);
    }

    // -------------------------------------------------------------------------
    // Cancellation propagates out of the retry back-off
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Cancellation_DuringRetryBackoff_Propagates()
    {
        // Six 429s would take ≥ 500 ms of back-off; cancel after 100 ms.
        var responses = new List<HttpResponseMessage> { FakeHttpHandler.TokenResponse() };
        for (var i = 0; i < 6; i++)
            responses.Add(FakeHttpHandler.Text(HttpStatusCode.TooManyRequests, ""));

        var handler = new FakeHttpHandler(responses.ToArray());
        var svc = Helpers.BuildService(handler);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(CT);
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            svc.HttpGetAsync("/api/rest/v1/products", cts.Token));
    }

    // -------------------------------------------------------------------------
    // Settings validation
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(null, "secret", "user", "pass", "https://x")]
    [InlineData("id", null, "user", "pass", "https://x")]
    [InlineData("id", "secret", null, "pass", "https://x")]
    [InlineData("id", "secret", "user", null, "https://x")]
    [InlineData("id", "secret", "user", "pass", null)]
    [InlineData("id", "secret", "user", "pass", "   ")]
    public void Validate_MissingRequiredField_Throws(string? clientId, string? secret, string? user, string? pass, string? url)
    {
        var settings = new AkeneoRestApiSettings
        {
            ClientId = clientId!,
            ClientSecret = secret!,
            Username = user!,
            Password = pass!,
            RestApiUrl = url!
        };

        Assert.ThrowsAny<ArgumentException>(settings.Validate);
    }

    [Fact]
    public void Validate_AllFieldsPresent_DoesNotThrow()
    {
        Helpers.Settings().Validate();
    }
}

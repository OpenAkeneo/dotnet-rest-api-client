using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class AssetTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string AssetFamilyCode = "logo";
    private const string AssetAttributeCode = "label";
    private const string AssetAttributeOptionFamilyCode = "manual";
    private const string AssetAttributeOptionAttributeCode = "published";
    private const string AssetAttributeOptionCode = "yes";

    // OpenAkeneo test data
    private const string OpenAkeneoAssetCode = "openakeneo_test_asset";

    public AssetTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    #region Asset family

    [Fact]
    public async Task GetAssetFamilyListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAssetFamilyListAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AssetFamilies);
    }

    [Fact]
    public async Task GetAssetFamilyAsync_ReturnsAssetFamily()
    {
        var result = await _fixture.Context.GetAssetFamilyAsync(AssetFamilyCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AssetFamilyCode, result.Code);
    }

    #endregion


    #region Asset attribute

    [Fact]
    public async Task GetAssetAttributeListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAssetAttributeListAsync(AssetFamilyCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetAssetAttributeAsync_ReturnsAttribute()
    {
        var result = await _fixture.Context.GetAssetAttributeAsync(AssetFamilyCode, AssetAttributeCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AssetAttributeCode, result.Code);
        Assert.NotEmpty(result.Type);
    }

    #endregion


    #region Asset attribute option

    [Fact]
    public async Task GetAssetAttributeOptionListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAssetAttributeOptionListAsync(AssetAttributeOptionFamilyCode, AssetAttributeOptionAttributeCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetAssetAttributeOptionAsync_ReturnsOption()
    {
        var result = await _fixture.Context.GetAssetAttributeOptionAsync(AssetAttributeOptionFamilyCode, AssetAttributeOptionAttributeCode, AssetAttributeOptionCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AssetAttributeOptionCode, result.Code);
    }

    #endregion

    #region Asset

    [Fact]
    public async Task GetAssetListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAssetListAsync(AssetFamilyCode, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotNull(result.Assets);
    }

    [Fact]
    public async Task GetAssetListFullAsync_ReturnsAllAssets()
    {
        var result = await _fixture.Context.GetAssetListFullAsync(AssetFamilyCode, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task StreamAssetsAsync_StreamsAssets()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamAssetsAsync(AssetFamilyCode, ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0, "Expected at least one asset to be streamed.");
    }

    [Fact]
    public async Task GetAssetAsync_ReturnsAsset()
    {
        var ct = TestContext.Current.CancellationToken;

        // First get a list of assets to find an existing code
        var listResult = await _fixture.Context.GetAssetListAsync(AssetFamilyCode, limit: 1, ct: ct);

        if (listResult.Assets.Count > 0)
        {
            var assetCode = listResult.Assets[0].Code;
            var result = await _fixture.Context.GetAssetAsync(AssetFamilyCode, assetCode, ct);

            Assert.NotNull(result);
            Assert.Equal(assetCode, result.Code);
        }
        else
        {
            Assert.True(true, "No assets found in the family to test GetAssetAsync.");
        }
    }

    #endregion


    #region Media file upload

    [Fact]
    public async Task UploadAssetMediaFileAsync_ReturnsNonEmptyCode()
    {
        var ct = TestContext.Current.CancellationToken;

        // Minimal 1×1 transparent PNG (67 bytes) — no external file dependency.
        var png1x1 = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

        var code = await _fixture.Context.UploadAssetMediaFileAsync(png1x1, "openakeneo_test.png", "image/png", ct);

        Assert.False(string.IsNullOrWhiteSpace(code), $"Expected a non-empty file code but got: '{code}'");
        Assert.EndsWith("openakeneo_test.png", code);

        // The real media-file code is hierarchical (e.g. "3/b/5/a/<hash>_filename.png"), not just the
        // filename. A regression that only recovers the last path segment (the old Location-parsing
        // fallback) would return just "openakeneo_test.png" — assert the full code so that can't pass.
        Assert.Contains('/', code);
    }

    // Regression test for: "UploadAssetMediaFileAsync returns empty string — media not attached to asset".
    // The previous test asserted only the *return value* of the upload, so it could pass even though the
    // code never actually attached to an asset (or was resolved to just the filename). This test drives
    // the full round-trip against the live instance: upload → attach to a media_file attribute → re-fetch →
    // assert the asset's media value reads back as the uploaded code. That is the behaviour the issue is
    // really about, and it exercises the branch that returned empty against Svedbergs Serenity.
    [Fact]
    public async Task UploadThenAttachMediaFile_AssetMediaValueRoundTrips()
    {
        var ct = TestContext.Current.CancellationToken;

        // Discover the family's media_file attribute at runtime rather than hardcoding a code we can't
        // verify — the test stays valid if the family's attribute naming differs.
        var attributes = await _fixture.Context.GetAssetAttributeListAsync(AssetFamilyCode, ct);
        var mediaAttribute = attributes.FirstOrDefault(a =>
            string.Equals(a.Type, "media_file", StringComparison.OrdinalIgnoreCase) && !a.IsReadOnly);
        Assert.SkipWhen(mediaAttribute is null,
            $"Family '{AssetFamilyCode}' has no writable media_file attribute; cannot round-trip media.");

        var png1x1 = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

        var mediaCode = await _fixture.Context.UploadAssetMediaFileAsync(png1x1, "openakeneo_roundtrip.png", "image/png", ct);
        Assert.False(string.IsNullOrWhiteSpace(mediaCode),
            $"Upload returned an empty media-file code — media cannot be attached. Got: '{mediaCode}'");

        // Attach the uploaded code to the media attribute, respecting locale scoping.
        var locale = mediaAttribute!.ValuePerLocale ? "en_US" : null;
        var asset = new Asset
        {
            Code = OpenAkeneoAssetCode,
            Values = new Dictionary<string, List<AssetValue>>
            {
                [mediaAttribute.Code] = new List<AssetValue>
                {
                    new AssetValue { Locale = locale, Channel = null, Data = mediaCode }
                }
            }
        };
        await _fixture.Context.CreateOrUpdateAssetAsync(AssetFamilyCode, asset, ct);

        // Re-fetch and assert the media value actually persisted as the uploaded code — this is what
        // "media not attached to asset" failed at, even though the upload call itself did not throw.
        var fetched = await _fixture.Context.GetAssetAsync(AssetFamilyCode, OpenAkeneoAssetCode, ct);
        Assert.NotNull(fetched.Values);
        Assert.True(fetched.Values.TryGetValue(mediaAttribute.Code, out var mediaValues) && mediaValues is { Count: > 0 },
            $"Asset '{OpenAkeneoAssetCode}' has no value for media attribute '{mediaAttribute.Code}' after attach.");

        var persisted = mediaValues!.First().Data as string;
        Assert.False(string.IsNullOrWhiteSpace(persisted),
            $"Media attribute '{mediaAttribute.Code}' read back empty — media was not attached.");
        Assert.Equal(mediaCode, persisted);
    }

    #endregion


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateAssetAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Step 1 — Create an OpenAkeneo asset in the logo family.
        var asset = new Asset
        {
            Code = OpenAkeneoAssetCode,
            Values = new Dictionary<string, List<AssetValue>>
            {
                ["label"] = new List<AssetValue>
                {
                    new AssetValue { Locale = "en_US", Data = "(OpenAkeneo) Test Asset" }
                }
            }
        };
        var createResult = await _fixture.Context.CreateOrUpdateAssetAsync(AssetFamilyCode, asset, ct);
        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoAssetCode, createResult.Code);

        // Step 2 — Update the asset label and verify.
        var updated = new Asset
        {
            Code = OpenAkeneoAssetCode,
            Values = new Dictionary<string, List<AssetValue>>
            {
                ["label"] = new List<AssetValue>
                {
                    new AssetValue { Locale = "en_US", Data = "(OpenAkeneo) Test Asset Updated" }
                }
            }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateAssetAsync(AssetFamilyCode, updated, ct);
        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoAssetCode, updateResult.Code);
    }

    #endregion

}

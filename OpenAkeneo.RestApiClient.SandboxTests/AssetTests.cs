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

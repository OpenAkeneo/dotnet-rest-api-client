using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

/// <summary>
/// Extends TestBase by seeding the reference entity, attribute, option, and record
/// that the read-only tests expect to find in the sandbox.
/// </summary>
public class ReferenceEntityFixture : TestBase
{
    internal const string ReferenceEntityCode = "openakeneo_testing";
    internal const string AttributeCode = "single_option";
    internal const string AttributeOptionCode = "1";
    internal const string RecordCode = "testing_1";
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        // Seed reference entity
        await Context.CreateOrUpdateReferenceEntityAsync(new ReferenceEntity
        {
            Code = ReferenceEntityCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "OpenAkeneo Testing" }
        });

        // Seed single_option attribute
        await Context.CreateOrUpdateReferenceEntityAttributeAsync(ReferenceEntityCode, new ReferenceEntityAttribute
        {
            Code = AttributeCode,
            Type = "single_option",
            ValuePerLocale = false,
            ValuePerChannel = false
        });

        // Seed option "1" on that attribute
        await Context.CreateOrUpdateReferenceEntityAttributeOptionAsync(ReferenceEntityCode, AttributeCode, new ReferenceEntityAttributeOption
        {
            Code = AttributeOptionCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "Option 1" }
        });

        // Seed record
        await Context.CreateOrUpdateReferenceEntityRecordAsync(ReferenceEntityCode, new ReferenceEntityRecord
        {
            Code = RecordCode
        });
    }
}

public class ReferenceEntityTests : IClassFixture<ReferenceEntityFixture>
{
    private readonly ReferenceEntityFixture _fixture;

    // Test data — seeded by ReferenceEntityFixture
    private const string ReferenceEntityCode = ReferenceEntityFixture.ReferenceEntityCode;
    private const string AttributeCode = ReferenceEntityFixture.AttributeCode;
    private const string AttributeOptionCode = ReferenceEntityFixture.AttributeOptionCode;
    private const string RecordCode = ReferenceEntityFixture.RecordCode;
    private const string MediaFileCode = "f/f/c/f/ffcf299bae0e4aeb0b85ea232722cf2a5efea125_Test_Image_01.png";

    // Pre-existing sandbox data — "logo" reference entity with an asset_collection attribute
    private const string LogoReferenceEntityCode = "logo";
    private const string LogoAssetCollectionAttributeCode = "image_asset";
    private const string LogoAssetFamilyCode = "logo";
    private const string LogoRecordCode = "hp";
    private const string LogoAssetCode = "HP";

    // OpenAkeneo test data
    private const string OpenAkeneoReferenceEntityCode = "openakeneo_test_ref_entity";
    private const string OpenAkeneoRecordCode = "openakeneo_test_record";

    public ReferenceEntityTests(ReferenceEntityFixture fixture)
    {
        _fixture = fixture;
    }


    #region Reference entity

    [Fact]
    public async Task GetReferenceEntityListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetReferenceEntityListAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.ReferenceEntities);
    }

    [Fact]
    public async Task GetReferenceEntityAsync_ReturnsEntity()
    {
        var result = await _fixture.Context.GetReferenceEntityAsync(ReferenceEntityCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(ReferenceEntityCode, result.Code);
    }

    #endregion


    #region Reference entity attribute

    [Fact]
    public async Task GetReferenceEntityAttributeListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetReferenceEntityAttributeListAsync(ReferenceEntityCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetReferenceEntityAttributeAsync_ReturnsAttribute()
    {
        var result = await _fixture.Context.GetReferenceEntityAttributeAsync(ReferenceEntityCode, AttributeCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AttributeCode, result.Code);
        Assert.NotEmpty(result.Type);
    }

    [Fact]
    public async Task GetReferenceEntityAttributeAsync_AssetCollection_ReturnsReferenceDataName()
    {
        var result = await _fixture.Context.GetReferenceEntityAttributeAsync(LogoReferenceEntityCode, LogoAssetCollectionAttributeCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(LogoAssetCollectionAttributeCode, result.Code);
        Assert.Equal("asset_collection", result.Type);
        Assert.Equal(LogoAssetFamilyCode, result.AssetFamilyIdentifier);
    }

    [Fact]
    public async Task GetReferenceEntityRecordAsync_AssetCollection_RecordValueContainsAssetCode()
    {
        var result = await _fixture.Context.GetReferenceEntityRecordAsync(LogoReferenceEntityCode, LogoRecordCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(LogoRecordCode, result.Code);
        var assetCodes = result.Values?[LogoAssetCollectionAttributeCode]
            .SelectMany(v => v.GetData<List<string>>() ?? new List<string>())
            .ToList();
        Assert.NotNull(assetCodes);
        Assert.Contains(LogoAssetCode, assetCodes);
    }

    #endregion


    #region Reference entity attribute option

    [Fact]
    public async Task GetReferenceEntityAttributeOptionListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetReferenceEntityAttributeOptionListAsync(ReferenceEntityCode, AttributeCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetReferenceEntityAttributeOptionAsync_ReturnsOption()
    {
        var result = await _fixture.Context.GetReferenceEntityAttributeOptionAsync(ReferenceEntityCode, AttributeCode, AttributeOptionCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AttributeOptionCode, result.Code);
    }

    #endregion


    #region Reference entity record

    [Fact]
    public async Task GetReferenceEntityRecordListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetReferenceEntityRecordListAsync(ReferenceEntityCode, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.ReferenceEntityRecords);
    }

    [Fact]
    public async Task GetReferenceEntityRecordAsync_ReturnsRecord()
    {
        var result = await _fixture.Context.GetReferenceEntityRecordAsync(ReferenceEntityCode, RecordCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(RecordCode, result.Code);
        Assert.Equal(ReferenceEntityCode, result.ReferenceEntityCode);
    }

    #endregion


    #region Reference entity media file

    [Fact]
    public async Task DownloadReferenceEntityMediaFileAsync_ReturnsBytes()
    {
        var result = await _fixture.Context.DownloadReferenceEntityMediaFileAsync(MediaFileCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion


    #region Write operations — lifecycle tests

    [Fact]
    public async Task CreateOrUpdateReferenceEntityAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Step 1 — Create the OpenAkeneo reference entity.
        var entity = new ReferenceEntity
        {
            Code = OpenAkeneoReferenceEntityCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Reference Entity" }
        };
        var createResult = await _fixture.Context.CreateOrUpdateReferenceEntityAsync(entity, ct);
        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoReferenceEntityCode, createResult.Code);
        Assert.Equal("(OpenAkeneo) Test Reference Entity", createResult.Labels?["en_US"]);

        // Step 2 — Update the label and verify.
        var updated = new ReferenceEntity
        {
            Code = OpenAkeneoReferenceEntityCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Reference Entity Updated" }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateReferenceEntityAsync(updated, ct);
        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoReferenceEntityCode, updateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Reference Entity Updated", updateResult.Labels?["en_US"]);
    }

    [Fact]
    public async Task CreateOrUpdateReferenceEntityRecordAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Step 1 — Ensure the OpenAkeneo reference entity exists.
        var entity = new ReferenceEntity
        {
            Code = OpenAkeneoReferenceEntityCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Reference Entity" }
        };
        await _fixture.Context.CreateOrUpdateReferenceEntityAsync(entity, ct);

        // Step 2 — Create a record under the OpenAkeneo reference entity.
        var record = new ReferenceEntityRecord
        {
            Code = OpenAkeneoRecordCode,
            Values = new Dictionary<string, List<ReferenceEntityRecordValue>>
            {
                ["label"] = new List<ReferenceEntityRecordValue>
                {
                    new ReferenceEntityRecordValue { Locale = "en_US", Data = "(OpenAkeneo) Test Record" }
                }
            }
        };
        var createResult = await _fixture.Context.CreateOrUpdateReferenceEntityRecordAsync(OpenAkeneoReferenceEntityCode, record, ct);
        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoRecordCode, createResult.Code);

        // Step 3 — Update the record label and verify.
        var updated = new ReferenceEntityRecord
        {
            Code = OpenAkeneoRecordCode,
            Values = new Dictionary<string, List<ReferenceEntityRecordValue>>
            {
                ["label"] = new List<ReferenceEntityRecordValue>
                {
                    new ReferenceEntityRecordValue { Locale = "en_US", Data = "(OpenAkeneo) Test Record Updated" }
                }
            }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateReferenceEntityRecordAsync(OpenAkeneoReferenceEntityCode, updated, ct);
        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoRecordCode, updateResult.Code);
    }

    #endregion
}

using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class FamilyVariantTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string FamilyCode = "clothing";
    private const string VariantCode = "clothing_color_size";

    // OpenAkeneo test data — uses existing clothing family so variant axes are valid
    private const string OpenAkeneoVariantCode = "openakeneo_test_variant";

    public FamilyVariantTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetFamilyVariantListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetFamilyVariantListAsync(FamilyCode, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.FamilyVariants);
    }

    [Fact]
    public async Task GetFamilyVariantListFullAsync_ReturnsAllVariants()
    {
        var result = await _fixture.Context.GetFamilyVariantListFullAsync(FamilyCode, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamFamilyVariantsAsync_StreamsVariants()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamFamilyVariantsAsync(FamilyCode, ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetFamilyVariantAsync_ReturnsVariant()
    {
        var result = await _fixture.Context.GetFamilyVariantAsync(FamilyCode, VariantCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(VariantCode, result.Code);
        Assert.NotEmpty(result.VariantAttributeSets);
    }


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateFamilyVariantAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Step 1 — Create the OpenAkeneo variant under the clothing family.
        var variant = new FamilyVariant
        {
            Code = OpenAkeneoVariantCode,
            VariantAttributeSets = new List<FamilyVariantAttributeSet>
            {
                new FamilyVariantAttributeSet
                {
                    Level = 1,
                    Axes = new List<string> { "color" },
                    Attributes = new List<string> { "color" }
                }
            },
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Variant" }
        };
        var createResult = await _fixture.Context.CreateOrUpdateFamilyVariantAsync(FamilyCode, variant, ct);
        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoVariantCode, createResult.Code);
        Assert.Equal("(OpenAkeneo) Test Variant", createResult.Labels?["en_US"]);

        // Step 2 — Update the label and verify.
        var updated = new FamilyVariant
        {
            Code = OpenAkeneoVariantCode,
            VariantAttributeSets = new List<FamilyVariantAttributeSet>
            {
                new FamilyVariantAttributeSet
                {
                    Level = 1,
                    Axes = new List<string> { "color" },
                    Attributes = new List<string> { "color" }
                }
            },
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Variant Updated" }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateFamilyVariantAsync(FamilyCode, updated, ct);
        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoVariantCode, updateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Variant Updated", updateResult.Labels?["en_US"]);
    }

    #endregion
}

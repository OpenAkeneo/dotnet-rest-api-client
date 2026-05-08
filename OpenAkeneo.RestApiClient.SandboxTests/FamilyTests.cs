using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class FamilyTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string FamilyCode = "digital_cameras";

    // OpenAkeneo test data
    private const string OpenAkeneoFamilyCode = "openakeneo_test_family";

    public FamilyTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    #region Family

    [Fact]
    public async Task GetFamilyListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetFamilyListAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Families);
    }

    [Fact]
    public async Task GetFamilyListAsync_WithPageAndLimit_ReturnsCorrectCount()
    {
        var result = await _fixture.Context.GetFamilyListAsync(page: 1, limit: 5, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.Families.Count <= 5);
    }

    [Fact]
    public async Task GetFamilyListFullAsync_ReturnsAllFamilies()
    {
        var result = await _fixture.Context.GetFamilyListFullAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamFamiliesAsync_StreamsFamilies()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamFamiliesAsync(ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetFamilyAsync_ReturnsFamily()
    {
        var result = await _fixture.Context.GetFamilyAsync(FamilyCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(FamilyCode, result.Code);
        Assert.NotNull(result.Attributes);
        Assert.NotEmpty(result.Attributes);
    }

    #endregion


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateFamilyAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Step 1 — Create the OpenAkeneo family.
        var family = new Family
        {
            Code = OpenAkeneoFamilyCode,
            AttributeAsLabel = "sku",
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Family" }
        };
        var createResult = await _fixture.Context.CreateOrUpdateFamilyAsync(family, ct);
        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoFamilyCode, createResult.Code);
        Assert.Equal("(OpenAkeneo) Test Family", createResult.Labels?["en_US"]);

        // Step 2 — Update the label and verify.
        var updated = new Family
        {
            Code = OpenAkeneoFamilyCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Family Updated" }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateFamilyAsync(updated, ct);
        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoFamilyCode, updateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Family Updated", updateResult.Labels?["en_US"]);
    }

    #endregion
}

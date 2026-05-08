using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class AttributeOptionTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string AttributeCode = "accessories_closure_type";
    private const string OptionCode = "buckle_fastening_ankle_strap";

    // OpenAkeneo test data — dedicated attribute so option tests don't mutate real attributes.
    private const string OpenAkeneoAttributeCode = "openakeneo_test_select_attribute";
    private const string OpenAkeneoTestOptionCode = "openakeneo_test_option";

    public AttributeOptionTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetAttributeOptionListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAttributeOptionListAsync(AttributeCode, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AttributeOptions);
    }

    [Fact]
    public async Task GetAttributeOptionListFullAsync_ReturnsAllOptions()
    {
        var result = await _fixture.Context.GetAttributeOptionListFullAsync(AttributeCode, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamAttributeOptionsAsync_StreamsOptions()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamAttributeOptionsAsync(AttributeCode, ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetAttributeOptionAsync_ReturnsOption()
    {
        var result = await _fixture.Context.GetAttributeOptionAsync(AttributeCode, OptionCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(OptionCode, result.Code);
        Assert.Equal(AttributeCode, result.Attribute);
    }


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateAttributeOptionAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Step 1 — Create a dedicated OpenAkeneo simple-select attribute to own the test options.
        var attribute = new Models.AkeneoAttribute
        {
            Code = OpenAkeneoAttributeCode,
            Type = "pim_catalog_simpleselect",
            Group = "other",
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Select Attribute" }
        };
        var attributeResult = await _fixture.Context.CreateOrUpdateAttributeAsync(attribute, ct);
        Assert.NotNull(attributeResult);
        Assert.Equal(OpenAkeneoAttributeCode, attributeResult.Code);
        Assert.Equal("pim_catalog_simpleselect", attributeResult.Type);
        Assert.Equal("(OpenAkeneo) Test Select Attribute", attributeResult.Labels?["en_US"]);

        // Step 2 — Create an option under the OpenAkeneo attribute.
        var created = new AttributeOption
        {
            Code = OpenAkeneoTestOptionCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Option" }
        };
        var createResult = await _fixture.Context.CreateOrUpdateAttributeOptionAsync(OpenAkeneoAttributeCode, created, ct);
        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoTestOptionCode, createResult.Code);
        Assert.Equal(OpenAkeneoAttributeCode, createResult.Attribute);
        Assert.Equal("(OpenAkeneo) Test Option", createResult.Labels?["en_US"]);

        // Step 3 — Update the attribute label and verify.
        var attributeUpdated = new Models.AkeneoAttribute
        {
            Code = OpenAkeneoAttributeCode,
            Type = "pim_catalog_simpleselect",
            Group = "other",
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Select Attribute Updated" }
        };
        var attributeUpdateResult = await _fixture.Context.CreateOrUpdateAttributeAsync(attributeUpdated, ct);
        Assert.NotNull(attributeUpdateResult);
        Assert.Equal(OpenAkeneoAttributeCode, attributeUpdateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Select Attribute Updated", attributeUpdateResult.Labels?["en_US"]);

        // Step 4 — Update the option label and verify.
        var updated = new AttributeOption
        {
            Code = OpenAkeneoTestOptionCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Option Updated" }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateAttributeOptionAsync(OpenAkeneoAttributeCode, updated, ct);
        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoTestOptionCode, updateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Option Updated", updateResult.Labels?["en_US"]);
    }

    #endregion
}

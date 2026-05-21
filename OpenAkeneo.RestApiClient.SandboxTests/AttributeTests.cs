namespace OpenAkeneo.RestApiClient.SandboxTests;

public class AttributeTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string AttributeCode = "accessories_care_instructions";

    // Dedicated test attribute — prefixed so it's clearly test data on the tenant.
    private const string OpenAkeneoAttributeCode = "openakeneo_test_text_attribute";

    public AttributeTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetAttributeListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAttributeListAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Attributes);
    }

    [Fact]
    public async Task GetAttributeListAsync_WithPageAndLimit_ReturnsCorrectCount()
    {
        var result = await _fixture.Context.GetAttributeListAsync(page: 1, limit: 5, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.Attributes.Count <= 5);
    }

    [Fact]
    public async Task GetAttributeListFullAsync_ReturnsAllAttributes()
    {
        var result = await _fixture.Context.GetAttributeListFullAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamAttributesAsync_StreamsAttributes()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamAttributesAsync(ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetAttributeAsync_ReturnsAttribute()
    {
        var result = await _fixture.Context.GetAttributeAsync(AttributeCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AttributeCode, result.Code);
        Assert.NotNull(result.Type);
        Assert.NotEmpty(result.Type);
        Assert.NotNull(result.Group);
        Assert.NotEmpty(result.Group);
    }


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateAttributeAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Step 1 — Create (or overwrite) a dedicated text attribute.
        var attribute = new Models.AkeneoAttribute
        {
            Code = OpenAkeneoAttributeCode,
            Type = "pim_catalog_text",
            Group = "other",
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Text Attribute" }
        };
        var createResult = await _fixture.Context.CreateOrUpdateAttributeAsync(attribute, ct);
        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoAttributeCode, createResult.Code);
        Assert.Equal("pim_catalog_text", createResult.Type);
        Assert.Equal("(OpenAkeneo) Test Text Attribute", createResult.Labels?["en_US"]);

        // Step 2 — Verify the attribute is retrievable via GET.
        var fetched = await _fixture.Context.GetAttributeAsync(OpenAkeneoAttributeCode, ct);
        Assert.NotNull(fetched);
        Assert.Equal(OpenAkeneoAttributeCode, fetched.Code);
        Assert.Equal("pim_catalog_text", fetched.Type);
        Assert.Equal("(OpenAkeneo) Test Text Attribute", fetched.Labels?["en_US"]);

        // Step 3 — Update the label and verify the change is reflected.
        var updated = new Models.AkeneoAttribute
        {
            Code = OpenAkeneoAttributeCode,
            Type = "pim_catalog_text",
            Group = "other",
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Text Attribute Updated" }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateAttributeAsync(updated, ct);
        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoAttributeCode, updateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Text Attribute Updated", updateResult.Labels?["en_US"]);

        // Step 4 — Confirm the update persisted by fetching again.
        var fetchedAfterUpdate = await _fixture.Context.GetAttributeAsync(OpenAkeneoAttributeCode, ct);
        Assert.Equal("(OpenAkeneo) Test Text Attribute Updated", fetchedAfterUpdate.Labels?["en_US"]);
    }

    #endregion
}

using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class AttributeGroupTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string AttributeGroupCode = "weight_dimensions";
    private const string OpenAkeneoTestAttributeGroupCode = "openakeneo_test_attribute_group";

    public AttributeGroupTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetAttributeGroupListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAttributeGroupListAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result.AttributeGroups);
    }

    [Fact]
    public async Task GetAttributeGroupListFullAsync_ReturnsAllGroups()
    {
        var result = await _fixture.Context.GetAttributeGroupListFullAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamAttributeGroupsAsync_StreamsGroups()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamAttributeGroupsAsync())
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetAttributeGroupAsync_ReturnsGroup()
    {
        var result = await _fixture.Context.GetAttributeGroupAsync(AttributeGroupCode);

        Assert.NotNull(result);
        Assert.Equal(AttributeGroupCode, result.Code);
        Assert.NotEmpty(result.Attributes);
    }


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateAttributeGroupAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        // Step 1 — Create the attribute group.
        var created = new AttributeGroup
        {
            Code = OpenAkeneoTestAttributeGroupCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Attribute Group" }
        };
        var createResult = await _fixture.Context.CreateOrUpdateAttributeGroupAsync(created);

        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoTestAttributeGroupCode, createResult.Code);
        Assert.Equal("(OpenAkeneo) Test Attribute Group", createResult.Labels?["en_US"]);

        // Step 2 — Patch the label and verify.
        var updated = new AttributeGroup
        {
            Code = OpenAkeneoTestAttributeGroupCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Attribute Group Updated" }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateAttributeGroupAsync(updated);

        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoTestAttributeGroupCode, updateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Attribute Group Updated", updateResult.Labels?["en_US"]);
    }

    #endregion
}

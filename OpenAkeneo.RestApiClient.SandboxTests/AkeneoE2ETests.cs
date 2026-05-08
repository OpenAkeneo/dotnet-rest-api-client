using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class AkeneoE2ETests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    // OpenAkeneo test data
    private const string OpenAkeneoRootCategoryCode = "openakeneo_root";
    private const string OpenAkeneoTestCategoryCode = "openakeneo_test_category";
    private const string OpenAkeneoTestChannelCode = "openakeneo_test_channel";
    private const string OpenAkeneoTestAttributeCode = "openakeneo_test_attribute";
    private const string OpenAkeneoTestAttributeGroupCode = "openakeneo_test_attribute_group";
    private const string OpenAkeneoTestFamilyCode = "openakeneo_test_family";
    private const string OpenAkeneoTestProductCode = "openakeneo_test_product";

    public AkeneoE2ETests(TestBase fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateOrUpdate_FullCatalog_OpenAkeneo_E2E_Test()
    {
        var ct = TestContext.Current.CancellationToken;

        // 1. Create root category
        var root = new Category
        {
            Code = OpenAkeneoRootCategoryCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Root" }
        };
        var rootResult = await _fixture.Context.CreateOrUpdateCategoryAsync(root, ct);
        Assert.NotNull(rootResult);

        // 2. Create child category
        var child = new Category
        {
            Code = OpenAkeneoTestCategoryCode,
            Parent = OpenAkeneoRootCategoryCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Category" }
        };
        var childResult = await _fixture.Context.CreateOrUpdateCategoryAsync(child, ct);
        Assert.NotNull(childResult);
        Assert.Equal(OpenAkeneoRootCategoryCode, childResult.Parent);

        // 3. Create channel linking to the root category tree
        var channel = new Channel
        {
            Code = OpenAkeneoTestChannelCode,
            CategoryTree = OpenAkeneoRootCategoryCode,
            Currencies = new List<string> { "USD", "EUR" },
            Locales = new List<string> { "en_US" },
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Channel" }
        };
        var channelResult = await _fixture.Context.CreateOrUpdateChannelAsync(channel, ct);
        Assert.NotNull(channelResult);
        Assert.Equal(OpenAkeneoTestChannelCode, channelResult.Code);

        // 4. Create attribute group
        var attributeGroup = new AttributeGroup
        {
            Code = OpenAkeneoTestAttributeGroupCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Attribute Group" }
        };
        var groupResult = await _fixture.Context.CreateOrUpdateAttributeGroupAsync(attributeGroup, ct);
        Assert.NotNull(groupResult);

        // 5. Create attribute
        var attribute = new OpenAkeneo.RestApiClient.Models.AkeneoAttribute
        {
            Code = OpenAkeneoTestAttributeCode,
            Type = "pim_catalog_text",
            Group = OpenAkeneoTestAttributeGroupCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Attribute" }
        };
        var attributeResult = await _fixture.Context.CreateOrUpdateAttributeAsync(attribute, ct);
        Assert.NotNull(attributeResult);
        Assert.Equal(OpenAkeneoTestAttributeGroupCode, attributeResult.Group);

        // 6. Create family linking to the created attribute
        var family = new Family
        {
            Code = OpenAkeneoTestFamilyCode,
            Attributes = new List<string> { "sku", OpenAkeneoTestAttributeCode },
            AttributeAsLabel = "sku",
            AttributeRequirements = new Dictionary<string, List<string>>
            {
                [OpenAkeneoTestChannelCode] = new List<string> { "sku", OpenAkeneoTestAttributeCode }
            },
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Family" }
        };
        var familyResult = await _fixture.Context.CreateOrUpdateFamilyAsync(family, ct);
        Assert.NotNull(familyResult);
        Assert.Contains(OpenAkeneoTestAttributeCode, familyResult.Attributes);

        // 7. Create product linking to the family, category, and using the attribute
        var product = new ProductIdentifier
        {
            Identifier = OpenAkeneoTestProductCode,
            Family = OpenAkeneoTestFamilyCode,
            Categories = new List<string> { OpenAkeneoTestCategoryCode },
            Enabled = true,
            Values = new Dictionary<string, List<ProductValue>>
            {
                [OpenAkeneoTestAttributeCode] = new List<ProductValue>
                {
                    new ProductValue { Data = "(OpenAkeneo) Product Data Value", Locale = null, Scope = null }
                }
            }
        };
        var productResult = await _fixture.Context.CreateOrUpdateProductIdentifierAsync(product, ct);
        Assert.NotNull(productResult);
        Assert.Equal(OpenAkeneoTestFamilyCode, productResult.Family);
        Assert.Contains(OpenAkeneoTestCategoryCode, productResult.Categories);
        Assert.NotNull(productResult.Values);
        Assert.True(productResult.Values.ContainsKey(OpenAkeneoTestAttributeCode));
    }
}

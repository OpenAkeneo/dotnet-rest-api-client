using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class CatalogTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    public CatalogTests(TestBase fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetCatalogListAsync_ReturnsListOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetCatalogListAsync(limit: 5);
            Assert.NotNull(result);
            Assert.NotNull(result.Catalogs);
        }
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("NotFound") || ex.Message.Contains("Unauthorized"))
        {
            Assert.Skip($"Catalog for Apps feature might not be enabled or authorized with current token: {ex.Message}");
        }
    }

    [Fact]
    public async Task GetCatalogAsync_ProductUuidList_ReturnsOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetCatalogListAsync(limit: 1);
            if (result.Catalogs != null && result.Catalogs.Count > 0)
            {
                var catalogId = result.Catalogs[0].Id;
                if (!string.IsNullOrEmpty(catalogId))
                {
                    var uuids = await _fixture.Context.GetCatalogProductUuidListAsync(catalogId);
                    Assert.NotNull(uuids);
                }
            }
        }
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("NotFound") || ex.Message.Contains("Unauthorized"))
        {
            Assert.Skip($"Catalog for Apps feature might not be enabled or authorized with current token: {ex.Message}");
        }
    }

    [Fact]
    public async Task GetCatalogMappedProductListAsync_ReturnsStringOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetCatalogListAsync(limit: 1);
            if (result.Catalogs != null && result.Catalogs.Count > 0)
            {
                var catalogId = result.Catalogs[0].Id;
                if (!string.IsNullOrEmpty(catalogId))
                {
                    var mappedProducts = await _fixture.Context.GetCatalogMappedProductListAsync(catalogId);
                    Assert.NotNull(mappedProducts);
                    Assert.NotEmpty(mappedProducts);
                }
            }
        }
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("NotFound") || ex.Message.Contains("Unauthorized"))
        {
            Assert.Skip($"Catalog for Apps feature might not be enabled or authorized with current token: {ex.Message}");
        }
    }
}

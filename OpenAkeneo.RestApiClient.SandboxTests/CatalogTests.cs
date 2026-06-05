using System.Text.Json;
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
            var result = await _fixture.Context.GetCatalogListAsync(limit: 5, ct: TestContext.Current.CancellationToken);
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
        var ct = TestContext.Current.CancellationToken;
        try
        {
            var result = await _fixture.Context.GetCatalogListAsync(limit: 1, ct: ct);
            if (result.Catalogs != null && result.Catalogs.Count > 0)
            {
                var catalogId = result.Catalogs[0].Id;
                if (!string.IsNullOrEmpty(catalogId))
                {
                    var uuids = await _fixture.Context.GetCatalogProductUuidListAsync(catalogId, ct: ct);
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
        var ct = TestContext.Current.CancellationToken;
        try
        {
            var result = await _fixture.Context.GetCatalogListAsync(limit: 1, ct: ct);
            if (result.Catalogs != null && result.Catalogs.Count > 0)
            {
                var catalogId = result.Catalogs[0].Id;
                if (!string.IsNullOrEmpty(catalogId))
                {
                    var mappedProducts = await _fixture.Context.GetCatalogMappedProductListAsync(catalogId, ct: ct);
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

    [Fact]
    public async Task GetCatalogMappedVariantListAsync_ReturnsStringOrForbidden()
    {
        var ct = TestContext.Current.CancellationToken;
        try
        {
            var catalogs = await _fixture.Context.GetCatalogListAsync(limit: 1, ct: ct);
            var catalogId = catalogs.Catalogs?.FirstOrDefault()?.Id;
            if (string.IsNullOrEmpty(catalogId))
                Assert.Skip("No catalog available to test mapped variants.");

            // The mapped-variants endpoint is scoped to a model code, so first resolve a model
            // code from the catalog's mapped models list.
            var mappedModelsJson = await _fixture.Context.GetCatalogMappedModelListAsync(catalogId, ct: ct);
            var modelCode = ExtractFirstItemCode(mappedModelsJson);
            if (string.IsNullOrEmpty(modelCode))
                Assert.Skip("No mapped model available to test mapped variants.");

            var mappedVariants = await _fixture.Context.GetCatalogMappedVariantListAsync(catalogId, modelCode, ct: ct);
            Assert.NotNull(mappedVariants);
        }
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("NotFound") || ex.Message.Contains("Unauthorized"))
        {
            Assert.Skip($"Catalog for Apps feature might not be enabled or authorized with current token: {ex.Message}");
        }
    }

    /// <summary>Extracts the first embedded item's <c>code</c> from a HAL-style mapped list JSON payload.</summary>
    private static string? ExtractFirstItemCode(string halJson)
    {
        using var doc = JsonDocument.Parse(halJson);
        if (!doc.RootElement.TryGetProperty("_embedded", out var embedded) ||
            !embedded.TryGetProperty("items", out var items) ||
            items.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var item in items.EnumerateArray())
            if (item.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String)
                return code.GetString();

        return null;
    }
}

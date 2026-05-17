using System.Net;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// Unit tests for all endpoints added in the coverage gap implementation.
/// Each test verifies:
///   1. The correct HTTP method and URL are sent to the server.
///   2. The response is correctly deserialized (or the method completes without throwing).
/// </summary>
public class NewEndpointTests
{
    private static readonly CancellationToken CT = TestContext.Current.CancellationToken;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string ProductUuidJson(string uuid = "abc-123") =>
        $$$"""{"uuid":"{{{uuid}}}","enabled":true,"family":null,"categories":[],"groups":[],"parent":null,"values":{}}""";

    private static string ProductIdentifierJson(string identifier = "SKU-001") =>
        $$$"""{"identifier":"{{{identifier}}}","enabled":true,"family":null,"categories":[],"groups":[],"parent":null,"values":{}}""";

    private static string ProductModelJson(string code = "model-001") =>
        $$$"""{"code":"{{{code}}}","family":"clothes","family_variant":"clothes_size","categories":[],"values":{}}""";

    private static string AttributeJson(string code = "attr_001") =>
        $$$"""{"code":"{{{code}}}","type":"pim_catalog_text","group":"other","unique":false,"useable_as_grid_filter":false,"allowed_extensions":[],"metric_family":null,"default_metric_unit":null,"reference_data_name":null,"available_locales":[],"max_characters":null,"validation_rule":null,"validation_regexp":null,"wysiwyg_enabled":null,"number_min":null,"number_max":null,"decimals_allowed":null,"negative_allowed":null,"date_min":null,"date_max":null,"max_file_size":null,"minimum_input_length":null,"sort_order":0,"localizable":false,"scopable":false,"labels":{}}""";

    private static string AttributeOptionJson(string code = "opt_001") =>
        $$$"""{"code":"{{{code}}}","attribute":"color","sort_order":1,"labels":{}}""";

    private static string AttributeGroupJson(string code = "grp_001") =>
        $$$"""{"code":"{{{code}}}","sort_order":1,"attributes":[],"labels":{}}""";

    private static string AssociationTypeJson(string code = "UPSELL") =>
        $$$"""{"code":"{{{code}}}","labels":{},"is_quantified":false,"is_two_way":false}""";

    private static string FamilyJson(string code = "clothing") =>
        $$$"""{"code":"{{{code}}}","attributes":[],"labels":{}}""";

    private static string FamilyVariantJson(string code = "clothing_size") =>
        $$$"""{"code":"{{{code}}}","labels":{},"variant_attribute_sets":[]}""";

    private static string CategoryJson(string code = "master") =>
        $$$"""{"code":"{{{code}}}","parent":null,"labels":{}}""";

    private static string ChannelJson(string code = "ecommerce") =>
        $$$"""{"code":"{{{code}}}","currencies":[],"locales":[],"category_tree":"master","labels":{}}""";

    private static string CatalogJson(string id = "cat-uuid-001") =>
        $$$"""{"id":"{{{id}}}","name":"My Catalog","enabled":true}""";

    // -------------------------------------------------------------------------
    // Service-layer: HttpPutAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HttpPutAsync_SendsPutWithBody()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok("{}"));

        var svc = Helpers.BuildService(handler);
        await svc.HttpPutAsync("/api/rest/v1/test", """{"key":"value"}""", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Put, req.Method);
        Assert.Contains("/api/rest/v1/test", req.RequestUri);
        Assert.Contains("\"key\"", req.Body);
        Assert.Equal("application/json", req.ContentType);
    }

    // -------------------------------------------------------------------------
    // Service-layer: HttpPostMultipartAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HttpPostMultipartAsync_SendsMultipartPost()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var svc = Helpers.BuildService(handler);
        var bytes = "fake-image-data"u8.ToArray();
        await svc.HttpPostMultipartAsync("/api/rest/v1/media-files", "file", bytes, "photo.jpg", "image/jpeg", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/rest/v1/media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
    }

    // -------------------------------------------------------------------------
    // Products UUID – create, delete, proposal, search
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProductUuidAsync_PostsToProductsUuid_ReturnsProduct()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(ProductUuidJson("new-uuid")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateProductUuidAsync(new ProductUuid { Uuid = "new-uuid" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/products-uuid", req.RequestUri);
        Assert.Equal("new-uuid", result.Uuid);
    }

    [Fact]
    public async Task DeleteProductUuidAsync_DeletesCorrectUrl()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.NoContent());

        var ctx = Helpers.BuildContext(handler);
        await ctx.DeleteProductUuidAsync("abc-123", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.EndsWith("/products-uuid/abc-123", req.RequestUri);
    }

    [Fact]
    public async Task SubmitProductUuidProposalAsync_PostsToProposalEndpoint()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var ctx = Helpers.BuildContext(handler);
        await ctx.SubmitProductUuidProposalAsync("abc-123", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/products-uuid/abc-123/proposal", req.RequestUri);
    }

    [Fact]
    public async Task SearchProductUuidsAsync_PostsToSearchEndpoint_ReturnsList()
    {
        var responseJson = Helpers.HalList(ProductUuidJson("found-uuid"));
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(responseJson));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.SearchProductUuidsAsync("""{"search":{"enabled":[{"operator":"=","value":true}]}}""", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/products-uuid/search", req.RequestUri);
        Assert.Single(result.Products);
        Assert.Equal("found-uuid", result.Products[0].Uuid);
    }

    // -------------------------------------------------------------------------
    // Products identifier – create, delete, proposal
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProductIdentifierAsync_PostsToProducts_ReturnsProduct()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(ProductIdentifierJson("SKU-NEW")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateProductIdentifierAsync(new ProductIdentifier { Identifier = "SKU-NEW" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/products", req.RequestUri);
        Assert.Equal("SKU-NEW", result.Identifier);
    }

    [Fact]
    public async Task DeleteProductIdentifierAsync_DeletesCorrectUrl()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.NoContent());

        var ctx = Helpers.BuildContext(handler);
        await ctx.DeleteProductIdentifierAsync("SKU-001", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.EndsWith("/products/SKU-001", req.RequestUri);
    }

    [Fact]
    public async Task SubmitProductIdentifierProposalAsync_PostsToProposalEndpoint()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var ctx = Helpers.BuildContext(handler);
        await ctx.SubmitProductIdentifierProposalAsync("SKU-001", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/products/SKU-001/proposal", req.RequestUri);
    }

    // -------------------------------------------------------------------------
    // Product models – create, delete, proposal
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProductModelAsync_PostsToProductModels_ReturnsModel()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(ProductModelJson("model-new")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateProductModelAsync(new ProductModel { Code = "model-new", Family = "clothes", FamilyVariant = "clothes_size" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/product-models", req.RequestUri);
        Assert.Equal("model-new", result.Code);
    }

    [Fact]
    public async Task DeleteProductModelAsync_DeletesCorrectUrl()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.NoContent());

        var ctx = Helpers.BuildContext(handler);
        await ctx.DeleteProductModelAsync("model-001", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.EndsWith("/product-models/model-001", req.RequestUri);
    }

    [Fact]
    public async Task SubmitProductModelProposalAsync_PostsToProposalEndpoint()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var ctx = Helpers.BuildContext(handler);
        await ctx.SubmitProductModelProposalAsync("model-001", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/product-models/model-001/proposal", req.RequestUri);
    }

    // -------------------------------------------------------------------------
    // Product media file upload
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UploadProductMediaFileAsync_PostsMultipartToMediaFiles()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var ctx = Helpers.BuildContext(handler);
        await ctx.UploadProductMediaFileAsync("data"u8.ToArray(), "photo.jpg", "image/jpeg", ct: CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
    }

    // -------------------------------------------------------------------------
    // Attributes
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAttributeAsync_PostsToAttributes_ReturnsAttribute()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(AttributeJson("new_attr")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateAttributeAsync(new AkeneoAttribute { Code = "new_attr", Type = "pim_catalog_text", Group = "other" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/attributes", req.RequestUri);
        Assert.Equal("new_attr", result.Code);
    }

    // -------------------------------------------------------------------------
    // Attribute options
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAttributeOptionAsync_PostsToAttributeOptions_ReturnsOption()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(AttributeOptionJson("red")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateAttributeOptionAsync("color", new AttributeOption { Code = "red" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/attributes/color/options", req.RequestUri);
        Assert.Equal("red", result.Code);
    }

    // -------------------------------------------------------------------------
    // Attribute groups
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAttributeGroupAsync_PostsToAttributeGroups_ReturnsGroup()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(AttributeGroupJson("marketing")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateAttributeGroupAsync(new AttributeGroup { Code = "marketing" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/attribute-groups", req.RequestUri);
        Assert.Equal("marketing", result.Code);
    }

    // -------------------------------------------------------------------------
    // Association types
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAssociationTypeAsync_PostsToAssociationTypes_ReturnsType()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(AssociationTypeJson("UPSELL")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateAssociationTypeAsync(new AssociationType { Code = "UPSELL" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/association-types", req.RequestUri);
        Assert.Equal("UPSELL", result.Code);
    }

    // -------------------------------------------------------------------------
    // Families
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateFamilyAsync_PostsToFamilies_ReturnsFamily()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(FamilyJson("shoes")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateFamilyAsync(new Family { Code = "shoes" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/families", req.RequestUri);
        Assert.Equal("shoes", result.Code);
    }

    // -------------------------------------------------------------------------
    // Family variants
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateFamilyVariantAsync_PostsToFamilyVariants_ReturnsVariant()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(FamilyVariantJson("shoes_size")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateFamilyVariantAsync("shoes", new FamilyVariant { Code = "shoes_size" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/families/shoes/variants", req.RequestUri);
        Assert.Equal("shoes_size", result.Code);
    }

    // -------------------------------------------------------------------------
    // Categories
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateCategoryAsync_PostsToCategories_ReturnsCategory()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(CategoryJson("summer")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateCategoryAsync(new Category { Code = "summer" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/categories", req.RequestUri);
        Assert.Equal("summer", result.Code);
    }

    [Fact]
    public async Task UploadCategoryMediaFileAsync_PostsMultipartToCategoryMediaFiles()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var ctx = Helpers.BuildContext(handler);
        await ctx.UploadCategoryMediaFileAsync("data"u8.ToArray(), "banner.jpg", "image/jpeg", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/category-media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
    }

    // -------------------------------------------------------------------------
    // Channels
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateChannelAsync_PostsToChannels_ReturnsChannel()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(ChannelJson("b2b")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateChannelAsync(new Channel { Code = "b2b" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/channels", req.RequestUri);
        Assert.Equal("b2b", result.Code);
    }

    // -------------------------------------------------------------------------
    // Measurement families
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateOrUpdateMeasurementFamiliesAsync_PatchesAndReturnsBody()
    {
        var responseBody = """[{"status_code":201}]""";
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(responseBody));

        var ctx = Helpers.BuildContext(handler);
        var families = new List<MeasurementFamily>
        {
            new() { Code = "Weight", StandardUnitCode = "KILOGRAM", Labels = new() { ["en_US"] = "Weight" } }
        };
        var result = await ctx.CreateOrUpdateMeasurementFamiliesAsync(families, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Patch, req.Method);
        Assert.EndsWith("/measurement-families", req.RequestUri);
        Assert.Contains("Weight", req.Body);
        Assert.Equal(responseBody, result);
    }

    // -------------------------------------------------------------------------
    // Catalogs
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateCatalogAsync_PostsToCatalogs_ReturnsCatalog()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(CatalogJson("new-cat-id")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateCatalogAsync(new Catalog { Name = "New Catalog" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/catalogs", req.RequestUri);
        Assert.Equal("new-cat-id", result.Id);
    }

    [Fact]
    public async Task UpdateCatalogAsync_PatchesCatalog_ReturnsCatalog()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.NoContent(),                  // PATCH returns 204
            FakeHttpHandler.Ok(CatalogJson("cat-uuid-001"))); // GET re-fetch

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.UpdateCatalogAsync("cat-uuid-001", new Catalog { Name = "Updated" }, CT);

        var captured = handler.Captured.Where(r => !r.RequestUri.Contains("/oauth/")).ToList();
        Assert.Equal(HttpMethod.Patch, captured[0].Method);
        Assert.EndsWith("/catalogs/cat-uuid-001", captured[0].RequestUri);
        Assert.Equal("cat-uuid-001", result.Id);
    }

    [Fact]
    public async Task DeleteCatalogAsync_DeletesCorrectUrl()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.NoContent());

        var ctx = Helpers.BuildContext(handler);
        await ctx.DeleteCatalogAsync("cat-uuid-001", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.EndsWith("/catalogs/cat-uuid-001", req.RequestUri);
    }

    [Fact]
    public async Task DuplicateCatalogAsync_PostsToDuplicateEndpoint_ReturnsCatalog()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(CatalogJson("dup-cat-id")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.DuplicateCatalogAsync("cat-uuid-001", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/catalogs/cat-uuid-001/duplicate", req.RequestUri);
        Assert.Equal("dup-cat-id", result.Id);
    }

    [Fact]
    public async Task SetCatalogMappingSchemaAsync_PutsToMappingSchemaEndpoint()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(""));

        var ctx = Helpers.BuildContext(handler);
        var schema = """{"type":"object","properties":{"name":{"type":"string"}}}""";
        await ctx.SetCatalogMappingSchemaAsync("cat-uuid-001", schema, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Put, req.Method);
        Assert.EndsWith("/catalogs/cat-uuid-001/mapping-schemas/product", req.RequestUri);
        Assert.Contains("\"properties\"", req.Body);
    }

    [Fact]
    public async Task DeleteCatalogMappingSchemaAsync_DeletesMappingSchemaEndpoint()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.NoContent());

        var ctx = Helpers.BuildContext(handler);
        await ctx.DeleteCatalogMappingSchemaAsync("cat-uuid-001", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.EndsWith("/catalogs/cat-uuid-001/mapping-schemas/product", req.RequestUri);
    }

    // -------------------------------------------------------------------------
    // Asset media file upload
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UploadAssetMediaFileAsync_PostsMultipartToAssetMediaFiles()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var ctx = Helpers.BuildContext(handler);
        await ctx.UploadAssetMediaFileAsync("data"u8.ToArray(), "image.jpg", "image/jpeg", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/asset-media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
    }

    // -------------------------------------------------------------------------
    // Reference entity media file upload
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UploadReferenceEntityMediaFileAsync_PostsMultipartToReferenceEntityMediaFiles()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""));

        var ctx = Helpers.BuildContext(handler);
        await ctx.UploadReferenceEntityMediaFileAsync("data"u8.ToArray(), "portrait.jpg", "image/jpeg", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/reference-entities-media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
    }

    // -------------------------------------------------------------------------
    // URL structure: codes are placed in the correct path segment
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteProductUuidAsync_PlacesUuidInCorrectPathSegment()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.NoContent());

        var ctx = Helpers.BuildContext(handler);
        await ctx.DeleteProductUuidAsync("abc-123", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Delete, req.Method);
        Assert.EndsWith("/products-uuid/abc-123", req.RequestUri);
    }

    [Fact]
    public async Task CreateFamilyVariantAsync_PlacesFamilyCodeAndVariantCodeInCorrectPath()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(FamilyVariantJson("v1")));

        var ctx = Helpers.BuildContext(handler);
        await ctx.CreateFamilyVariantAsync("shoes", new FamilyVariant { Code = "v1" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Contains("/families/shoes/variants", req.RequestUri);
    }

    // -------------------------------------------------------------------------
    // Request body content: JSON serialization spot-checks
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProductUuidAsync_SerializesProductAsJson()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(ProductUuidJson("p1")));

        var ctx = Helpers.BuildContext(handler);
        await ctx.CreateProductUuidAsync(new ProductUuid { Uuid = "p1", Enabled = true, Family = "clothes" }, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal("application/json", req.ContentType);
        Assert.Contains("\"uuid\"", req.Body);
        Assert.Contains("\"family\"", req.Body);
    }

    [Fact]
    public async Task SetCatalogMappingSchemaAsync_SendsExactSchemaString()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(""));

        var ctx = Helpers.BuildContext(handler);
        const string schema = """{"$schema":"https://json-schema.org/draft-07/schema"}""";
        await ctx.SetCatalogMappingSchemaAsync("c1", schema, CT);

        Assert.Equal(schema, handler.LastApiRequest!.Body);
    }
}

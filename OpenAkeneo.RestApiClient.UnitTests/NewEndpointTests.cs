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
    public async Task HttpPostMultipartAsync_SendsMultipartPost_ReturnsCodeFromLocationHeader()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation("/api/rest/v1/media-files/3/b/5/a/3b5a8cphoto.jpg"));

        var svc = Helpers.BuildService(handler);
        var bytes = "fake-image-data"u8.ToArray();
        var code = await svc.HttpPostMultipartAsync("/api/rest/v1/media-files", "file", bytes, "photo.jpg", "image/jpeg", ct: CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/rest/v1/media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
        Assert.Equal("3/b/5/a/3b5a8cphoto.jpg", code);
    }

    // Regression: the 'asset-media-file-code' response header is the code source on Akeneo Serenity
    // (SaaS). Earlier the client read only Location and returned empty when this header was the carrier.
    [Fact]
    public async Task HttpPostMultipartAsync_ReturnsCodeFromAssetMediaFileCodeHeader()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithMediaFileCodeHeader("3/c/d/6/3cd6a399_image.png"));

        var svc = Helpers.BuildService(handler);
        var code = await svc.HttpPostMultipartAsync(
            "/api/rest/v1/asset-media-files", "file", "data"u8.ToArray(), "image.png", "image/png", ct: CT);

        Assert.Equal("3/c/d/6/3cd6a399_image.png", code);
    }

    // Regression: Akeneo's spec documents an ABSOLUTE Location URI. The code must be recovered from the
    // path tail regardless of host — the earlier relative-only prefix match failed on this shape.
    [Fact]
    public async Task HttpPostMultipartAsync_ReturnsCodeFromAbsoluteLocationHeader()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation(
                "https://demo.akeneo.com/api/rest/v1/asset-media-files/3/c/d/6/3cd6a399_image.png"));

        var svc = Helpers.BuildService(handler);
        var code = await svc.HttpPostMultipartAsync(
            "/api/rest/v1/asset-media-files", "file", "data"u8.ToArray(), "image.png", "image/png", ct: CT);

        Assert.Equal("3/c/d/6/3cd6a399_image.png", code);
    }

    // Regression: a 201 that carries neither a code header nor a parseable Location must fail loudly —
    // this is the exact production condition that previously returned "" and attached empty media.
    [Fact]
    public async Task HttpPostMultipartAsync_ThrowsWhenNoCodeResolvable()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithoutCode());

        var svc = Helpers.BuildService(handler);

        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() =>
            svc.HttpPostMultipartAsync(
                "/api/rest/v1/asset-media-files", "file", "data"u8.ToArray(), "image.png", "image/png", ct: CT));
        Assert.Equal(HttpStatusCode.Created, ex.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Products UUID – create, delete, proposal, search
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProductUuidAsync_PostsToProductsUuid_ReturnsProduct()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""),                          // POST → 201 empty body (real Akeneo behaviour)
            FakeHttpHandler.Ok(ProductUuidJson("new-uuid")));     // GET → product

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateProductUuidAsync(new ProductUuid { Uuid = "new-uuid" }, CT);

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
            FakeHttpHandler.Created(""),                              // POST → 201 empty body
            FakeHttpHandler.Ok(ProductIdentifierJson("SKU-NEW")));   // GET → product

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateProductIdentifierAsync(new ProductIdentifier { Identifier = "SKU-NEW" }, CT);

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
            FakeHttpHandler.Created(""),                           // POST → 201 empty body
            FakeHttpHandler.Ok(ProductModelJson("model-new")));   // GET → model

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateProductModelAsync(new ProductModel { Code = "model-new", Family = "clothes", FamilyVariant = "clothes_size" }, CT);

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
    public async Task UploadProductMediaFileAsync_PostsMultipartToMediaFiles_ReturnsCodeFromLocationHeader()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation("/api/rest/v1/media-files/3/b/5/a/3b5a8cphoto.jpg"));

        var ctx = Helpers.BuildContext(handler);
        var code = await ctx.UploadProductMediaFileAsync("data"u8.ToArray(), "photo.jpg", "image/jpeg", ct: CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
        Assert.Equal("3/b/5/a/3b5a8cphoto.jpg", code);
    }

    // -------------------------------------------------------------------------
    // Attributes
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAttributeAsync_PostsToAttributes_ReturnsAttribute()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""),                        // POST → 201 empty body (real Akeneo behaviour)
            FakeHttpHandler.Ok(AttributeJson("new_attr")));     // GET → created entity

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateAttributeAsync(new AkeneoAttribute { Code = "new_attr", Type = "pim_catalog_text", Group = "other" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.EndsWith("/attributes", post.RequestUri);
        Assert.EndsWith("/attributes/new_attr", handler.LastApiRequest!.RequestUri);
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
            FakeHttpHandler.Created(""),
            FakeHttpHandler.Ok(AttributeOptionJson("red")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateAttributeOptionAsync("color", new AttributeOption { Code = "red" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.EndsWith("/attributes/color/options", post.RequestUri);
        Assert.EndsWith("/attributes/color/options/red", handler.LastApiRequest!.RequestUri);
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
            FakeHttpHandler.Created(""),
            FakeHttpHandler.Ok(AttributeGroupJson("marketing")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateAttributeGroupAsync(new AttributeGroup { Code = "marketing" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.EndsWith("/attribute-groups", post.RequestUri);
        Assert.EndsWith("/attribute-groups/marketing", handler.LastApiRequest!.RequestUri);
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
            FakeHttpHandler.Created(""),
            FakeHttpHandler.Ok(AssociationTypeJson("UPSELL")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateAssociationTypeAsync(new AssociationType { Code = "UPSELL" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.EndsWith("/association-types", post.RequestUri);
        Assert.EndsWith("/association-types/UPSELL", handler.LastApiRequest!.RequestUri);
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
            FakeHttpHandler.Created(""),
            FakeHttpHandler.Ok(FamilyJson("shoes")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateFamilyAsync(new Family { Code = "shoes" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.EndsWith("/families", post.RequestUri);
        Assert.EndsWith("/families/shoes", handler.LastApiRequest!.RequestUri);
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
            FakeHttpHandler.Created(""),
            FakeHttpHandler.Ok(FamilyVariantJson("shoes_size")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateFamilyVariantAsync("shoes", new FamilyVariant { Code = "shoes_size" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.EndsWith("/families/shoes/variants", post.RequestUri);
        Assert.EndsWith("/families/shoes/variants/shoes_size", handler.LastApiRequest!.RequestUri);
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
            FakeHttpHandler.Created(""),
            FakeHttpHandler.Ok(CategoryJson("summer")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateCategoryAsync(new Category { Code = "summer" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.EndsWith("/categories", post.RequestUri);
        Assert.Contains("/categories/summer", handler.LastApiRequest!.RequestUri);
        Assert.Equal("summer", result.Code);
    }

    [Fact]
    public async Task UploadCategoryMediaFileAsync_PostsMultipartToCategoryMediaFiles()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation("/api/rest/v1/media-files/3/b/5/a/3b5a8cbanner.jpg"));

        var ctx = Helpers.BuildContext(handler);
        var categoryJson = """{"code":"summer","attribute_code":"image_1","channel":null,"locale":null}""";
        var code = await ctx.UploadCategoryMediaFileAsync("data"u8.ToArray(), "banner.jpg", "image/jpeg", categoryJson, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/category-media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
        // The spec-required "category" part must be in the multipart body.
        Assert.Contains("name=category", req.Body);
        Assert.Contains("\"attribute_code\":\"image_1\"", req.Body);
        Assert.Equal("3/b/5/a/3b5a8cbanner.jpg", code);
    }

    // -------------------------------------------------------------------------
    // Channels
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateChannelAsync_PostsToChannels_ReturnsChannel()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""),
            FakeHttpHandler.Ok(ChannelJson("b2b")));

        var ctx = Helpers.BuildContext(handler);
        var result = await ctx.CreateChannelAsync(new Channel { Code = "b2b" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.EndsWith("/channels", post.RequestUri);
        Assert.EndsWith("/channels/b2b", handler.LastApiRequest!.RequestUri);
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
    public async Task UploadAssetMediaFileAsync_PostsMultipartToAssetMediaFiles_ReturnsCodeFromLocationHeader()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation("/api/rest/v1/asset-media-files/3/b/5/a/3b5a8cimage.jpg"));

        var ctx = Helpers.BuildContext(handler);
        var code = await ctx.UploadAssetMediaFileAsync("data"u8.ToArray(), "image.jpg", "image/jpeg", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/asset-media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
        Assert.Equal("3/b/5/a/3b5a8cimage.jpg", code);
    }

    // -------------------------------------------------------------------------
    // Reference entity media file upload
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UploadReferenceEntityMediaFileAsync_PostsMultipartToReferenceEntityMediaFiles()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.CreatedWithLocation("/api/rest/v1/reference-entities-media-files/3/b/5/a/3b5a8cportrait.jpg"));

        var ctx = Helpers.BuildContext(handler);
        var code = await ctx.UploadReferenceEntityMediaFileAsync("data"u8.ToArray(), "portrait.jpg", "image/jpeg", CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/reference-entities-media-files", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
        Assert.Equal("3/b/5/a/3b5a8cportrait.jpg", code);
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
            FakeHttpHandler.Created(""),
            FakeHttpHandler.Ok(FamilyVariantJson("v1")));

        var ctx = Helpers.BuildContext(handler);
        await ctx.CreateFamilyVariantAsync("shoes", new FamilyVariant { Code = "v1" }, CT);

        var post = handler.Captured.First(r => r.Method == HttpMethod.Post && !r.RequestUri.Contains("/oauth/"));
        Assert.Contains("/families/shoes/variants", post.RequestUri);
    }

    // -------------------------------------------------------------------------
    // Request body content: JSON serialization spot-checks
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProductUuidAsync_SerializesProductAsJson()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Created(""),                       // POST → 201 empty body
            FakeHttpHandler.Ok(ProductUuidJson("p1")));        // GET → product

        var ctx = Helpers.BuildContext(handler);
        await ctx.CreateProductUuidAsync(new ProductUuid { Uuid = "p1", Enabled = true, Family = "clothes" }, CT);

        // Inspect the POST request (second request, index 1 after token)
        var postReq = handler.Captured[1];
        Assert.Equal("application/json", postReq.ContentType);
        Assert.Contains("\"uuid\"", postReq.Body);
        Assert.Contains("\"family\"", postReq.Body);
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

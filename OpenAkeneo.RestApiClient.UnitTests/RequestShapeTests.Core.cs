using System.Net;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// WP3 — request-shape and loop-guard tests for the core domains (products, attributes,
/// families, categories, channels/locales/currencies/measurement-families, association types).
/// Every test asserts the outgoing request (method, path, query) and, for streamers, that the
/// pagination loop terminates (a runaway loop drains the FakeHttpHandler queue and fails loudly).
/// </summary>
public class RequestShapeTestsCore
{
    private static readonly CancellationToken CT = TestContext.Current.CancellationToken;

    private static FakeHttpHandler Handler(params HttpResponseMessage[] responses)
        => new([FakeHttpHandler.TokenResponse(), .. responses]);

    /// <summary>HAL page with N empty items and an optional next link.</summary>
    private static HttpResponseMessage HalPage(int items = 1, string? nextHref = null)
    {
        var itemsJson = string.Join(",", Enumerable.Repeat("{}", items));
        var links = nextHref is null ? "{}" : $$$"""{"next":{"href":"{{{nextHref}}}"}}""";
        return FakeHttpHandler.Ok($$$"""{"_links":{{{links}}},"_embedded":{"items":[{{{itemsJson}}}]}}""");
    }

    private static string UrlOf(FakeHttpHandler handler, int apiCallIndex = 0)
        => handler.Captured.Where(r => !r.RequestUri.Contains("/oauth/")).ElementAt(apiCallIndex).RequestUri;

    // -------------------------------------------------------------------------
    // Products — typed list params, gets, drafts, CreateOrUpdate, media files
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProductUuidListAsync_BuildsTypedQueryParameters()
    {
        var handler = Handler(HalPage());
        await Helpers.BuildContext(handler).GetProductUuidListAsync(2, 50, """{"enabled":[]}""", "ecommerce", "en_US,de_DE", withAssetShareLinks: true, ct: CT);

        var url = UrlOf(handler);
        Assert.Contains("/api/rest/v1/products-uuid?", url);
        Assert.Contains("page=2", url);
        Assert.Contains("limit=50", url);
        Assert.Contains("search=", url);
        Assert.Contains("scope=ecommerce", url);
        Assert.Contains("locales=en_US%2Cde_DE", url);
        Assert.Contains("with_asset_share_links=true", url);
    }

    [Fact]
    public async Task GetProductUuidAsync_And_Draft_UseExpectedPaths()
    {
        var handler = Handler(FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetProductUuidAsync("uuid-1", CT);
        await ctx.GetProductUuidDraftAsync("uuid-1", CT);

        Assert.EndsWith("/products-uuid/uuid-1", UrlOf(handler, 0));
        Assert.EndsWith("/products-uuid/uuid-1/draft", UrlOf(handler, 1));
    }

    [Fact]
    public async Task CreateOrUpdateProductUuidAsync_PatchesThenFetches()
    {
        var handler = Handler(FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("""{"uuid":"u1"}"""));
        var result = await Helpers.BuildContext(handler).CreateOrUpdateProductUuidAsync(new ProductUuid { Uuid = "u1" }, CT);

        var patch = handler.Captured.First(r => r.Method == HttpMethod.Patch);
        Assert.EndsWith("/products-uuid/u1", patch.RequestUri);
        Assert.Contains("\"uuid\":\"u1\"", patch.Body);
        Assert.Equal(HttpMethod.Get, handler.Captured.Last().Method);
        Assert.Equal("u1", result.Uuid);
    }

    [Fact]
    public async Task CreateOrUpdateProductIdentifierAsync_PatchesThenFetches()
    {
        var handler = Handler(FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("""{"identifier":"SKU-1"}"""));
        await Helpers.BuildContext(handler).CreateOrUpdateProductIdentifierAsync(new ProductIdentifier { Identifier = "SKU-1" }, CT);

        var patch = handler.Captured.First(r => r.Method == HttpMethod.Patch);
        Assert.EndsWith("/products/SKU-1", patch.RequestUri);
    }

    [Fact]
    public async Task CreateOrUpdateProductModelAsync_PatchesThenFetches()
    {
        var handler = Handler(FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("""{"code":"pm-1"}"""));
        await Helpers.BuildContext(handler).CreateOrUpdateProductModelAsync(new ProductModel { Code = "pm-1" }, CT);

        var patch = handler.Captured.First(r => r.Method == HttpMethod.Patch);
        Assert.EndsWith("/product-models/pm-1", patch.RequestUri);
    }

    [Fact]
    public async Task StreamProductIdentifiersAsync_UsesSearchAfter_AndTerminates()
    {
        var handler = Handler(
            HalPage(1, "https://akeneo.test/api/rest/v1/products?pagination_type=search_after&search_after=c1"),
            HalPage(1));
        var items = await Materialize(Helpers.BuildContext(handler).StreamProductIdentifiersAsync(ct: CT));

        Assert.Equal(2, items);
        Assert.Contains("pagination_type=search_after", UrlOf(handler, 0));
        Assert.Contains("search_after=c1", UrlOf(handler, 1));
    }

    [Fact]
    public async Task StreamProductModelsAsync_UsesSearchAfter_AndTerminates()
    {
        var handler = Handler(
            HalPage(1, "/api/rest/v1/product-models?search_after=c1"),
            HalPage(1));
        var items = await Materialize(Helpers.BuildContext(handler).StreamProductModelsAsync(ct: CT));

        Assert.Equal(2, items);
        Assert.Contains("/product-models?", UrlOf(handler, 0));
        Assert.Contains("search_after=c1", UrlOf(handler, 1));
    }

    [Fact]
    public async Task StreamProductMediaFilesAsync_PagePagination_Terminates()
    {
        var handler = Handler(HalPage(2, "/api/rest/v1/media-files?page=2"), HalPage(1));
        var items = await Materialize(Helpers.BuildContext(handler).StreamProductMediaFilesAsync(CT));

        Assert.Equal(3, items);
        Assert.Contains("page=1", UrlOf(handler, 0));
        Assert.Contains("page=2", UrlOf(handler, 1));
    }

    [Fact]
    public async Task GetProductMediaFileAsync_KeepsSlashesInCode()
    {
        var handler = Handler(FakeHttpHandler.Ok("{}"));
        await Helpers.BuildContext(handler).GetProductMediaFileAsync("3/b/5/a/photo.jpg", CT);

        Assert.EndsWith("/media-files/3/b/5/a/photo.jpg", UrlOf(handler));
    }

    [Fact]
    public async Task DownloadProductMediaFileAsync_AppendsDownloadSegment()
    {
        var handler = Handler(FakeHttpHandler.Ok("bytes"));
        await Helpers.BuildContext(handler).DownloadProductMediaFileAsync("3/b/photo.jpg", CT);

        Assert.EndsWith("/media-files/3/b/photo.jpg/download", UrlOf(handler));
    }

    // -------------------------------------------------------------------------
    // Attributes / options / groups
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAttributeListAsync_BuildsTypedQueryParameters()
    {
        var handler = Handler(HalPage());
        await Helpers.BuildContext(handler).GetAttributeListAsync(3, 25, "srch", withCount: true, withTableSelectOptions: true, ct: CT);

        var url = UrlOf(handler);
        Assert.Contains("/api/rest/v1/attributes?", url);
        Assert.Contains("page=3", url);
        Assert.Contains("limit=25", url);
        Assert.Contains("search=srch", url);
        Assert.Contains("with_count=true", url);
        Assert.Contains("with_table_select_options=true", url);
    }

    [Fact]
    public async Task GetAttribute_Option_Group_UseExpectedPaths()
    {
        var handler = Handler(FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetAttributeAsync("color", CT);
        await ctx.GetAttributeOptionAsync("color", "red", CT);
        await ctx.GetAttributeGroupAsync("marketing", CT);

        Assert.EndsWith("/attributes/color", UrlOf(handler, 0));
        Assert.EndsWith("/attributes/color/options/red", UrlOf(handler, 1));
        Assert.EndsWith("/attribute-groups/marketing", UrlOf(handler, 2));
    }

    [Fact]
    public async Task CreateOrUpdateAttribute_Option_Group_PatchExpectedPaths()
    {
        var handler = Handler(
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.CreateOrUpdateAttributeAsync(new AkeneoAttribute { Code = "color", Type = "pim_catalog_simpleselect", Group = "other" }, CT);
        await ctx.CreateOrUpdateAttributeOptionAsync("color", new AttributeOption { Code = "red" }, CT);
        await ctx.CreateOrUpdateAttributeGroupAsync(new AttributeGroup { Code = "marketing" }, CT);

        var patches = handler.Captured.Where(r => r.Method == HttpMethod.Patch).ToList();
        Assert.EndsWith("/attributes/color", patches[0].RequestUri);
        Assert.EndsWith("/attributes/color/options/red", patches[1].RequestUri);
        Assert.EndsWith("/attribute-groups/marketing", patches[2].RequestUri);
    }

    [Fact]
    public async Task StreamAttributes_Options_Groups_TerminateOnMissingNextLink()
    {
        var ctxAttr = Helpers.BuildContext(Handler(HalPage(1, "/api/rest/v1/attributes?page=2"), HalPage(1)));
        Assert.Equal(2, await Materialize(ctxAttr.StreamAttributesAsync(ct: CT)));

        var ctxOpt = Helpers.BuildContext(Handler(HalPage(1, "/x?page=2"), HalPage(1)));
        Assert.Equal(2, await Materialize(ctxOpt.StreamAttributeOptionsAsync("color", ct: CT)));

        var ctxGrp = Helpers.BuildContext(Handler(HalPage(1)));
        Assert.Equal(1, await Materialize(ctxGrp.StreamAttributeGroupsAsync(ct: CT)));
    }

    // -------------------------------------------------------------------------
    // Families / variants
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetFamilyListAsync_And_VariantList_BuildExpectedUrls()
    {
        var handler = Handler(HalPage(), HalPage());
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetFamilyListAsync(1, 100, "srch", withCount: true, ct: CT);
        await ctx.GetFamilyVariantListAsync("clothing", 2, 10, ct: CT);

        Assert.Contains("/api/rest/v1/families?", UrlOf(handler, 0));
        Assert.Contains("search=srch", UrlOf(handler, 0));
        Assert.Contains("/families/clothing/variants?", UrlOf(handler, 1));
        Assert.Contains("page=2", UrlOf(handler, 1));
    }

    [Fact]
    public async Task GetFamily_And_Variant_UseExpectedPaths()
    {
        var handler = Handler(FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetFamilyAsync("clothing", CT);
        await ctx.GetFamilyVariantAsync("clothing", "by_size", CT);

        Assert.EndsWith("/families/clothing", UrlOf(handler, 0));
        Assert.EndsWith("/families/clothing/variants/by_size", UrlOf(handler, 1));
    }

    [Fact]
    public async Task CreateOrUpdateFamily_And_Variant_PatchExpectedPaths()
    {
        var handler = Handler(
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.CreateOrUpdateFamilyAsync(new Family { Code = "clothing" }, CT);
        await ctx.CreateOrUpdateFamilyVariantAsync("clothing", new FamilyVariant { Code = "by_size" }, CT);

        var patches = handler.Captured.Where(r => r.Method == HttpMethod.Patch).ToList();
        Assert.EndsWith("/families/clothing", patches[0].RequestUri);
        Assert.EndsWith("/families/clothing/variants/by_size", patches[1].RequestUri);
    }

    [Fact]
    public async Task StreamFamilies_And_Variants_Terminate()
    {
        Assert.Equal(2, await Materialize(Helpers.BuildContext(Handler(HalPage(1, "/x?page=2"), HalPage(1))).StreamFamiliesAsync(ct: CT)));
        Assert.Equal(1, await Materialize(Helpers.BuildContext(Handler(HalPage(1))).StreamFamilyVariantsAsync("clothing", ct: CT)));
    }

    // -------------------------------------------------------------------------
    // Categories
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetCategoryListAsync_BuildsTypedQueryParameters()
    {
        var handler = Handler(HalPage());
        await Helpers.BuildContext(handler).GetCategoryListAsync(1, 100, "srch", withCount: true, withPosition: true, withEnrichedAttributes: true, ct: CT);

        var url = UrlOf(handler);
        Assert.Contains("/api/rest/v1/categories?", url);
        Assert.Contains("with_position=true", url);
        Assert.Contains("with_enriched_attributes=true", url);
    }

    [Fact]
    public async Task GetCategoryAsync_IncludesEnrichmentParams()
    {
        var handler = Handler(FakeHttpHandler.Ok("{}"));
        await Helpers.BuildContext(handler).GetCategoryAsync("master", withPosition: true, withEnrichedAttributes: false, ct: CT);

        var url = UrlOf(handler);
        Assert.Contains("/categories/master?", url);
        Assert.Contains("with_position=true", url);
        Assert.Contains("with_enriched_attributes=false", url);
    }

    [Fact]
    public async Task CreateOrUpdateCategoryAsync_PatchesThenFetches()
    {
        var handler = Handler(FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        await Helpers.BuildContext(handler).CreateOrUpdateCategoryAsync(new Category { Code = "master" }, CT);

        Assert.EndsWith("/categories/master", handler.Captured.First(r => r.Method == HttpMethod.Patch).RequestUri);
    }

    [Fact]
    public async Task DownloadCategoryMediaFileAsync_KeepsSlashesAndAppendsDownload()
    {
        var handler = Handler(FakeHttpHandler.Ok("bytes"));
        await Helpers.BuildContext(handler).DownloadCategoryMediaFileAsync("1/2/banner.jpg", CT);

        Assert.EndsWith("/category-media-files/1/2/banner.jpg/download", UrlOf(handler));
    }

    [Fact]
    public async Task StreamCategoriesAsync_Terminates()
    {
        Assert.Equal(2, await Materialize(Helpers.BuildContext(Handler(HalPage(1, "/x?page=2"), HalPage(1))).StreamCategoriesAsync(ct: CT)));
    }

    // -------------------------------------------------------------------------
    // Channels / locales / currencies / measurement families
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ChannelLocaleCurrency_ListsAndGets_UseExpectedUrls()
    {
        var handler = Handler(
            HalPage(), FakeHttpHandler.Ok("{}"),
            HalPage(), FakeHttpHandler.Ok("{}"),
            HalPage(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetChannelListAsync(1, 100, withCount: true, ct: CT);
        await ctx.GetChannelAsync("ecommerce", CT);
        await ctx.GetLocaleListAsync(2, 50, ct: CT);
        await ctx.GetLocaleAsync("en_US", CT);
        await ctx.GetCurrencyListAsync(ct: CT);
        await ctx.GetCurrencyAsync("EUR", CT);

        Assert.Contains("/api/rest/v1/channels?", UrlOf(handler, 0));
        Assert.Contains("with_count=true", UrlOf(handler, 0));
        Assert.EndsWith("/channels/ecommerce", UrlOf(handler, 1));
        Assert.Contains("/api/rest/v1/locales?", UrlOf(handler, 2));
        Assert.Contains("page=2", UrlOf(handler, 2));
        Assert.EndsWith("/locales/en_US", UrlOf(handler, 3));
        Assert.Contains("/api/rest/v1/currencies?", UrlOf(handler, 4));
        Assert.EndsWith("/currencies/EUR", UrlOf(handler, 5));
    }

    [Fact]
    public async Task CreateOrUpdateChannelAsync_PatchesThenFetches()
    {
        var handler = Handler(FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        await Helpers.BuildContext(handler).CreateOrUpdateChannelAsync(new Channel { Code = "b2b" }, CT);

        Assert.EndsWith("/channels/b2b", handler.Captured.First(r => r.Method == HttpMethod.Patch).RequestUri);
    }

    [Fact]
    public async Task GetMeasurementFamilyListAsync_GetsBareArrayEndpoint()
    {
        var handler = Handler(FakeHttpHandler.Ok("[]"));
        var result = await Helpers.BuildContext(handler).GetMeasurementFamilyListAsync(CT);

        Assert.Empty(result);
        Assert.EndsWith("/api/rest/v1/measurement-families", UrlOf(handler));
    }

    [Fact]
    public async Task StreamChannels_Locales_Currencies_Terminate()
    {
        Assert.Equal(2, await Materialize(Helpers.BuildContext(Handler(HalPage(1, "/x?page=2"), HalPage(1))).StreamChannelsAsync(ct: CT)));
        Assert.Equal(1, await Materialize(Helpers.BuildContext(Handler(HalPage(1))).StreamLocalesAsync(ct: CT)));
        Assert.Equal(1, await Materialize(Helpers.BuildContext(Handler(HalPage(1))).StreamCurrenciesAsync(ct: CT)));
    }

    // -------------------------------------------------------------------------
    // Association types
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AssociationTypes_ListGetPatchStream_UseExpectedUrls()
    {
        var handler = Handler(
            HalPage(), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"),
            HalPage(1));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetAssociationTypeListAsync(1, 100, withCount: false, ct: CT);
        await ctx.GetAssociationTypeAsync("UPSELL", CT);
        await ctx.CreateOrUpdateAssociationTypeAsync(new AssociationType { Code = "UPSELL" }, CT);
        Assert.Equal(1, await Materialize(ctx.StreamAssociationTypesAsync(ct: CT)));

        Assert.Contains("/api/rest/v1/association-types?", UrlOf(handler, 0));
        Assert.Contains("with_count=false", UrlOf(handler, 0));
        Assert.EndsWith("/association-types/UPSELL", UrlOf(handler, 1));
        Assert.EndsWith("/association-types/UPSELL", handler.Captured.First(r => r.Method == HttpMethod.Patch).RequestUri);
    }

    private static async Task<int> Materialize<T>(IAsyncEnumerable<T> stream)
    {
        var count = 0;
        await foreach (var _ in stream)
            count++;
        return count;
    }
}

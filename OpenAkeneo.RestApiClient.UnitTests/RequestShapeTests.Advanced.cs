using System.Net;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// WP3 — request-shape and loop-guard tests for the advanced domains (reference entities,
/// assets, catalogs, workflows, jobs, utilities).
/// </summary>
public class RequestShapeTestsAdvanced
{
    private static readonly CancellationToken CT = TestContext.Current.CancellationToken;

    private static FakeHttpHandler Handler(params HttpResponseMessage[] responses)
        => new([FakeHttpHandler.TokenResponse(), .. responses]);

    private static HttpResponseMessage HalPage(int items = 1, string? nextHref = null)
    {
        var itemsJson = string.Join(",", Enumerable.Repeat("{}", items));
        var links = nextHref is null ? "{}" : $$$"""{"next":{"href":"{{{nextHref}}}"}}""";
        return FakeHttpHandler.Ok($$$"""{"_links":{{{links}}},"_embedded":{"items":[{{{itemsJson}}}]}}""");
    }

    private static string UrlOf(FakeHttpHandler handler, int apiCallIndex = 0)
        => handler.Captured.Where(r => !r.RequestUri.Contains("/oauth/")).ElementAt(apiCallIndex).RequestUri;

    private static async Task<int> Materialize<T>(IAsyncEnumerable<T> stream)
    {
        var count = 0;
        await foreach (var _ in stream)
            count++;
        return count;
    }

    // -------------------------------------------------------------------------
    // Reference entities — entity / attribute / option
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ReferenceEntity_GetListPatch_UseExpectedUrls()
    {
        var handler = Handler(
            HalPage(), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetReferenceEntityListAsync("cursor-1", CT);
        await ctx.GetReferenceEntityAsync("brands", CT);
        await ctx.CreateOrUpdateReferenceEntityAsync(new ReferenceEntity { Code = "brands" }, CT);

        Assert.Contains("/api/rest/v1/reference-entities?", UrlOf(handler, 0));
        Assert.Contains("search_after=cursor-1", UrlOf(handler, 0));
        Assert.EndsWith("/reference-entities/brands", UrlOf(handler, 1));
        Assert.EndsWith("/reference-entities/brands", handler.Captured.First(r => r.Method == HttpMethod.Patch).RequestUri);
    }

    [Fact]
    public async Task ReferenceEntityAttribute_AndOption_UseExpectedUrls()
    {
        var handler = Handler(
            FakeHttpHandler.Ok("[]"), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.Ok("[]"), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetReferenceEntityAttributeListAsync("brands", CT);
        await ctx.GetReferenceEntityAttributeAsync("brands", "label", CT);
        await ctx.CreateOrUpdateReferenceEntityAttributeAsync("brands", new ReferenceEntityAttribute { Code = "label" }, CT);
        await ctx.GetReferenceEntityAttributeOptionListAsync("brands", "seg", CT);
        await ctx.GetReferenceEntityAttributeOptionAsync("brands", "seg", "opt1", CT);
        await ctx.CreateOrUpdateReferenceEntityAttributeOptionAsync("brands", "seg", new ReferenceEntityAttributeOption { Code = "opt1" }, CT);

        // Request order: list, get, PATCH+GET (CreateOrUpdate), option list, option get, PATCH+GET.
        Assert.EndsWith("/reference-entities/brands/attributes", UrlOf(handler, 0));
        Assert.EndsWith("/reference-entities/brands/attributes/label", UrlOf(handler, 1));
        Assert.EndsWith("/reference-entities/brands/attributes/seg/options", UrlOf(handler, 4));
        Assert.EndsWith("/reference-entities/brands/attributes/seg/options/opt1", UrlOf(handler, 5));
        var patches = handler.Captured.Where(r => r.Method == HttpMethod.Patch).ToList();
        Assert.EndsWith("/reference-entities/brands/attributes/label", patches[0].RequestUri);
        Assert.EndsWith("/reference-entities/brands/attributes/seg/options/opt1", patches[1].RequestUri);
    }

    [Fact]
    public async Task ReferenceEntityRecordList_BuildsTypedQueryParameters()
    {
        var handler = Handler(HalPage());
        await Helpers.BuildContext(handler).GetReferenceEntityRecordListAsync("brands", "srch", "ecommerce", "en_US", "cur-1", CT);

        var url = UrlOf(handler);
        Assert.Contains("/reference-entities/brands/records?", url);
        Assert.Contains("search=srch", url);
        Assert.Contains("channel=ecommerce", url);
        Assert.Contains("locales=en_US", url);
        Assert.Contains("search_after=cur-1", url);
    }

    [Fact]
    public async Task ReferenceEntityRecord_GetAndPatch_UseExpectedUrls()
    {
        var handler = Handler(FakeHttpHandler.Ok("{}"), FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetReferenceEntityRecordAsync("brands", "nike", CT);
        await ctx.CreateOrUpdateReferenceEntityRecordAsync("brands", new ReferenceEntityRecord { Code = "nike" }, CT);

        Assert.EndsWith("/reference-entities/brands/records/nike", UrlOf(handler, 0));
        Assert.EndsWith("/reference-entities/brands/records/nike", handler.Captured.First(r => r.Method == HttpMethod.Patch).RequestUri);
    }

    [Fact]
    public async Task StreamReferenceEntityRecordsAsync_FollowsCursorAndTerminates()
    {
        var handler = Handler(
            HalPage(1, "/api/rest/v1/reference-entities/brands/records?search_after=rec-c1"),
            HalPage(1));
        var items = await Materialize(Helpers.BuildContext(handler).StreamReferenceEntityRecordsAsync("brands", ct: CT));

        Assert.Equal(2, items);
        Assert.Contains("search_after=rec-c1", UrlOf(handler, 1));
    }

    [Fact]
    public async Task GetReferenceEntityListFullAsync_FollowsCursorAndTerminates()
    {
        var handler = Handler(
            HalPage(1, "/api/rest/v1/reference-entities?search_after=re-c1"),
            HalPage(1));
        var list = await Helpers.BuildContext(handler).GetReferenceEntityListFullAsync(CT);

        Assert.Equal(2, list.Count);
        Assert.Contains("search_after=re-c1", UrlOf(handler, 1));
    }

    [Fact]
    public async Task DownloadReferenceEntityMediaFileAsync_KeepsSlashesInCode()
    {
        var handler = Handler(FakeHttpHandler.Ok("bytes"));
        await Helpers.BuildContext(handler).DownloadReferenceEntityMediaFileAsync("a/b/portrait.jpg", CT);

        Assert.EndsWith("/reference-entities-media-files/a/b/portrait.jpg", UrlOf(handler));
    }

    // -------------------------------------------------------------------------
    // Assets — family / attribute / option / asset
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AssetFamily_GetListPatch_UseExpectedUrls()
    {
        var handler = Handler(
            HalPage(), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetAssetFamilyListAsync("cursor-1", CT);
        await ctx.GetAssetFamilyAsync("packshots", CT);
        await ctx.CreateOrUpdateAssetFamilyAsync(new AssetFamily { Code = "packshots" }, CT);

        Assert.Contains("/api/rest/v1/asset-families?", UrlOf(handler, 0));
        Assert.Contains("search_after=cursor-1", UrlOf(handler, 0));
        Assert.EndsWith("/asset-families/packshots", UrlOf(handler, 1));
        Assert.EndsWith("/asset-families/packshots", handler.Captured.First(r => r.Method == HttpMethod.Patch).RequestUri);
    }

    [Fact]
    public async Task AssetAttribute_AndOption_UseExpectedUrls()
    {
        var handler = Handler(
            FakeHttpHandler.Ok("[]"), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.Ok("[]"), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetAssetAttributeListAsync("packshots", CT);
        await ctx.GetAssetAttributeAsync("packshots", "media", CT);
        await ctx.CreateOrUpdateAssetAttributeAsync("packshots", new AssetAttribute { Code = "media" }, CT);
        await ctx.GetAssetAttributeOptionListAsync("packshots", "tags", CT);
        await ctx.GetAssetAttributeOptionAsync("packshots", "tags", "new", CT);
        await ctx.CreateOrUpdateAssetAttributeOptionAsync("packshots", "tags", new AssetAttributeOption { Code = "new" }, CT);

        // Request order: list, get, PATCH+GET (CreateOrUpdate), option list, option get, PATCH+GET.
        Assert.EndsWith("/asset-families/packshots/attributes", UrlOf(handler, 0));
        Assert.EndsWith("/asset-families/packshots/attributes/media", UrlOf(handler, 1));
        Assert.EndsWith("/asset-families/packshots/attributes/tags/options", UrlOf(handler, 4));
        Assert.EndsWith("/asset-families/packshots/attributes/tags/options/new", UrlOf(handler, 5));
        var patches = handler.Captured.Where(r => r.Method == HttpMethod.Patch).ToList();
        Assert.EndsWith("/asset-families/packshots/attributes/media", patches[0].RequestUri);
        Assert.EndsWith("/asset-families/packshots/attributes/tags/options/new", patches[1].RequestUri);
    }

    [Fact]
    public async Task GetAssetListAsync_BuildsTypedQueryParameters()
    {
        var handler = Handler(HalPage());
        await Helpers.BuildContext(handler).GetAssetListAsync("packshots", 50, "srch", "cur-1", CT);

        var url = UrlOf(handler);
        Assert.Contains("/asset-families/packshots/assets?", url);
        Assert.Contains("limit=50", url);
        Assert.Contains("search=srch", url);
        Assert.Contains("search_after=cur-1", url);
    }

    [Fact]
    public async Task Asset_GetAndPatch_UseExpectedUrls()
    {
        var handler = Handler(FakeHttpHandler.Ok("{}"), FakeHttpHandler.NoContent(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetAssetAsync("packshots", "asset-1", CT);
        await ctx.CreateOrUpdateAssetAsync("packshots", new Asset { Code = "asset-1" }, CT);

        Assert.EndsWith("/asset-families/packshots/assets/asset-1", UrlOf(handler, 0));
        Assert.EndsWith("/asset-families/packshots/assets/asset-1", handler.Captured.First(r => r.Method == HttpMethod.Patch).RequestUri);
    }

    // -------------------------------------------------------------------------
    // Catalogs
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StreamCatalogsAsync_PagePagination_Terminates()
    {
        var handler = Handler(HalPage(2, "/api/rest/v1/catalogs?page=2"), HalPage(1));
        var items = await Materialize(Helpers.BuildContext(handler).StreamCatalogsAsync(CT));

        Assert.Equal(3, items);
        Assert.Contains("page=1", UrlOf(handler, 0));
        Assert.Contains("page=2", UrlOf(handler, 1));
    }

    [Fact]
    public async Task Catalog_GetAndProductEndpoints_UseExpectedUrls()
    {
        var handler = Handler(
            FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.Ok("""{"_links":{},"_embedded":{"items":["u1"]}}"""), // product-uuids are string items
            HalPage(),
            FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetCatalogAsync("cat-1", CT);
        await ctx.GetCatalogProductUuidListAsync("cat-1", 50, "cur-1", CT);
        await ctx.GetCatalogProductListAsync("cat-1", 25, null, CT);
        await ctx.GetCatalogProductAsync("cat-1", "uuid-9", CT);

        Assert.EndsWith("/catalogs/cat-1", UrlOf(handler, 0));
        Assert.Contains("/catalogs/cat-1/product-uuids?limit=50", UrlOf(handler, 1));
        Assert.Contains("search_after=cur-1", UrlOf(handler, 1));
        Assert.Contains("/catalogs/cat-1/products?limit=25", UrlOf(handler, 2));
        Assert.EndsWith("/catalogs/cat-1/products/uuid-9", UrlOf(handler, 3));
    }

    [Fact]
    public async Task Catalog_MappedEndpointsAndSchema_UseExpectedUrls()
    {
        var handler = Handler(
            FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetCatalogMappedProductListAsync("cat-1", 100, "m-cur", CT);
        await ctx.GetCatalogMappedModelListAsync("cat-1", ct: CT);
        await ctx.GetCatalogMappedVariantListAsync("cat-1", "model-1", ct: CT);
        await ctx.GetCatalogMappingSchemaAsync("cat-1", CT);

        Assert.Contains("/catalogs/cat-1/mapped-products?limit=100", UrlOf(handler, 0));
        Assert.Contains("search_after=m-cur", UrlOf(handler, 0));
        Assert.Contains("/catalogs/cat-1/mapped-models?limit=100", UrlOf(handler, 1));
        Assert.Contains("/catalogs/cat-1/mapped-models/model-1/variants?limit=100", UrlOf(handler, 2));
        Assert.EndsWith("/catalogs/cat-1/mapping-schemas/product", UrlOf(handler, 3));
    }

    // -------------------------------------------------------------------------
    // Workflows
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Workflows_ListsAndGets_UseExpectedUrls()
    {
        var handler = Handler(
            HalPage(), FakeHttpHandler.Ok("{}"),
            HalPage(),
            HalPage(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetWorkflowListAsync(2, 10, CT);
        await ctx.GetWorkflowAsync("wf-uuid", CT);
        await ctx.GetWorkflowStepAssigneeListAsync("step-uuid", 1, 100, CT);
        await ctx.GetWorkflowTaskListAsync(1, 100, "srch", withAttributes: true, ct: CT);
        await ctx.GetWorkflowTaskAsync("task-uuid", CT);

        Assert.Contains("/api/rest/v1/workflows?", UrlOf(handler, 0));
        Assert.Contains("page=2", UrlOf(handler, 0));
        Assert.EndsWith("/workflows/wf-uuid", UrlOf(handler, 1));
        Assert.Contains("/workflows/steps/step-uuid/assignees?", UrlOf(handler, 2));
        Assert.Contains("/workflows/tasks?", UrlOf(handler, 3));
        Assert.Contains("search=srch", UrlOf(handler, 3));
        Assert.Contains("with_attributes=true", UrlOf(handler, 3));
        Assert.EndsWith("/workflows/tasks/task-uuid", UrlOf(handler, 4));
    }

    [Fact]
    public async Task StreamWorkflows_Assignees_Tasks_Terminate()
    {
        Assert.Equal(2, await Materialize(Helpers.BuildContext(Handler(HalPage(1, "/x?page=2"), HalPage(1))).StreamWorkflowsAsync(CT)));
        Assert.Equal(1, await Materialize(Helpers.BuildContext(Handler(HalPage(1))).StreamWorkflowStepAssigneesAsync("step-uuid", CT)));
        Assert.Equal(1, await Materialize(Helpers.BuildContext(Handler(HalPage(1))).StreamWorkflowTasksAsync(ct: CT)));
    }

    // -------------------------------------------------------------------------
    // Jobs — zero coverage before WP3
    // -------------------------------------------------------------------------

    [Fact]
    public async Task LaunchExportJobAsync_PostsDryRunBody()
    {
        var handler = Handler(FakeHttpHandler.Ok("""{"execution_id":42}"""));
        await Helpers.BuildContext(handler).LaunchExportJobAsync("my_export", isDryRun: true, CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/jobs/export/my_export", req.RequestUri);
        Assert.Contains("\"is_dry_run\":true", req.Body);
    }

    [Fact]
    public async Task LaunchImportJobAsync_IncludesImportModeOnlyWhenSet()
    {
        var handler = Handler(FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.LaunchImportJobAsync("my_import", isDryRun: false, importMode: "add_or_update", CT);
        await ctx.LaunchImportJobAsync("my_import", ct: CT);

        var requests = handler.Captured.Where(r => !r.RequestUri.Contains("/oauth/")).ToList();
        Assert.EndsWith("/jobs/import/my_import", requests[0].RequestUri);
        Assert.Contains("\"import_mode\":\"add_or_update\"", requests[0].Body);
        Assert.Contains("\"is_dry_run\":false", requests[1].Body);
        Assert.DoesNotContain("import_mode", requests[1].Body);
    }

    // -------------------------------------------------------------------------
    // Utilities
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Utilities_GetEndpoints_UseExpectedUrls()
    {
        var handler = Handler(
            FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.Ok("{}"), FakeHttpHandler.Ok("[]"),
            HalPage(), FakeHttpHandler.Ok("{}"));
        var ctx = Helpers.BuildContext(handler);
        await ctx.GetSystemInformationAsync(CT);
        await ctx.GetUserChannelsPermissionsAsync("user-uuid", CT);
        await ctx.GetUserLocalesPermissionsAsync("user-uuid", CT);
        await ctx.GetApiOverviewAsync(CT);
        await ctx.GetExtensionListAsync(CT);
        await ctx.GetModelizationSuggestionListAsync(1, 100, CT);
        await ctx.GetModelizationSuggestionAsync("sugg-uuid", CT);

        Assert.EndsWith("/api/rest/v1/system-information", UrlOf(handler, 0));
        Assert.EndsWith("/permissions/user-uuid/channels", UrlOf(handler, 1));
        Assert.EndsWith("/permissions/user-uuid/locales", UrlOf(handler, 2));
        Assert.EndsWith("/api/rest/v1", UrlOf(handler, 3));
        Assert.EndsWith("/api/rest/v1/ui-extensions", UrlOf(handler, 4));
        Assert.Contains("/data-model-designer/modelization-suggestions?page=1&limit=100", UrlOf(handler, 5));
        Assert.EndsWith("/data-model-designer/modelization-suggestion/sugg-uuid", UrlOf(handler, 6));
    }
}

using System.Net;
using System.Text.Json;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// Locking tests for the 0.9.0 major revision: bulk PATCH endpoints, the new domains/operations
/// (rule definitions, workflow write ops, ui-extensions write ops, modelization actions,
/// cross-entity records), the Enabled bool? breaking change, and streaming downloads.
/// </summary>
public class Revision090Tests
{
    private static readonly CancellationToken CT = TestContext.Current.CancellationToken;

    private static FakeHttpHandler Handler(params HttpResponseMessage[] responses)
        => new([FakeHttpHandler.TokenResponse(), .. responses]);

    private static string UrlOf(FakeHttpHandler handler, int apiCallIndex = 0)
        => handler.Captured.Where(r => !r.RequestUri.Contains("/oauth/")).ElementAt(apiCallIndex).RequestUri;

    // -------------------------------------------------------------------------
    // Bulk PATCH — NDJSON endpoints
    // -------------------------------------------------------------------------

    [Fact]
    public async Task BulkCreateOrUpdateProductUuids_SendsNdjsonWithCollectionContentType()
    {
        var responseLines = "{\"line\":1,\"uuid\":\"u1\",\"status_code\":201}\n{\"line\":2,\"uuid\":\"u2\",\"status_code\":204}";
        var handler = Handler(FakeHttpHandler.Text(HttpStatusCode.OK, responseLines));

        var results = await Helpers.BuildContext(handler).BulkCreateOrUpdateProductUuidsAsync(
            [new ProductUuid { Uuid = "u1" }, new ProductUuid { Uuid = "u2" }], CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Patch, req.Method);
        Assert.EndsWith("/api/rest/v1/products-uuid", req.RequestUri);
        Assert.Equal("application/vnd.akeneo.collection+json", req.ContentType);
        // One JSON object per line, no array brackets.
        var lines = req.Body!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Equal(2, lines.Length);
        Assert.StartsWith("{", lines[0]);
        Assert.Contains("\"uuid\":\"u1\"", lines[0]);

        Assert.Equal(2, results.Count);
        Assert.True(results[0].Succeeded);
        Assert.Equal("u1", results[0].Key);
        Assert.Equal(204, results[1].StatusCode);
    }

    [Fact]
    public async Task BulkCreateOrUpdate_RejectedLine_SurfacesMessageAndErrors()
    {
        var responseLines = """{"line":1,"code":"bad","status_code":422,"message":"Validation failed","errors":[{"property":"labels","message":"Locale x does not exist"}]}""";
        var handler = Handler(FakeHttpHandler.Text(HttpStatusCode.OK, responseLines));

        var results = await Helpers.BuildContext(handler).BulkCreateOrUpdateFamiliesAsync([new Family { Code = "bad" }], CT);

        var item = Assert.Single(results);
        Assert.False(item.Succeeded);
        Assert.Equal("Validation failed", item.Message);
        Assert.Equal("labels", item.Errors![0].Property);
    }

    [Fact]
    public async Task BulkCreateOrUpdate_Over100Items_ChunksAndRenumbersLines()
    {
        static HttpResponseMessage ChunkResponse(int count) => FakeHttpHandler.Text(HttpStatusCode.OK,
            string.Join("\n", Enumerable.Range(1, count).Select(i => $$"""{"line":{{i}},"code":"c{{i}}","status_code":204}""")));

        var handler = Handler(ChunkResponse(100), ChunkResponse(50));
        var items = Enumerable.Range(1, 150).Select(i => new Channel { Code = $"ch{i}" });

        var results = await Helpers.BuildContext(handler).BulkCreateOrUpdateChannelsAsync(items, CT);

        Assert.Equal(150, results.Count);
        Assert.Equal(2, handler.Captured.Count(r => r.Method == HttpMethod.Patch));
        // Lines from the second chunk are renumbered to absolute positions.
        Assert.Equal(101, results[100].Line);
        Assert.Equal(150, results[149].Line);
    }

    [Theory]
    [InlineData("products")]
    [InlineData("product-models")]
    [InlineData("attributes")]
    [InlineData("attribute-groups")]
    [InlineData("association-types")]
    [InlineData("categories")]
    public async Task BulkCreateOrUpdate_TargetsExpectedEndpoint(string expectedPath)
    {
        var handler = Handler(FakeHttpHandler.Text(HttpStatusCode.OK, """{"line":1,"code":"x","status_code":204}"""));
        var ctx = Helpers.BuildContext(handler);

        Task<List<BulkItemResult>> call = expectedPath switch
        {
            "products" => ctx.BulkCreateOrUpdateProductIdentifiersAsync([new ProductIdentifier { Identifier = "x" }], CT),
            "product-models" => ctx.BulkCreateOrUpdateProductModelsAsync([new ProductModel { Code = "x" }], CT),
            "attributes" => ctx.BulkCreateOrUpdateAttributesAsync([new AkeneoAttribute { Code = "x", Type = "pim_catalog_text", Group = "other" }], CT),
            "attribute-groups" => ctx.BulkCreateOrUpdateAttributeGroupsAsync([new AttributeGroup { Code = "x" }], CT),
            "association-types" => ctx.BulkCreateOrUpdateAssociationTypesAsync([new AssociationType { Code = "x" }], CT),
            "categories" => ctx.BulkCreateOrUpdateCategoriesAsync([new Category { Code = "x" }], CT),
            _ => throw new InvalidOperationException()
        };
        await call;

        Assert.EndsWith($"/api/rest/v1/{expectedPath}", handler.LastApiRequest!.RequestUri);
    }

    [Fact]
    public async Task BulkCreateOrUpdate_ScopedEndpoints_BindParentCodes()
    {
        var handler = Handler(
            FakeHttpHandler.Text(HttpStatusCode.OK, """{"line":1,"code":"v1","status_code":204}"""),
            FakeHttpHandler.Text(HttpStatusCode.OK, """{"line":1,"code":"o1","status_code":204}"""));
        var ctx = Helpers.BuildContext(handler);

        await ctx.BulkCreateOrUpdateFamilyVariantsAsync("clothing", [new FamilyVariant { Code = "v1" }], CT);
        await ctx.BulkCreateOrUpdateAttributeOptionsAsync("color", [new AttributeOption { Code = "o1" }], CT);

        Assert.EndsWith("/families/clothing/variants", UrlOf(handler, 0));
        Assert.EndsWith("/attributes/color/options", UrlOf(handler, 1));
    }

    // -------------------------------------------------------------------------
    // Bulk PATCH — JSON-array endpoints (records, assets)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task BulkCreateOrUpdateAssets_SendsJsonArray_ParsesArrayResponse()
    {
        var handler = Handler(FakeHttpHandler.Json(HttpStatusCode.OK,
            """[{"code":"a1","status_code":201},{"code":"a2","status_code":422,"message":"nope"}]"""));

        var results = await Helpers.BuildContext(handler).BulkCreateOrUpdateAssetsAsync("packshots",
            [new Asset { Code = "a1" }, new Asset { Code = "a2" }], CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Patch, req.Method);
        Assert.EndsWith("/asset-families/packshots/assets", req.RequestUri);
        Assert.Equal("application/json", req.ContentType);
        Assert.StartsWith("[", req.Body);

        Assert.Equal(2, results.Count);
        // Line numbers are synthesised for array endpoints (the API returns none).
        Assert.Equal(1, results[0].Line);
        Assert.Equal(2, results[1].Line);
        Assert.False(results[1].Succeeded);
    }

    [Fact]
    public async Task BulkCreateOrUpdateReferenceEntityRecords_TargetsRecordsEndpoint()
    {
        var handler = Handler(FakeHttpHandler.Json(HttpStatusCode.OK, """[{"code":"r1","status_code":201}]"""));

        await Helpers.BuildContext(handler).BulkCreateOrUpdateReferenceEntityRecordsAsync("brands",
            [new ReferenceEntityRecord { Code = "r1" }], CT);

        Assert.EndsWith("/reference-entities/brands/records", handler.LastApiRequest!.RequestUri);
        Assert.Equal("application/json", handler.LastApiRequest!.ContentType);
    }

    // -------------------------------------------------------------------------
    // Rule definitions
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RuleDefinitions_ListGetPut_UseExpectedShapes()
    {
        var handler = Handler(
            FakeHttpHandler.Ok("""{"_links":{},"_embedded":{"items":[{"code":"r1","type":"product"}]}}"""),
            FakeHttpHandler.Ok("""{"code":"r1","type":"product","enabled":true}"""),
            FakeHttpHandler.NoContent(),
            FakeHttpHandler.Ok("""{"code":"r1","type":"product"}"""));
        var ctx = Helpers.BuildContext(handler);

        var list = await ctx.GetRuleDefinitionListAsync(1, 50, withCount: true, ct: CT);
        Assert.Single(list.RuleDefinitions);
        Assert.Contains("/api/rest/v1/rule-definitions?", UrlOf(handler, 0));
        Assert.Contains("limit=50", UrlOf(handler, 0));

        var rule = await ctx.GetRuleDefinitionAsync("r1", CT);
        Assert.True(rule.Enabled);
        Assert.EndsWith("/rule-definitions/r1", UrlOf(handler, 1));

        await ctx.CreateOrReplaceRuleDefinitionAsync(new RuleDefinition { Code = "r1", Type = "product", Actions = new List<object?>() }, CT);
        var put = handler.Captured.First(r => r.Method == HttpMethod.Put);
        Assert.EndsWith("/rule-definitions/r1", put.RequestUri);
        Assert.Contains("\"actions\":[]", put.Body);
    }

    [Fact]
    public async Task StreamRuleDefinitionsAsync_Terminates()
    {
        var handler = Handler(FakeHttpHandler.Ok("""{"_links":{},"_embedded":{"items":[{"code":"r1"}]}}"""));
        var count = 0;
        await foreach (var _ in Helpers.BuildContext(handler).StreamRuleDefinitionsAsync(CT))
            count++;
        Assert.Equal(1, count);
    }

    // -------------------------------------------------------------------------
    // Workflow write operations
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StartWorkflowExecutionsAsync_PostsWorkflowProductPairs()
    {
        var handler = Handler(FakeHttpHandler.Created("[]"));

        await Helpers.BuildContext(handler).StartWorkflowExecutionsAsync(
        [
            WorkflowExecutionRequest.ForProduct("wf-1", "prod-uuid-1"),
            WorkflowExecutionRequest.ForProductModel("wf-1", "model-1")
        ], CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/workflows/executions", req.RequestUri);
        Assert.Contains("""{"workflow":{"uuid":"wf-1"},"product":{"uuid":"prod-uuid-1"}}""", req.Body);
        Assert.Contains("""{"workflow":{"uuid":"wf-1"},"product_model":{"code":"model-1"}}""", req.Body);
    }

    [Fact]
    public async Task WorkflowTaskActions_PatchExpectedStatusPayloads()
    {
        var handler = Handler(FakeHttpHandler.NoContent(), FakeHttpHandler.NoContent(), FakeHttpHandler.NoContent());
        var ctx = Helpers.BuildContext(handler);

        await ctx.CompleteWorkflowTaskAsync("task-1", CT);
        await ctx.ApproveWorkflowTaskAsync("task-1", CT);
        await ctx.RejectWorkflowTaskAsync("task-1", "step-9", """{"name":[{"comment":"bad","locale":null,"scope":null}]}""", CT);

        var patches = handler.Captured.Where(r => r.Method == HttpMethod.Patch).ToList();
        Assert.All(patches, p => Assert.EndsWith("/workflows/tasks/task-1", p.RequestUri));
        Assert.Contains("\"status\":\"completed\"", patches[0].Body);
        Assert.Contains("\"status\":\"approved\"", patches[1].Body);
        Assert.Contains("\"status\":\"rejected\"", patches[2].Body);
        Assert.Contains("\"send_back_to_step_uuid\":\"step-9\"", patches[2].Body);
        Assert.Contains("\"rejected_attributes\"", patches[2].Body);
    }

    // -------------------------------------------------------------------------
    // UI extensions write operations
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Extensions_CreateUpdateDelete_UseExpectedShapes()
    {
        var handler = Handler(
            FakeHttpHandler.Created("""{"uuid":"ext-1","name":"My ext"}"""),
            FakeHttpHandler.Ok("""{"uuid":"ext-1","name":"Renamed"}"""),
            FakeHttpHandler.NoContent());
        var ctx = Helpers.BuildContext(handler);

        var created = await ctx.CreateExtensionAsync(new Extension { Name = "My ext", Type = "iframe", Position = "pim.product.tab" }, CT);
        Assert.Equal("ext-1", created.Uuid);
        Assert.EndsWith("/api/rest/v1/ui-extensions", UrlOf(handler, 0));

        var updated = await ctx.UpdateExtensionAsync("ext-1", new Extension { Name = "Renamed" }, CT);
        Assert.Equal("Renamed", updated.Name);
        var patch = handler.Captured.First(r => r.Method == HttpMethod.Patch);
        Assert.EndsWith("/ui-extensions/ext-1", patch.RequestUri);

        await ctx.DeleteExtensionAsync("ext-1", CT);
        var delete = handler.Captured.First(r => r.Method == HttpMethod.Delete);
        Assert.EndsWith("/ui-extensions/ext-1", delete.RequestUri);
    }

    // -------------------------------------------------------------------------
    // Modelization actions
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ModelizationActions_PostToExpectedUrls()
    {
        var handler = Handler(
            FakeHttpHandler.Created("{}"),
            FakeHttpHandler.Ok("{}"),
            FakeHttpHandler.NoContent());
        var ctx = Helpers.BuildContext(handler);

        await ctx.SuggestModelizationAttributeAsync("""{"source":"x","code":"c","type":"text"}""", CT);
        await ctx.ApproveModelizationSuggestionAsync("sugg-1", ct: CT);
        await ctx.DeclineModelizationSuggestionAsync("sugg-1", CT);

        Assert.EndsWith("/data-model-designer/modelization-suggestion/attribute", UrlOf(handler, 0));
        Assert.EndsWith("/data-model-designer/modelization-suggestion/sugg-1/approve", UrlOf(handler, 1));
        Assert.EndsWith("/data-model-designer/modelization-suggestion/sugg-1/decline", UrlOf(handler, 2));
        Assert.All(handler.Captured.Where(r => !r.RequestUri.Contains("/oauth/")), r => Assert.Equal(HttpMethod.Post, r.Method));
    }

    // -------------------------------------------------------------------------
    // Cross-entity reference entity records
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StreamAllReferenceEntityRecordsAsync_FiltersAndFollowsCursor()
    {
        var page1 = """{"_links":{"next":{"href":"/api/rest/v1/reference-entities/records?search_after=c1"}},"_embedded":{"items":[{"code":"r1"}]}}""";
        var page2 = """{"_links":{},"_embedded":{"items":[{"code":"r2"}]}}""";
        var handler = Handler(FakeHttpHandler.Ok(page1), FakeHttpHandler.Ok(page2));

        var codes = new List<string>();
        await foreach (var record in Helpers.BuildContext(handler).StreamAllReferenceEntityRecordsAsync("brands", ct: CT))
            codes.Add(record.Code);

        Assert.Equal(["r1", "r2"], codes);
        Assert.Contains("/reference-entities/records?", UrlOf(handler, 0));
        Assert.Contains("reference_entity=brands", UrlOf(handler, 0));
        Assert.Contains("search_after=c1", UrlOf(handler, 1));
    }

    // -------------------------------------------------------------------------
    // Enabled bool? — the server default is no longer overridden
    // -------------------------------------------------------------------------

    [Fact]
    public void ProductEnabled_Unset_IsOmittedFromWritePayloads()
    {
        // Spec-confirmed: the server defaults enabled to true. A non-nullable bool used to
        // force "enabled":false into every payload whose caller never touched the property.
        var body = JsonSerializer.Serialize(new ProductUuid { Uuid = "u1" });
        Assert.DoesNotContain("enabled", body);

        var explicitFalse = JsonSerializer.Serialize(new ProductUuid { Uuid = "u1", Enabled = false });
        Assert.Contains("\"enabled\":false", explicitFalse);
    }

    // -------------------------------------------------------------------------
    // Streaming download
    // -------------------------------------------------------------------------

    [Fact]
    public async Task HttpGetStreamAsync_StreamsBody_AndDisposes()
    {
        var handler = Handler(FakeHttpHandler.Ok("streamed-content"));
        var svc = Helpers.BuildService(handler);

        await using (var stream = await svc.HttpGetStreamAsync("/api/rest/v1/media-files/a/b/download", CT))
        using (var reader = new StreamReader(stream))
        {
            Assert.Equal("streamed-content", await reader.ReadToEndAsync(CT));
        }
    }

    [Fact]
    public async Task HttpGetStreamAsync_NonSuccess_ThrowsAkeneoApiException()
    {
        var handler = Handler(FakeHttpHandler.Json(HttpStatusCode.NotFound, """{"message":"gone"}"""));
        var svc = Helpers.BuildService(handler);

        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() => svc.HttpGetStreamAsync("/api/rest/v1/media-files/x/download", CT));
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("gone", ex.ApiMessage);
    }

    [Fact]
    public async Task DownloadProductMediaFileStreamAsync_TargetsDownloadUrl()
    {
        var handler = Handler(FakeHttpHandler.Ok("bytes"));
        await using var stream = await Helpers.BuildContext(handler).DownloadProductMediaFileStreamAsync("3/b/photo.jpg", CT);

        Assert.EndsWith("/media-files/3/b/photo.jpg/download", UrlOf(handler));
    }

    [Fact]
    public async Task RemainingStreamDownloads_TargetExpectedUrls()
    {
        var handler = Handler(FakeHttpHandler.Ok("a"), FakeHttpHandler.Ok("b"), FakeHttpHandler.Ok("c"));
        var ctx = Helpers.BuildContext(handler);

        await using (await ctx.DownloadAssetMediaFileStreamAsync("a/b/img.png", CT)) { }
        await using (await ctx.DownloadReferenceEntityMediaFileStreamAsync("c/d/img.png", CT)) { }
        await using (await ctx.DownloadCategoryMediaFileStreamAsync("e/f/banner.jpg", CT)) { }

        Assert.EndsWith("/asset-media-files/a/b/img.png", UrlOf(handler, 0));
        Assert.EndsWith("/reference-entities-media-files/c/d/img.png", UrlOf(handler, 1));
        Assert.EndsWith("/category-media-files/e/f/banner.jpg/download", UrlOf(handler, 2));
    }

    // -------------------------------------------------------------------------
    // UI extension file update — the final spec operation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateExtensionWithFileAsync_PostsMultipartWithFieldsAndFile()
    {
        var handler = Handler(FakeHttpHandler.Ok("""{"uuid":"ext-1","version":"1.1.0"}"""));

        var body = await Helpers.BuildContext(handler).UpdateExtensionWithFileAsync(
            "ext-1", "console.log('hi')"u8.ToArray(), "extension.js",
            new Dictionary<string, string> { ["version"] = "1.1.0", ["configuration[default_label]"] = "My ext" }, ct: CT);

        var req = handler.LastApiRequest!;
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.EndsWith("/ui-extensions/ext-1", req.RequestUri);
        Assert.StartsWith("multipart/form-data", req.ContentType);
        Assert.Contains("name=version", req.Body);
        Assert.Contains("configuration[default_label]", req.Body);
        Assert.Contains("console.log", req.Body);
        Assert.Contains("\"version\":\"1.1.0\"", body);
    }
}

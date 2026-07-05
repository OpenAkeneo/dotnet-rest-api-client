using System.Net;
using OpenAkeneo.RestApiClient;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

/// <summary>
/// WP6 (guidelines/TESTING.md §6) — closes the remaining S1 live-coverage gaps: the 12 bulk
/// endpoints not yet exercised live, the cross-entity records read, the streaming download
/// round-trip, and rule-definition reads. Conventions per guidelines/TESTING.md §2.1.
/// </summary>
public class BulkAndCoverageTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;
    private AkeneoContext Context => _fixture.Context;

    private static readonly string RunId = $"oa_test_{DateTime.UtcNow:yyyyMMddHHmmss}";

    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    public BulkAndCoverageTests(TestBase fixture) => _fixture = fixture;

    private static void AssertAllSucceeded(List<BulkItemResult> results, int expectedCount)
    {
        Assert.Equal(expectedCount, results.Count);
        Assert.All(results, r => Assert.True(r.Succeeded, $"line {r.Line} ({r.Key}): {r.StatusCode} {r.Message}"));
    }

    // -------------------------------------------------------------------------
    // Bulk: attributes, attribute options, attribute groups (perm fixtures)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Bulk_Attributes_Options_And_Groups_UpsertLive()
    {
        var ct = TestContext.Current.CancellationToken;
        var group = await ResolveAttributeGroupCodeAsync(ct);

        var attributes = await Context.BulkCreateOrUpdateAttributesAsync(
        [
            new AkeneoAttribute { Code = "oa_test_perm_select", Type = "pim_catalog_simpleselect", Group = group },
            new AkeneoAttribute { Code = "oa_test_perm_text", Type = "pim_catalog_text", Group = group }
        ], ct);
        AssertAllSucceeded(attributes, 2);

        var options = await Context.BulkCreateOrUpdateAttributeOptionsAsync("oa_test_perm_select",
        [
            new AttributeOption { Code = "opt_a", Labels = new() { ["en_US"] = "A" } },
            new AttributeOption { Code = "opt_b", Labels = new() { ["en_US"] = "B" } }
        ], ct);
        AssertAllSucceeded(options, 2);

        var groups = await Context.BulkCreateOrUpdateAttributeGroupsAsync(
            [new AttributeGroup { Code = "oa_test_perm_group", Labels = new() { ["en_US"] = "OA test group" } }], ct);
        AssertAllSucceeded(groups, 1);
    }

    // -------------------------------------------------------------------------
    // Bulk: families and family variants (perm fixtures; variants need axes)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Bulk_Families_And_Variants_UpsertLive()
    {
        var ct = TestContext.Current.CancellationToken;
        var group = await ResolveAttributeGroupCodeAsync(ct);
        var identifier = await ResolveIdentifierAttributeCodeAsync(ct);

        // Self-contained prerequisites: every attribute the family references must exist
        // (tests must not depend on sibling tests having run first).
        await Context.CreateOrUpdateAttributeAsync(
            new AkeneoAttribute { Code = "oa_test_perm_axis", Type = "pim_catalog_simpleselect", Group = group }, ct);
        await Context.CreateOrUpdateAttributeOptionAsync("oa_test_perm_axis",
            new AttributeOption { Code = "axis_a", Labels = new() { ["en_US"] = "Axis A" } }, ct);
        await Context.CreateOrUpdateAttributeAsync(
            new AkeneoAttribute { Code = "oa_test_perm_text", Type = "pim_catalog_text", Group = group }, ct);

        var families = await Context.BulkCreateOrUpdateFamiliesAsync(
        [
            new Family
            {
                Code = "oa_test_perm_vfam",
                AttributeAsLabel = identifier,
                Attributes = [identifier, "oa_test_perm_axis", "oa_test_perm_text"]
            }
        ], ct);
        AssertAllSucceeded(families, 1);

        try
        {
            var variants = await Context.BulkCreateOrUpdateFamilyVariantsAsync("oa_test_perm_vfam",
            [
                new FamilyVariant
                {
                    Code = "oa_test_perm_variant",
                    Labels = new() { ["en_US"] = "OA test variant" },
                    VariantAttributeSets =
                    [
                        new FamilyVariantAttributeSet { Level = 1, Axes = ["oa_test_perm_axis"], Attributes = ["oa_test_perm_axis"] }
                    ]
                }
            ], ct);
            AssertAllSucceeded(variants, 1);
        }
        catch (AkeneoApiException ex) when (ex.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            Assert.Skip($"Variant modeling rejected by tenant rules: {ex.ApiMessage}");
        }
    }

    // -------------------------------------------------------------------------
    // Bulk: association types, categories, channels
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Bulk_AssociationTypes_UpsertLive()
    {
        var ct = TestContext.Current.CancellationToken;
        var results = await Context.BulkCreateOrUpdateAssociationTypesAsync(
            [new AssociationType { Code = "OA_TEST_PERM_ASSOC", Labels = new() { ["en_US"] = "OA test association" } }], ct);
        AssertAllSucceeded(results, 1);
    }

    [Fact]
    public async Task Bulk_Categories_UpsertLive()
    {
        var ct = TestContext.Current.CancellationToken;

        // Categories need an existing parent; resolve a root category from the tenant.
        var page = await Context.GetCategoryListAsync(1, 100, ct: ct);
        var root = page.Categories.FirstOrDefault(c => string.IsNullOrEmpty(c.Parent));
        if (root is null)
            Assert.Skip("Tenant has no root category to attach a test category to.");

        var results = await Context.BulkCreateOrUpdateCategoriesAsync(
            [new Category { Code = "oa_test_perm_cat", Parent = root.Code, Labels = new() { ["en_US"] = "OA test category" } }], ct);
        AssertAllSucceeded(results, 1);
    }

    [Fact]
    public async Task Bulk_Channels_UpsertLive()
    {
        var ct = TestContext.Current.CancellationToken;

        // Channels cannot be deleted; use one fixed perm channel modelled on an existing one
        // so the tree/locale/currency references are valid for this tenant.
        var existing = (await Context.GetChannelListAsync(1, 1, ct: ct)).Channels.FirstOrDefault();
        if (existing is null)
            Assert.Skip("Tenant has no channel to use as a template.");

        var results = await Context.BulkCreateOrUpdateChannelsAsync(
        [
            new Channel
            {
                Code = "oa_test_perm_channel",
                CategoryTree = existing.CategoryTree,
                Currencies = existing.Currencies,
                Locales = existing.Locales,
                Labels = new() { ["en_US"] = "OA test channel" }
            }
        ], ct);
        AssertAllSucceeded(results, 1);
    }

    // -------------------------------------------------------------------------
    // Bulk: products (uuid), product models — deletable, RunId codes
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Bulk_ProductUuids_CreatesLive_ThenDeletes()
    {
        var ct = TestContext.Current.CancellationToken;
        var uuid1 = Guid.NewGuid().ToString();
        var uuid2 = Guid.NewGuid().ToString();
        try
        {
            var results = await Context.BulkCreateOrUpdateProductUuidsAsync(
                [new ProductUuid { Uuid = uuid1 }, new ProductUuid { Uuid = uuid2, Enabled = false }], ct);
            AssertAllSucceeded(results, 2);
            Assert.Equal(uuid1, results[0].Key);
        }
        finally
        {
            await TryDeleteAsync(() => Context.DeleteProductUuidAsync(uuid1, ct));
            await TryDeleteAsync(() => Context.DeleteProductUuidAsync(uuid2, ct));
        }
    }

    [Fact]
    public async Task Bulk_ProductModels_UpsertLive_WhenModelingAvailable()
    {
        var ct = TestContext.Current.CancellationToken;

        var existing = await Context.GetProductModelListAsync(1, 1, ct: ct);
        if (existing.ProductModels.Count == 0)
            Assert.Skip("Tenant has no product models to derive a family variant from.");
        var template = existing.ProductModels[0];

        var code = $"{RunId}_bulk_pm";
        try
        {
            var results = await Context.BulkCreateOrUpdateProductModelsAsync(
                [new ProductModel { Code = code, Family = template.Family, FamilyVariant = template.FamilyVariant }], ct);

            if (results[0].StatusCode == 422)
                Assert.Skip($"Minimal product model rejected by tenant modeling rules: {results[0].Message}");
            AssertAllSucceeded(results, 1);
        }
        finally
        {
            await TryDeleteAsync(() => Context.DeleteProductModelAsync(code, ct));
        }
    }

    // -------------------------------------------------------------------------
    // Bulk: reference entity records + the cross-entity read
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Bulk_ReferenceEntityRecords_UpsertLive_And_CrossEntityReadFindsThem()
    {
        var ct = TestContext.Current.CancellationToken;

        await Context.CreateOrUpdateReferenceEntityAsync(
            new ReferenceEntity { Code = "oa_test_perm_re", Labels = new() { ["en_US"] = "OA test entity" } }, ct);

        var results = await Context.BulkCreateOrUpdateReferenceEntityRecordsAsync("oa_test_perm_re",
        [
            new ReferenceEntityRecord { Code = "oa_rec_a" },
            new ReferenceEntityRecord { Code = "oa_rec_b" }
        ], ct);
        AssertAllSucceeded(results, 2);

        // The per-entity records list is strongly consistent — sanity-check the writes landed.
        var direct = await Context.GetReferenceEntityRecordListAsync("oa_test_perm_re", ct: ct);
        Assert.Contains(direct.ReferenceEntityRecords, r => r.Code == "oa_rec_a");

        // The cross-entity records endpoint (GET /reference-entities/records) is search-index
        // backed and eventually consistent — poll briefly for the index to catch up.
        var codes = new List<string>();
        for (var attempt = 0; attempt < 10; attempt++)
        {
            codes.Clear();
            await foreach (var record in Context.StreamAllReferenceEntityRecordsAsync("oa_test_perm_re", ct: ct))
                codes.Add(record.Code);
            if (codes.Contains("oa_rec_a") && codes.Contains("oa_rec_b"))
                break;
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        Assert.Contains("oa_rec_a", codes);
        Assert.Contains("oa_rec_b", codes);
    }

    // -------------------------------------------------------------------------
    // Bulk: assets — deletable, RunId codes
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Bulk_Assets_CreatesLive_ThenDeletes()
    {
        var ct = TestContext.Current.CancellationToken;
        await Context.CreateOrUpdateAssetFamilyAsync(new AssetFamily { Code = "oa_test_perm_af" }, ct);

        var code1 = $"{RunId}_bulk_asset_1";
        var code2 = $"{RunId}_bulk_asset_2";
        try
        {
            var results = await Context.BulkCreateOrUpdateAssetsAsync("oa_test_perm_af",
                [new Asset { Code = code1 }, new Asset { Code = code2 }], ct);
            AssertAllSucceeded(results, 2);
        }
        finally
        {
            await TryDeleteAsync(() => Context.DeleteAssetAsync("oa_test_perm_af", code1, ct));
            await TryDeleteAsync(() => Context.DeleteAssetAsync("oa_test_perm_af", code2, ct));
        }
    }

    // -------------------------------------------------------------------------
    // Streaming download round-trip
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StreamingDownload_RoundTripsBytes()
    {
        var ct = TestContext.Current.CancellationToken;

        var code = await Context.UploadAssetMediaFileAsync(TinyPng, "oa_test_stream.png", "image/png", ct);

        await using var stream = await Context.DownloadAssetMediaFileStreamAsync(code, ct);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);

        Assert.Equal(TinyPng, buffer.ToArray());
    }

    // -------------------------------------------------------------------------
    // Rule definitions — reads (Rules Engine may be feature-gated)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RuleDefinitions_List_ReturnsOrSkips()
    {
        var ct = TestContext.Current.CancellationToken;
        try
        {
            var list = await Context.GetRuleDefinitionListAsync(1, 10, ct: ct);
            Assert.NotNull(list.RuleDefinitions);

            if (list.RuleDefinitions.Count > 0)
            {
                var single = await Context.GetRuleDefinitionAsync(list.RuleDefinitions[0].Code, ct);
                Assert.Equal(list.RuleDefinitions[0].Code, single.Code);
            }
        }
        catch (AkeneoApiException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound or HttpStatusCode.Unauthorized)
        {
            Assert.Skip($"Rules Engine not available with current token: {ex.ApiMessage}");
        }
    }

    // -------------------------------------------------------------------------
    // Helpers (same conventions as LifecycleTests)
    // -------------------------------------------------------------------------

    private async Task<string> ResolveIdentifierAttributeCodeAsync(CancellationToken ct)
    {
        try
        {
            var sku = await Context.GetAttributeAsync("sku", ct);
            if (sku.Type == "pim_catalog_identifier")
                return sku.Code;
        }
        catch (AkeneoApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
        }

        await foreach (var attribute in Context.StreamAttributesAsync(ct: ct))
        {
            if (attribute.Type == "pim_catalog_identifier")
                return attribute.Code;
        }

        throw new InvalidOperationException("Tenant has no pim_catalog_identifier attribute.");
    }

    private async Task<string> ResolveAttributeGroupCodeAsync(CancellationToken ct)
    {
        var groups = await Context.GetAttributeGroupListAsync(1, 100, ct: ct);
        var other = groups.AttributeGroups.FirstOrDefault(g => g.Code == "other") ?? groups.AttributeGroups.FirstOrDefault();
        return other?.Code ?? throw new InvalidOperationException("Tenant has no attribute groups.");
    }

    private static async Task TryDeleteAsync(Func<Task> delete)
    {
        try { await delete(); }
        catch (AkeneoApiException) { }
    }
}

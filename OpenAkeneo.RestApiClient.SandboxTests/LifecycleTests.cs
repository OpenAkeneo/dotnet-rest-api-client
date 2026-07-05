using System.Net;
using System.Text.Json;
using OpenAkeneo.RestApiClient;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

/// <summary>
/// WP4 (test-plan.md) — CRUD lifecycle tests closing the P1 sandbox gaps from
/// test-coverage-matrix.md: delete flows (products, assets, catalogs), the POST-then-GET create
/// flow, media upload with the spec-required product part, the asset media download round-trip
/// (review finding 1.7), the fetched-entity PATCH round-trip (finding 1.6), measurement-family
/// upsert, and asset family/attribute/option upserts.
/// Conventions: run-scoped codes are prefixed <c>oa_test_</c> and deleted in <c>finally</c>;
/// resources the API cannot delete use fixed <c>oa_test_perm_*</c> codes updated idempotently.
/// </summary>
public class LifecycleTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;
    private AkeneoContext Context => _fixture.Context;

    /// <summary>Codes for deletable resources created by this run.</summary>
    private static readonly string RunId = $"oa_test_{DateTime.UtcNow:yyyyMMddHHmmss}";

    /// <summary>A minimal valid 1×1 PNG.</summary>
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    public LifecycleTests(TestBase fixture) => _fixture = fixture;

    // -------------------------------------------------------------------------
    // Products — POST create (incl. server-generated UUID), update, delete → 404
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ProductUuid_PostWithoutUuid_ResolvesGeneratedUuid_ThenDeletes()
    {
        var ct = TestContext.Current.CancellationToken;
        ProductUuid? created = null;
        try
        {
            // uuid deliberately null: the server generates it and the client must resolve it
            // from the 201 Location header (review finding 1.8).
            created = await Context.CreateProductUuidAsync(new ProductUuid { Enabled = false }, ct);
            Assert.False(string.IsNullOrEmpty(created.Uuid));
        }
        finally
        {
            if (created?.Uuid is { } uuid)
                await Context.DeleteProductUuidAsync(uuid, ct);
        }

        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() => Context.GetProductUuidAsync(created!.Uuid!, ct));
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task ProductIdentifier_CreateUpdateDelete_Lifecycle()
    {
        var ct = TestContext.Current.CancellationToken;
        var identifier = $"{RunId}_sku";
        var deleted = false;
        try
        {
            // POST create → 201 empty body → follow-up GET (the create flow fixed in Phase B).
            var created = await Context.CreateProductIdentifierAsync(
                new ProductIdentifier { Identifier = identifier, Enabled = false }, ct);
            Assert.Equal(identifier, created.Identifier);
            Assert.False(created.Enabled);

            // PATCH the FETCHED entity back with one change — this is the finding 1.6 probe:
            // the body carries server-managed fields (created/updated); the API must accept it.
            created.Enabled = true;
            var updated = await Context.CreateOrUpdateProductIdentifierAsync(created, ct);
            Assert.True(updated.Enabled);

            await Context.DeleteProductIdentifierAsync(identifier, ct);
            deleted = true;

            var ex = await Assert.ThrowsAsync<AkeneoApiException>(() => Context.GetProductIdentifierAsync(identifier, ct));
            Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        }
        finally
        {
            if (!deleted)
                await TryDeleteAsync(() => Context.DeleteProductIdentifierAsync(identifier, ct));
        }
    }

    [Fact]
    public async Task ProductModel_CreateDelete_WhenModelingAvailable()
    {
        var ct = TestContext.Current.CancellationToken;

        // Creating a product model needs an existing family variant and its axes values, which
        // are tenant-specific. Borrow the structure from an existing model; skip when the tenant
        // has none or the axes make a minimal clone invalid.
        var existing = await Context.GetProductModelListAsync(1, 1, ct: ct);
        if (existing.ProductModels.Count == 0)
            Assert.Skip("Tenant has no product models to derive a family variant from.");

        var template = existing.ProductModels[0];
        var code = $"{RunId}_pm";
        try
        {
            await Context.CreateProductModelAsync(new ProductModel
            {
                Code = code,
                Family = template.Family,
                FamilyVariant = template.FamilyVariant
            }, ct);
        }
        catch (AkeneoApiException ex) when (ex.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            Assert.Skip($"Minimal product model rejected by tenant modeling rules: {ex.ApiMessage}");
        }

        await Context.DeleteProductModelAsync(code, ct);
        var notFound = await Assert.ThrowsAsync<AkeneoApiException>(() => Context.GetProductModelAsync(code, ct));
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Media upload with the spec-required product part (review finding 2.1)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UploadProductMediaFile_WithProductPart_LinksValueToProduct()
    {
        var ct = TestContext.Current.CancellationToken;

        var identifierAttribute = await ResolveIdentifierAttributeCodeAsync(ct);
        var groupCode = await ResolveAttributeGroupCodeAsync(ct);

        // Permanent prerequisites (not deletable via API) — idempotent upserts on fixed codes.
        await Context.CreateOrUpdateAttributeAsync(new AkeneoAttribute
        {
            Code = "oa_test_perm_image",
            Type = "pim_catalog_image",
            Group = groupCode,
            AllowedExtensions = ["png", "jpg"]
        }, ct);

        await Context.CreateOrUpdateFamilyAsync(new Family
        {
            Code = "oa_test_perm_family",
            AttributeAsLabel = identifierAttribute,
            Attributes = [identifierAttribute, "oa_test_perm_image"]
        }, ct);

        var identifier = $"{RunId}_media_sku";
        try
        {
            await Context.CreateOrUpdateProductIdentifierAsync(
                new ProductIdentifier { Identifier = identifier, Family = "oa_test_perm_family" }, ct);

            var productJson = $$"""{"identifier":"{{identifier}}","attribute":"oa_test_perm_image","scope":null,"locale":null}""";
            var code = await Context.UploadProductMediaFileAsync(TinyPng, "oa_test.png", "image/png", productJson, ct: ct);
            Assert.False(string.IsNullOrEmpty(code));

            // The upload must have landed on the product's image attribute value.
            var product = await Context.GetProductIdentifierAsync(identifier, ct);
            var value = product.Values?["oa_test_perm_image"].FirstOrDefault();
            Assert.NotNull(value);
            Assert.Equal(code, value!.GetStringData());

            // Round-trip the binary back down (product media download path).
            var downloaded = await Context.DownloadProductMediaFileAsync(code, ct);
            Assert.Equal(TinyPng, downloaded);
        }
        finally
        {
            await TryDeleteAsync(() => Context.DeleteProductIdentifierAsync(identifier, ct));
        }
    }

    // -------------------------------------------------------------------------
    // Assets — family/attribute/option upserts, media round-trip, delete → 404
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AssetFamily_Attribute_Option_UpsertsSucceed()
    {
        var ct = TestContext.Current.CancellationToken;

        var family = await Context.CreateOrUpdateAssetFamilyAsync(new AssetFamily
        {
            Code = "oa_test_perm_af",
            Labels = new() { ["en_US"] = "OA test asset family" }
        }, ct);
        Assert.Equal("oa_test_perm_af", family.Code);

        var attribute = await Context.CreateOrUpdateAssetAttributeAsync("oa_test_perm_af", new AssetAttribute
        {
            Code = "oa_test_tags",
            Type = "single_option",
            Labels = new() { ["en_US"] = "OA test tags" }
        }, ct);
        Assert.Equal("single_option", attribute.Type);

        var option = await Context.CreateOrUpdateAssetAttributeOptionAsync("oa_test_perm_af", "oa_test_tags",
            new AssetAttributeOption { Code = "opt_a", Labels = new() { ["en_US"] = "Option A" } }, ct);
        Assert.Equal("opt_a", option.Code);
    }

    [Fact]
    public async Task Asset_CreateDelete_Lifecycle()
    {
        var ct = TestContext.Current.CancellationToken;
        await Context.CreateOrUpdateAssetFamilyAsync(new AssetFamily { Code = "oa_test_perm_af" }, ct);

        var code = $"{RunId}_asset";
        var created = await Context.CreateOrUpdateAssetAsync("oa_test_perm_af", new Asset { Code = code }, ct);
        Assert.Equal(code, created.Code);

        await Context.DeleteAssetAsync("oa_test_perm_af", code, ct);

        var ex = await Assert.ThrowsAsync<AkeneoApiException>(() => Context.GetAssetAsync("oa_test_perm_af", code, ct));
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task AssetMediaFile_UploadThenDownload_RoundTripsBytes()
    {
        var ct = TestContext.Current.CancellationToken;

        var code = await Context.UploadAssetMediaFileAsync(TinyPng, "oa_test_roundtrip.png", "image/png", ct);
        Assert.False(string.IsNullOrEmpty(code));

        // Locks review finding 1.7 live: the code contains '/' segments that must survive
        // URL building for the download to resolve.
        Assert.Contains("/", code);
        var downloaded = await Context.DownloadAssetMediaFileAsync(code, ct);
        Assert.Equal(TinyPng, downloaded);
    }

    // -------------------------------------------------------------------------
    // Catalogs (feature-gated) — full lifecycle incl. duplicate and mapping schema
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Catalog_CreateUpdateDuplicateDelete_Lifecycle()
    {
        var ct = TestContext.Current.CancellationToken;
        string? catalogId = null;
        string? duplicateId = null;
        try
        {
            Catalog created;
            try
            {
                created = await Context.CreateCatalogAsync(new Catalog { Name = $"{RunId} catalog" }, ct);
            }
            catch (AkeneoApiException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound or HttpStatusCode.Unauthorized)
            {
                Assert.Skip($"Catalog for Apps feature not available with current token: {ex.ApiMessage}");
                return;
            }
            catalogId = created.Id;
            Assert.False(string.IsNullOrEmpty(catalogId));

            var fetched = await Context.GetCatalogAsync(catalogId!, ct);
            Assert.Equal($"{RunId} catalog", fetched.Name);

            fetched.Name = $"{RunId} renamed";
            var updated = await Context.UpdateCatalogAsync(catalogId!, fetched, ct);
            Assert.Equal($"{RunId} renamed", updated.Name);

            var duplicate = await Context.DuplicateCatalogAsync(catalogId!, ct);
            duplicateId = duplicate.Id;
            Assert.False(string.IsNullOrEmpty(duplicateId));
            Assert.NotEqual(catalogId, duplicateId);

            // Mapping schema set → get → delete on the same catalog.
            var schema = """
                {"$id":"https://example.com/oa-test","$schema":"https://api.akeneo.com/mapping/product/0.0.13/schema","type":"object","properties":{"uuid":{"type":"string","title":"UUID"}}}
                """;
            try
            {
                await Context.SetCatalogMappingSchemaAsync(catalogId!, schema, ct);
                var fetchedSchema = await Context.GetCatalogMappingSchemaAsync(catalogId!, ct);
                Assert.NotNull(fetchedSchema);
                await Context.DeleteCatalogMappingSchemaAsync(catalogId!, ct);
            }
            catch (AkeneoApiException ex) when (ex.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                // Schema meta-version drift is tenant-dependent; the set/get/delete request flow
                // itself is what this exercises.
                Assert.Skip($"Mapping schema version rejected by tenant: {ex.ApiMessage}");
            }
        }
        finally
        {
            if (duplicateId != null)
                await TryDeleteAsync(() => Context.DeleteCatalogAsync(duplicateId, ct));
            if (catalogId != null)
                await TryDeleteAsync(() => Context.DeleteCatalogAsync(catalogId, ct));
        }

        if (catalogId != null)
        {
            var ex = await Assert.ThrowsAsync<AkeneoApiException>(() => Context.GetCatalogAsync(catalogId, ct));
            Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        }
    }

    // -------------------------------------------------------------------------
    // Measurement families — upsert against the live API (known gap)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MeasurementFamilies_CreateOrUpdate_ReturnsPerItemSuccess()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await Context.CreateOrUpdateMeasurementFamiliesAsync(
        [
            new MeasurementFamily
            {
                Code = "oa_test_perm_mf",
                StandardUnitCode = "OA_TEST_UNIT",
                Labels = new() { ["en_US"] = "OA test measurement" },
                Units = new()
                {
                    ["OA_TEST_UNIT"] = new MeasurementUnit
                    {
                        Code = "OA_TEST_UNIT",
                        Symbol = "oat",
                        Labels = new() { ["en_US"] = "OA test unit" },
                        ConvertFromStandard = [new MeasurementConversion { Operator = "mul", Value = "1" }]
                    }
                }
            }
        ], ct);

        using var doc = JsonDocument.Parse(response);
        var statuses = doc.RootElement.EnumerateArray()
            .Select(item => item.GetProperty("status_code").GetInt32())
            .ToList();
        Assert.NotEmpty(statuses);
        Assert.All(statuses, status => Assert.True(status is 201 or 204, $"Unexpected per-item status {status}: {response}"));
    }

    // -------------------------------------------------------------------------
    // Bulk PATCH (0.9.0) — live validation of the NDJSON batch flow
    // -------------------------------------------------------------------------

    [Fact]
    public async Task BulkCreateOrUpdateProductIdentifiers_CreatesAndReportsPerItem_ThenDeletes()
    {
        var ct = TestContext.Current.CancellationToken;
        var id1 = $"{RunId}_bulk_1";
        var id2 = $"{RunId}_bulk_2";
        try
        {
            var results = await Context.BulkCreateOrUpdateProductIdentifiersAsync(
            [
                new ProductIdentifier { Identifier = id1 },
                new ProductIdentifier { Identifier = id2, Enabled = false }
            ], ct);

            Assert.Equal(2, results.Count);
            Assert.All(results, r => Assert.True(r.Succeeded, $"line {r.Line} ({r.Key}): {r.StatusCode} {r.Message}"));
            Assert.Equal(id1, results[0].Key);

            // Round-trip: both products exist; the second is disabled, the first got the
            // server default (enabled — locks the Enabled bool? change live).
            var p1 = await Context.GetProductIdentifierAsync(id1, ct);
            var p2 = await Context.GetProductIdentifierAsync(id2, ct);
            Assert.True(p1.Enabled);
            Assert.False(p2.Enabled);

            // Second bulk call updates instead of creating → 204s.
            var again = await Context.BulkCreateOrUpdateProductIdentifiersAsync([new ProductIdentifier { Identifier = id1 }], ct);
            Assert.Equal(204, again[0].StatusCode);
        }
        finally
        {
            await TryDeleteAsync(() => Context.DeleteProductIdentifierAsync(id1, ct));
            await TryDeleteAsync(() => Context.DeleteProductIdentifierAsync(id2, ct));
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>Resolves the tenant's identifier attribute code (usually <c>sku</c>).</summary>
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
            // fall through to the scan below
        }

        await foreach (var attribute in Context.StreamAttributesAsync(ct: ct))
        {
            if (attribute.Type == "pim_catalog_identifier")
                return attribute.Code;
        }

        throw new InvalidOperationException("Tenant has no pim_catalog_identifier attribute.");
    }

    /// <summary>Resolves an existing attribute group code (usually <c>other</c>).</summary>
    private async Task<string> ResolveAttributeGroupCodeAsync(CancellationToken ct)
    {
        var groups = await Context.GetAttributeGroupListAsync(1, 100, ct: ct);
        var other = groups.AttributeGroups.FirstOrDefault(g => g.Code == "other") ?? groups.AttributeGroups.FirstOrDefault();
        return other?.Code ?? throw new InvalidOperationException("Tenant has no attribute groups.");
    }

    /// <summary>Best-effort cleanup that never masks the primary test failure.</summary>
    private static async Task TryDeleteAsync(Func<Task> delete)
    {
        try { await delete(); }
        catch (AkeneoApiException) { /* already gone or feature-gated — cleanup is best effort */ }
    }
}

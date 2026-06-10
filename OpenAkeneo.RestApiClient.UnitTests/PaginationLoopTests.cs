using System.Net;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// Regression tests for keyset (<c>search_after</c>) streaming methods.
///
/// These endpoints paginate ONLY via <c>search_after</c> — the Akeneo API ignores any
/// <c>page</c> query parameter. A streamer that increments a page number instead of
/// advancing the cursor from the response's <c>next</c> link re-requests the first page
/// forever (the <c>next</c> link is always present), producing an infinite loop that
/// re-yields the same items. See the asset-streaming bug fixed in 0.8.0.
///
/// The tests guard against that in two ways:
///   1. The <see cref="FakeHttpHandler"/> queue holds a FINITE number of pages and throws
///      "ran out of queued responses" once exhausted. A runaway loop drains the queue and
///      fails loudly instead of hanging the test run.
///   2. Each request after the first is asserted to carry the cursor extracted from the
///      previous page's <c>next</c> link — proving the cursor actually advances.
/// </summary>
public class PaginationLoopTests
{
    private static readonly CancellationToken CT = TestContext.Current.CancellationToken;

    // -------------------------------------------------------------------------
    // HAL page builders
    // -------------------------------------------------------------------------

    /// <summary>
    /// A HAL list page with the given embedded items and, optionally, a <c>next</c> link
    /// carrying a <c>search_after</c> cursor. When <paramref name="nextCursor"/> is null the
    /// page has no <c>next</c> link (last page).
    /// </summary>
    private static string HalPage(string basePath, string? nextCursor, params string[] itemJson)
    {
        var items = string.Join(",", itemJson);
        var next = nextCursor is null
            ? ""
            : ",\"next\":{\"href\":\"" + basePath + "?limit=100&search_after=" + nextCursor + "\"}";
        return "{\"_links\":{\"self\":{\"href\":\"" + basePath + "\"}" + next
            + "},\"_embedded\":{\"items\":[" + items + "]}}";
    }

    private static string AssetJson(string code) =>
        "{\"code\":\"" + code + "\",\"values\":{}}";

    private static string ProductUuidJson(string uuid) =>
        "{\"uuid\":\"" + uuid + "\",\"enabled\":true,\"family\":null,\"categories\":[],\"groups\":[],\"parent\":null,\"values\":{}}";

    /// <summary>The non-token API requests received, in order.</summary>
    private static IReadOnlyList<CapturedRequest> ApiRequests(FakeHttpHandler handler) =>
        handler.Captured.Where(r => !r.RequestUri.Contains("/oauth/")).ToList();

    // -------------------------------------------------------------------------
    // StreamAssetsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StreamAssetsAsync_TerminatesAndAdvancesCursorAcrossPages()
    {
        const string path = "/api/rest/v1/asset-families/logo/assets";
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(HalPage(path, "cursorA", AssetJson("a1"), AssetJson("a2"))),
            FakeHttpHandler.Ok(HalPage(path, "cursorB", AssetJson("a3"), AssetJson("a4"))),
            FakeHttpHandler.Ok(HalPage(path, null, AssetJson("a5"))));   // last page: no next link

        var ctx = Helpers.BuildContext(handler);

        var codes = new List<string>();
        await foreach (var asset in ctx.StreamAssetsAsync("logo", ct: CT))
            codes.Add(asset.Code!);

        // Every queued page consumed exactly once — no duplicates, no runaway loop.
        Assert.Equal(new[] { "a1", "a2", "a3", "a4", "a5" }, codes);

        // Cursor advances: page 2 uses cursorA, page 3 uses cursorB; the page param is gone.
        var reqs = ApiRequests(handler);
        Assert.Equal(3, reqs.Count);
        Assert.DoesNotContain("page=", reqs[0].RequestUri);
        Assert.DoesNotContain("search_after", reqs[0].RequestUri);
        Assert.Contains("search_after=cursorA", reqs[1].RequestUri);
        Assert.Contains("search_after=cursorB", reqs[2].RequestUri);
    }

    [Fact]
    public async Task StreamAssetsAsync_StopsWhenNextLinkPresentButCursorMissing()
    {
        // Defensive guard: a malformed next link without a search_after value must not
        // re-request the same (cursorless) page forever.
        const string path = "/api/rest/v1/asset-families/logo/assets";
        var malformedNext = "{\"_links\":{\"self\":{\"href\":\"" + path + "\"},\"next\":{\"href\":\"" + path
            + "?limit=100\"}},\"_embedded\":{\"items\":[" + AssetJson("only") + "]}}";

        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(malformedNext));

        var ctx = Helpers.BuildContext(handler);

        var codes = new List<string>();
        await foreach (var asset in ctx.StreamAssetsAsync("logo", ct: CT))
            codes.Add(asset.Code!);

        Assert.Equal(new[] { "only" }, codes);
        Assert.Single(ApiRequests(handler));   // exactly one page, then stop
    }

    // -------------------------------------------------------------------------
    // StreamCatalogProductsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StreamCatalogProductsAsync_TerminatesAndAdvancesCursorAcrossPages()
    {
        const string path = "/api/rest/v1/catalogs/cat-1/products";
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(HalPage(path, "c1", ProductUuidJson("u1"))),
            FakeHttpHandler.Ok(HalPage(path, null, ProductUuidJson("u2"))));

        var ctx = Helpers.BuildContext(handler);

        var uuids = new List<string>();
        await foreach (var p in ctx.StreamCatalogProductsAsync("cat-1", CT))
            uuids.Add(p.Uuid!);

        Assert.Equal(new[] { "u1", "u2" }, uuids);

        var reqs = ApiRequests(handler);
        Assert.Equal(2, reqs.Count);
        Assert.DoesNotContain("page=", reqs[0].RequestUri);
        Assert.Contains("search_after=c1", reqs[1].RequestUri);
    }

    // -------------------------------------------------------------------------
    // StreamCatalogProductUuidsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task StreamCatalogProductUuidsAsync_TerminatesAndAdvancesCursorAcrossPages()
    {
        const string path = "/api/rest/v1/catalogs/cat-1/product-uuids";
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(HalPage(path, "c1", "\"u1\"")),
            FakeHttpHandler.Ok(HalPage(path, null, "\"u2\"")));

        var ctx = Helpers.BuildContext(handler);

        var uuids = new List<string>();
        await foreach (var u in ctx.StreamCatalogProductUuidsAsync("cat-1", CT))
            uuids.Add(u);

        Assert.Equal(new[] { "u1", "u2" }, uuids);

        var reqs = ApiRequests(handler);
        Assert.Equal(2, reqs.Count);
        Assert.DoesNotContain("page=", reqs[0].RequestUri);
        Assert.Contains("search_after=c1", reqs[1].RequestUri);
    }
}

namespace OpenAkeneo.RestApiClient.UnitTests;

public class TokenSemaphoreTests
{
    [Fact]
    public async Task ConcurrentCallsWithNoToken_FetchesTokenExactlyOnce()
    {
        // Token endpoint + enough API responses for both concurrent GET calls
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(),
            FakeHttpHandler.Ok());

        var svc = Helpers.BuildService(handler);
        var ct = TestContext.Current.CancellationToken;

        // Fire two concurrent API calls before any token exists in cache
        await Task.WhenAll(
            svc.HttpGetAsync("/api/rest/v1/products", ct),
            svc.HttpGetAsync("/api/rest/v1/categories", ct));

        // handler.CallCount = 1 token fetch + 2 API calls = 3 total
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task CachedToken_NotRefetchedOnSubsequentCall()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Ok(),
            FakeHttpHandler.Ok());

        var svc = Helpers.BuildService(handler);
        var ct = TestContext.Current.CancellationToken;

        await svc.HttpGetAsync("/api/rest/v1/products", ct);
        await svc.HttpGetAsync("/api/rest/v1/categories", ct);

        // Still only 1 token fetch
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task ForceRefresh_AlwaysFetchesNewToken()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse("token-1"),
            FakeHttpHandler.TokenResponse("token-2"));

        var svc = Helpers.BuildService(handler);

        var ct = TestContext.Current.CancellationToken;
        var first = await svc.GetTokenAsync(ct: ct);
        var second = await svc.GetTokenAsync(forceRefresh: true, ct: ct);

        Assert.Equal("token-1", first);
        Assert.Equal("token-2", second);
        Assert.Equal(2, handler.CallCount);
    }
}

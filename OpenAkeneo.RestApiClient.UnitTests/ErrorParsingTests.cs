using System.Net;

namespace OpenAkeneo.RestApiClient.UnitTests;

public class ErrorParsingTests
{
    [Fact]
    public async Task JsonBodyWithMessageField_UsesMessageAsApiMessage()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Json(HttpStatusCode.NotFound, """{"code":404,"message":"Product not found."}"""));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpGetAsync("/api/rest/v1/products/missing", TestContext.Current.CancellationToken));

        Assert.Equal("Product not found.", ex.ApiMessage);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task NonJsonBody_UsesRawBodyAsApiMessage()
    {
        // 403 is non-transient so Polly will not retry it
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.Forbidden, "Access denied"));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpGetAsync("/api/rest/v1/products", TestContext.Current.CancellationToken));

        Assert.Equal("Access denied", ex.ApiMessage);
    }

    [Fact]
    public async Task NonJsonBodyOver500Chars_IsTruncatedWithEllipsis()
    {
        var longBody = new string('x', 600);
        // 403 is non-transient so Polly will not retry it
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.Forbidden, longBody));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpGetAsync("/api/rest/v1/products", TestContext.Current.CancellationToken));

        Assert.Equal(501, ex.ApiMessage.Length); // 500 chars + "…"
        Assert.EndsWith("…", ex.ApiMessage);
    }

    [Fact]
    public async Task EmptyBody_ReturnsEmptyBodyPlaceholder()
    {
        // 403 is non-transient so Polly will not retry it
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.Forbidden, ""));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpGetAsync("/api/rest/v1/products", TestContext.Current.CancellationToken));

        Assert.Equal("(empty response body)", ex.ApiMessage);
    }
}

using System.Net;

namespace OpenAkeneo.RestApiClient.UnitTests;

public class RetryTests
{
    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    public async Task TransientFailureThenSuccess_ReturnsSuccessAfterRetry(HttpStatusCode transientCode)
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(transientCode, ""),
            FakeHttpHandler.Ok("""{"result":"ok"}"""));

        var svc = Helpers.BuildService(handler);
        var result = await svc.HttpGetAsync("/api/rest/v1/products");

        Assert.Contains("ok", result);
    }

    [Fact]
    public async Task PermanentFailureAfterAllRetries_ThrowsAkeneoApiException()
    {
        // 1 token fetch + 1 initial attempt + 5 retries = 7 handler calls total
        var responses = new List<HttpResponseMessage> { FakeHttpHandler.TokenResponse() };
        for (var i = 0; i < 6; i++)
            responses.Add(FakeHttpHandler.Text(HttpStatusCode.ServiceUnavailable, "down"));

        var handler = new FakeHttpHandler(responses.ToArray());
        var svc = Helpers.BuildService(handler);

        await Assert.ThrowsAsync<AkeneoApiException>(() => svc.HttpGetAsync("/api/rest/v1/products"));
        Assert.Equal(7, handler.CallCount);
    }
}

using System.Collections.Concurrent;
using System.Net;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// Replays a fixed sequence of responses and records how many times it was called.
/// Thread-safe so it can be used in concurrent token-semaphore tests.
/// </summary>
internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly ConcurrentQueue<HttpResponseMessage> _responses;
    private int _callCount;

    public int CallCount => _callCount;

    public FakeHttpHandler(params HttpResponseMessage[] responses)
    {
        _responses = new ConcurrentQueue<HttpResponseMessage>(responses);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);

        if (!_responses.TryDequeue(out var response))
            throw new InvalidOperationException("FakeHttpHandler ran out of queued responses.");

        return Task.FromResult(response);
    }

    internal static HttpResponseMessage TokenResponse(string accessToken = "test-token", int expiresIn = 3600)
    {
        var json = $$"""{"access_token":"{{accessToken}}","expires_in":{{expiresIn}},"token_type":"bearer","scope":null}""";
        return Json(HttpStatusCode.OK, json);
    }

    internal static HttpResponseMessage Json(HttpStatusCode status, string json)
        => new(status) { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };

    internal static HttpResponseMessage Text(HttpStatusCode status, string body)
        => new(status) { Content = new StringContent(body) };

    internal static HttpResponseMessage Ok(string json = "{}")
        => Json(HttpStatusCode.OK, json);
}

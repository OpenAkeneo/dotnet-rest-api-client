using System.Collections.Concurrent;
using System.Net;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>Captures the details of a single HTTP request for assertion in tests.</summary>
internal sealed record CapturedRequest(
    HttpMethod Method,
    string RequestUri,
    string? Body,
    string? ContentType);

/// <summary>
/// Replays a fixed sequence of responses and records how many times it was called.
/// Thread-safe so it can be used in concurrent token-semaphore tests.
/// </summary>
internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly ConcurrentQueue<HttpResponseMessage> _responses;
    private readonly ConcurrentQueue<CapturedRequest> _captured = new();
    private int _callCount;

    public int CallCount => _callCount;

    /// <summary>All requests received, in order (including the token request).</summary>
    public IReadOnlyList<CapturedRequest> Captured => _captured.ToArray();

    /// <summary>The last non-token API request received (skips /oauth/ calls).</summary>
    public CapturedRequest? LastApiRequest => _captured
        .Where(r => !r.RequestUri.Contains("/oauth/"))
        .LastOrDefault();

    public FakeHttpHandler(params HttpResponseMessage[] responses)
    {
        _responses = new ConcurrentQueue<HttpResponseMessage>(responses);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);

        string? body = null;
        string? contentType = null;
        if (request.Content is not null)
        {
            body = await request.Content.ReadAsStringAsync(cancellationToken);
            contentType = request.Content.Headers.ContentType?.MediaType;
        }

        _captured.Enqueue(new CapturedRequest(
            request.Method,
            request.RequestUri?.ToString() ?? string.Empty,
            body,
            contentType));

        if (!_responses.TryDequeue(out var response))
            throw new InvalidOperationException("FakeHttpHandler ran out of queued responses.");

        return response;
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

    internal static HttpResponseMessage Created(string json = "{}")
        => Json(HttpStatusCode.Created, json);

    internal static HttpResponseMessage CreatedWithLocation(string location)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Headers.Location = new Uri(location, UriKind.RelativeOrAbsolute);
        return response;
    }

    internal static HttpResponseMessage NoContent()
        => new(HttpStatusCode.NoContent);

    /// <summary>201 carrying the bare code in the dedicated <c>asset-media-file-code</c> header and no Location.</summary>
    internal static HttpResponseMessage CreatedWithMediaFileCodeHeader(string code)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Headers.TryAddWithoutValidation("asset-media-file-code", code);
        return response;
    }

    /// <summary>201 with neither a Location nor a code header — a successful status but no resolvable code.</summary>
    internal static HttpResponseMessage CreatedWithoutCode()
        => new(HttpStatusCode.Created);
}

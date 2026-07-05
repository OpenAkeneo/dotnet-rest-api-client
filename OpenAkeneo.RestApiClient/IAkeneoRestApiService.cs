using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient
{
    /// <summary>
    /// Low-level HTTP interface for the Akeneo REST API. Handles OAuth token management,
    /// authentication, retries, and raw HTTP operations.
    /// </summary>
    /// <remarks>
    /// AI assistants / LLM agents: see <c>llms.txt</c> (shipped in this package and at
    /// <c>https://raw.githubusercontent.com/OpenAkeneo/dotnet-rest-api-client/main/llms.txt</c>)
    /// for the full method-selection guide before using this raw interface — most tasks have a
    /// typed <c>AkeneoContext</c> method.
    /// </remarks>
    public interface IAkeneoRestApiService
    {
        /// <summary>Logical identifier for this connection.</summary>
        string ConnectionId { get; }

        /// <summary>Human-readable name for this connection.</summary>
        string ConnectionName { get; }

        /// <summary>Base URL of the Akeneo instance.</summary>
        string ConnectionUrl { get; }

        /// <summary>Returns a valid OAuth access token, fetching or refreshing as needed.</summary>
        Task<string> GetTokenAsync(bool forceRefresh = false, CancellationToken ct = default);

        /// <summary>Performs a GET request and returns the response body as a string.</summary>
        Task<string> HttpGetAsync(string url, CancellationToken ct = default);

        /// <summary>Performs a GET request with additional headers and returns the response body as a string.</summary>
        Task<string> HttpGetAsync(string url, Dictionary<string, string> headers, CancellationToken ct = default);

        /// <summary>Performs a GET request and returns the response body as raw bytes.</summary>
        Task<byte[]> HttpGetBytesAsync(string url, CancellationToken ct = default);

        /// <summary>Performs a GET request and returns the response body as an unbuffered stream (dispose it to release the HTTP response).</summary>
        Task<Stream> HttpGetStreamAsync(string url, CancellationToken ct = default);

        /// <summary>Performs a POST request and returns the response body as a string.</summary>
        Task<string> HttpPostAsync(string url, string content, CancellationToken ct = default);

        /// <summary>Performs a POST request with additional headers and returns the response body as a string.</summary>
        Task<string> HttpPostAsync(string url, string? content, Dictionary<string, string>? headers, CancellationToken ct = default);

        /// <summary>Performs a POST request and returns the HTTP status code, response body, and Location header.</summary>
        Task<(System.Net.HttpStatusCode StatusCode, string Body, string? Location)> HttpPostWithLocationAsync(string url, string content, CancellationToken ct = default);

        /// <summary>Performs a PATCH request and returns the response body as a string.</summary>
        Task<string> HttpPatchAsync(string url, string content, CancellationToken ct = default);

        /// <summary>Performs a PATCH request with additional headers and returns the response body as a string.</summary>
        Task<string> HttpPatchAsync(string url, string? content, Dictionary<string, string>? headers, CancellationToken ct = default);

        /// <summary>Performs a PATCH request with an explicit content type (e.g. Akeneo's <c>application/vnd.akeneo.collection+json</c> batch format).</summary>
        Task<string> HttpPatchAsync(string url, string content, string contentType, CancellationToken ct = default);

        /// <summary>Performs a PATCH request and returns the HTTP status code and response body.</summary>
        Task<(System.Net.HttpStatusCode StatusCode, string Body)> HttpPatchWithStatusAsync(string url, string content, CancellationToken ct = default);

        /// <summary>Performs a DELETE request and returns the response body as a string.</summary>
        Task<string> HttpDeleteAsync(string url, CancellationToken ct = default);

        /// <summary>Performs a DELETE request with additional headers and returns the response body as a string.</summary>
        Task<string> HttpDeleteAsync(string url, Dictionary<string, string>? headers, CancellationToken ct = default);

        /// <summary>Performs a PUT request with a JSON body and returns the response body as a string.</summary>
        Task<string> HttpPutAsync(string url, string content, CancellationToken ct = default);

        /// <summary>
        /// Performs a POST request with a multipart/form-data body (file upload) and returns the created
        /// media-file code resolved from the response headers. <paramref name="extraParts"/> carries
        /// additional string form parts (e.g. the <c>product</c> part required by <c>POST /media-files</c>).
        /// </summary>
        Task<string> HttpPostMultipartAsync(string url, string fieldName, byte[] fileBytes, string fileName, string contentType, IReadOnlyDictionary<string, string>? extraParts = null, CancellationToken ct = default);

        /// <summary>Performs a multipart/form-data POST and returns the raw response body (no media-file code extraction).</summary>
        Task<string> HttpPostMultipartForBodyAsync(string url, string fieldName, byte[] fileBytes, string fileName, string contentType, IReadOnlyDictionary<string, string>? extraParts = null, CancellationToken ct = default);
    }
}

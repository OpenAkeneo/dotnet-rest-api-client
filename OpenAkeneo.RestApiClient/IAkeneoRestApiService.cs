using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient
{
    /// <summary>
    /// Low-level HTTP interface for the Akeneo REST API. Handles OAuth token management,
    /// authentication, retries, and raw HTTP operations.
    /// </summary>
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

        /// <summary>Performs a POST request and returns the response body as a string.</summary>
        Task<string> HttpPostAsync(string url, string content, CancellationToken ct = default);

        /// <summary>Performs a POST request with additional headers and returns the response body as a string.</summary>
        Task<string> HttpPostAsync(string url, string? content, Dictionary<string, string>? headers, CancellationToken ct = default);

        /// <summary>Performs a PATCH request and returns the response body as a string.</summary>
        Task<string> HttpPatchAsync(string url, string content, CancellationToken ct = default);

        /// <summary>Performs a PATCH request with additional headers and returns the response body as a string.</summary>
        Task<string> HttpPatchAsync(string url, string? content, Dictionary<string, string>? headers, CancellationToken ct = default);

        /// <summary>Performs a PATCH request and returns the HTTP status code and response body.</summary>
        Task<(System.Net.HttpStatusCode StatusCode, string Body)> HttpPatchWithStatusAsync(string url, string content, CancellationToken ct = default);

        /// <summary>Performs a DELETE request and returns the response body as a string.</summary>
        Task<string> HttpDeleteAsync(string url, CancellationToken ct = default);

        /// <summary>Performs a DELETE request with additional headers and returns the response body as a string.</summary>
        Task<string> HttpDeleteAsync(string url, Dictionary<string, string>? headers, CancellationToken ct = default);

        /// <summary>Performs a PUT request with a JSON body and returns the response body as a string.</summary>
        Task<string> HttpPutAsync(string url, string content, CancellationToken ct = default);

        /// <summary>Performs a POST request with a multipart/form-data body (file upload) and returns the response body as a string.</summary>
        Task<string> HttpPostMultipartAsync(string url, string fieldName, byte[] fileBytes, string fileName, string contentType, CancellationToken ct = default);
    }
}

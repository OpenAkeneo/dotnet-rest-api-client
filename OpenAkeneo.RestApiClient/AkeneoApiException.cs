using System.Collections.Generic;
using System.Net;

namespace OpenAkeneo.RestApiClient
{
    /// <summary>A single per-field validation error returned by Akeneo in a 422 response.</summary>
    public class AkeneoFieldError
    {
        /// <summary>The attribute or property that failed validation.</summary>
        public string? Property { get; init; }

        /// <summary>Human-readable description of the validation failure.</summary>
        public string? Message { get; init; }
    }

    /// <summary>
    /// Exception thrown by <see cref="AkeneoRestApiService"/> when the Akeneo API
    /// returns a non-success HTTP response, or when a response cannot be parsed.
    /// </summary>
    public class AkeneoApiException : Exception
    {
        /// <summary>The URL that produced the error.</summary>
        public string RequestUrl { get; }

        /// <summary>The HTTP method used (GET, POST, PATCH, etc.). May be null for parse errors.</summary>
        public string? RequestMethod { get; }

        /// <summary>The HTTP status code returned by the server.</summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The Akeneo error message extracted from the response body.
        /// For well-formed Akeneo JSON errors this is the <c>"message"</c> field value.
        /// For non-JSON responses (e.g. 502 HTML gateway pages) this is the raw body string,
        /// truncated to 500 characters.
        /// </summary>
        public string ApiMessage { get; }

        /// <summary>The raw, untruncated response body. Useful for debugging unexpected responses.</summary>
        public string? ResponseBody { get; }

        /// <summary>Selected response headers (e.g. <c>X-Request-Id</c>, <c>X-Correlation-Id</c>) for correlation.</summary>
        public IReadOnlyDictionary<string, string>? ResponseHeaders { get; }

        /// <summary>
        /// Per-field validation errors from a 422 response. Non-null and non-empty only when
        /// Akeneo returned a structured <c>"errors"</c> array (e.g. attribute validation failures).
        /// </summary>
        public IReadOnlyList<AkeneoFieldError>? FieldErrors { get; }

        /// <summary>
        /// Initialises a new <see cref="AkeneoApiException"/>.
        /// </summary>
        /// <param name="requestUrl">The URL that returned the error.</param>
        /// <param name="requestMethod">The HTTP method used.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="apiMessage">The error message from the response body.</param>
        /// <param name="responseBody">The full, untruncated response body (optional).</param>
        /// <param name="responseHeaders">Selected response headers (optional).</param>
        /// <param name="fieldErrors">Per-field validation errors (optional).</param>
        /// <param name="innerException">Inner exception, if any.</param>
        public AkeneoApiException(
            string requestUrl,
            string requestMethod,
            HttpStatusCode statusCode,
            string apiMessage,
            string? responseBody = null,
            IReadOnlyDictionary<string, string>? responseHeaders = null,
            IReadOnlyList<AkeneoFieldError>? fieldErrors = null,
            Exception? innerException = null)
            : base($"Akeneo API error {(int)statusCode} ({statusCode}) on '{requestMethod} {requestUrl}': {apiMessage}", innerException)
        {
            RequestUrl = requestUrl;
            RequestMethod = requestMethod;
            StatusCode = statusCode;
            ApiMessage = apiMessage;
            ResponseBody = responseBody;
            ResponseHeaders = responseHeaders;
            FieldErrors = fieldErrors;
        }
    }
}

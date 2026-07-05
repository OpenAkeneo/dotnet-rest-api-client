using OpenAkeneo.RestApiClient.Models;
using System.Net;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    internal static class AkeneoContextHelpers
    {
        /// <summary>
        /// Validates that <paramref name="page"/> is at least 1 and <paramref name="limit"/> is between 1 and 100 (inclusive).
        /// </summary>
        internal static void ValidatePagination(int page, int limit)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, 100, nameof(limit));
        }

        /// <summary>
        /// Validates that <paramref name="limit"/> is between 1 and 100 (inclusive).
        /// Used by endpoints that paginate via <c>search_after</c> and have no page number.
        /// </summary>
        internal static void ValidateLimit(int limit)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit, nameof(limit));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, 100, nameof(limit));
        }

        /// <summary>
        /// Builds a URL query string (including the leading '?') from a dictionary of parameters.
        /// Returns an empty string when the dictionary is null or empty.
        /// </summary>
        internal static string BuildQueryString(Dictionary<string, string>? queryParameters)
        {
            if (queryParameters == null || queryParameters.Count == 0)
                return string.Empty;

            return "?" + string.Join("&", queryParameters.Select(
                x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
        }

        /// <summary>
        /// Deserializes <paramref name="responseString"/> to <typeparamref name="T"/>.
        /// Throws <see cref="AkeneoApiException"/> with a clear message when the JSON is null, empty, or malformed.
        /// </summary>
        internal static T DeserializeOrThrow<T>(string responseString, string requestUrl)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(responseString)
                    ?? throw new AkeneoApiException(requestUrl, "GET", HttpStatusCode.OK, "Empty or null JSON response");
            }
            catch (JsonException ex)
            {
                throw new AkeneoApiException(requestUrl, "GET", HttpStatusCode.OK,
                    $"Failed to parse Akeneo response: {ex.Message}", responseString, innerException: ex);
            }
        }

        /// <summary>
        /// Extracts the <c>search_after</c> cursor value from a HAL <c>next</c> link URL.
        /// Returns <c>null</c> when the URL carries no cursor.
        /// </summary>
        internal static string? ExtractSearchAfter(string url)
        {
            var idx = url.IndexOf("search_after=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            idx += "search_after=".Length;
            var end = url.IndexOf('&', idx);
            return Uri.UnescapeDataString(end < 0 ? url[idx..] : url[idx..end]);
        }

        /// <summary>
        /// Extracts the trailing path segment from a <c>Location</c> header value (absolute or
        /// relative), e.g. the server-generated UUID of a created resource. Returns <c>null</c>
        /// when no segment can be resolved.
        /// </summary>
        internal static string? ExtractLastPathSegment(string? location)
        {
            if (string.IsNullOrEmpty(location))
                return null;

            var path = Uri.TryCreate(location, UriKind.Absolute, out var abs) ? abs.AbsolutePath : location;
            var queryIdx = path.IndexOf('?');
            if (queryIdx >= 0)
                path = path[..queryIdx];
            path = path.TrimEnd('/');

            var idx = path.LastIndexOf('/');
            var segment = path[(idx + 1)..];
            return string.IsNullOrEmpty(segment) ? null : Uri.UnescapeDataString(segment);
        }

        /// <summary>
        /// Deserializes a HAL response string and extracts the embedded "items" array as a typed list
        /// in a single deserialization pass. Returns an empty list when no embedded items are present.
        /// </summary>
        internal static (HalLinks? Links, List<T> Items) ParseHalResponse<T>(string responseString, string requestUrl)
        {
            var responseJson = DeserializeOrThrow<HalResponse<T>>(responseString, requestUrl);
            return (responseJson.Links, responseJson.Embedded?.Items ?? new List<T>());
        }
    }
}

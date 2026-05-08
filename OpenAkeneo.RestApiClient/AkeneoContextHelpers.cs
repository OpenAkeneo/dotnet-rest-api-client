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
        /// Deserializes a HAL response string and extracts the embedded "items" array as a typed list.
        /// Returns an empty list when no embedded items are present.
        /// </summary>
        internal static (HalLinks? Links, List<T> Items) ParseHalResponse<T>(string responseString, string requestUrl)
        {
            var responseJson = DeserializeOrThrow<HalResponse>(responseString, requestUrl);
            var items = new List<T>();

            if (responseJson.Embedded != null && responseJson.Embedded.TryGetValue("items", out var itemsElement))
                items = itemsElement.EnumerateArray().Select(x => x.Deserialize<T>()!).ToList();

            return (responseJson.Links, items);
        }
    }
}

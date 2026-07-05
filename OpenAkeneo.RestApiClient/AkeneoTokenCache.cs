using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient
{
    /// <summary>
    /// OAuth2 token state cached between requests.
    /// </summary>
    internal class TokenData
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("expires_at")]
        public DateTimeOffset ExpiresAt { get; set; }
    }

    /// <summary>
    /// Holds the cached OAuth2 token and the lock that serialises token refreshes.
    /// <para>
    /// Register a single instance per connection (as <c>AddAkeneoClient</c> does) so that all
    /// <see cref="AkeneoRestApiService"/> instances created for that connection — including the
    /// transient instances produced by <see cref="System.Net.Http.IHttpClientFactory"/> — share
    /// one token instead of each fetching their own. When no cache is supplied,
    /// <see cref="AkeneoRestApiService"/> creates a private one, giving per-instance caching
    /// (the pre-existing behaviour for directly constructed services).
    /// </para>
    /// </summary>
    public sealed class AkeneoTokenCache : IDisposable
    {
        internal SemaphoreSlim TokenLock { get; } = new(1, 1);

        internal TokenData? TokenData { get; set; }

        /// <summary>Set once the token file has been read (successfully or not) so it is not re-read on every call.</summary>
        internal bool FileLoadAttempted { get; set; }

        /// <inheritdoc/>
        public void Dispose() => TokenLock.Dispose();
    }
}

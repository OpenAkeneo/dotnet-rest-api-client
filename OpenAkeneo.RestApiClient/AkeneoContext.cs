using OpenAkeneo.RestApiClient.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    /// <summary>
    /// High-level Akeneo API client. Provides typed methods for every Akeneo REST API resource
    /// (products, assets, attributes, categories, channels, families, reference entities, etc.).
    /// Internally delegates all HTTP calls to an <see cref="IAkeneoRestApiService"/> instance.
    /// <para>
    /// For long-lived applications, prefer <c>AddAkeneoClient</c> (DI extension) so an
    /// <see cref="System.Net.Http.IHttpClientFactory"/>-managed handler is used for DNS rotation.
    /// </para>
    /// </summary>
    public partial class AkeneoContext
    {
        private readonly IAkeneoRestApiService _service;

        /// <summary>
        /// Initialises the context from settings using a plain <see cref="System.Net.Http.HttpClient"/>.
        /// Suitable for short-lived scripts and console apps. For long-lived services use
        /// <c>AddAkeneoClient</c> so handler rotation is managed by <see cref="System.Net.Http.IHttpClientFactory"/>.
        /// </summary>
        /// <param name="settings">Akeneo connection settings.</param>
        /// <param name="logger">Optional logger for HTTP and token lifecycle tracing.</param>
        public AkeneoContext(AkeneoRestApiSettings settings, ILogger<AkeneoRestApiService>? logger = null)
            : this(new AkeneoRestApiService(new HttpClient(), settings, logger)) { }

        /// <summary>
        /// Initialises the context with the provided <see cref="IAkeneoRestApiService"/>.
        /// </summary>
        /// <param name="service">A configured service instance used for all HTTP calls.</param>
        public AkeneoContext(IAkeneoRestApiService service)
        {
            ArgumentNullException.ThrowIfNull(service);
            _service = service;
        }

        /// <summary>Exposes the underlying low-level service for advanced token and HTTP operations.</summary>
        public IAkeneoRestApiService Service => _service;

        /// <summary>Identifies this connection (mapped from the API credentials id or username).</summary>
        public string ConnectionId => _service.ConnectionId;
        /// <summary>Human-readable name for this connection (mapped from the API credentials name or REST API URL).</summary>
        public string ConnectionName => _service.ConnectionName;
        /// <summary>Base REST API URL for this connection.</summary>
        public string ConnectionUrl => _service.ConnectionUrl;

        /// <summary>
        /// Sends a PATCH to <paramref name="url"/> with <paramref name="body"/>, then fetches the
        /// current state via <paramref name="fetchAsync"/>. Akeneo returns 201 on create or 204 on
        /// update — in both cases the response body is not reliable (201 returns pre-patch state),
        /// so a GET is always performed to return the definitive post-patch resource.
        /// </summary>
        internal async Task<T> PatchAndFetchAsync<T>(string url, string body, Func<Task<T>> fetchAsync, CancellationToken ct = default)
        {
            await _service.HttpPatchWithStatusAsync(url, body, ct).ConfigureAwait(false);
            return await fetchAsync().ConfigureAwait(false);
        }
    }
}

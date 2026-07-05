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
    /// <remarks>
    /// AI assistants / LLM agents: a complete machine-oriented reference (method selection guide,
    /// per-domain pages with signatures and gotchas) ships with this package as <c>llms.txt</c>
    /// and <c>docs/*.md</c> (in the NuGet cache under
    /// <c>~/.nuget/packages/openakeneo.restapiclient/&lt;version&gt;/</c>), and online at
    /// <c>https://raw.githubusercontent.com/OpenAkeneo/dotnet-rest-api-client/main/llms.txt</c>.
    /// Read llms.txt before choosing methods.
    /// </remarks>
    public partial class AkeneoContext : IDisposable
    {
        private readonly IAkeneoRestApiService _service;
        private readonly HttpClient? _ownedHttpClient;

        /// <summary>
        /// Initialises the context from settings using a plain <see cref="System.Net.Http.HttpClient"/>
        /// owned (and disposed) by this context. Suitable for short-lived scripts and console apps.
        /// For long-lived services use <c>AddAkeneoClient</c> so handler rotation is managed by
        /// <see cref="System.Net.Http.IHttpClientFactory"/>.
        /// </summary>
        /// <param name="settings">Akeneo connection settings.</param>
        /// <param name="logger">Optional logger for HTTP and token lifecycle tracing.</param>
        public AkeneoContext(AkeneoRestApiSettings settings, ILogger<AkeneoRestApiService>? logger = null)
        {
            _ownedHttpClient = new HttpClient();
            _service = new AkeneoRestApiService(_ownedHttpClient, settings, logger);
        }

        /// <summary>
        /// Initialises the context with the provided <see cref="IAkeneoRestApiService"/>.
        /// The service is not owned by the context and will not be disposed with it.
        /// </summary>
        /// <param name="service">A configured service instance used for all HTTP calls.</param>
        public AkeneoContext(IAkeneoRestApiService service)
        {
            ArgumentNullException.ThrowIfNull(service);
            _service = service;
        }

        /// <summary>
        /// Disposes the <see cref="System.Net.Http.HttpClient"/> and service this context created
        /// (settings-based constructor only). A service supplied by the caller is left untouched.
        /// </summary>
        public void Dispose()
        {
            if (_ownedHttpClient is null)
                return;
            (_service as IDisposable)?.Dispose();
            _ownedHttpClient.Dispose();
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

using OpenAkeneo.RestApiClient.Models;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    /// <summary>
    /// Low-level HTTP client for the Akeneo REST API. Handles OAuth2 token acquisition,
    /// caching, and transparent refresh on 401 responses. Automatically retries transient
    /// failures and 429 Too Many Requests responses with exponential back-off and jitter
    /// via a Polly resilience pipeline. POST requests are only retried on 429/408 (responses
    /// that guarantee the server did not process the request) so non-idempotent operations
    /// such as media uploads and job launches are never replayed after an ambiguous failure.
    /// Inject an optional <see cref="ILogger{AkeneoRestApiService}"/> to trace all HTTP
    /// activity and token lifecycle events.
    /// </summary>
    public class AkeneoRestApiService : IAkeneoRestApiService, IDisposable
    {

        #region Classes, variables and constructors

        private const string _tokenUrl = "/api/oauth/v1/token";
        private const double _tokenEarlyExpirationFraction = 0.75;
        private readonly AkeneoTokenCache _tokenCache;
        private readonly bool _ownsTokenCache;


        private readonly HttpClient _httpClient;
        private readonly AkeneoRestApiSettings _settings;
        private readonly ILogger<AkeneoRestApiService>? _logger;
        private readonly ResiliencePipeline<HttpResponseMessage> _idempotentPipeline;
        private readonly ResiliencePipeline<HttpResponseMessage> _nonIdempotentPipeline;


        /// <summary>
        /// Initialises the service with a pre-configured <see cref="HttpClient"/> and Akeneo credentials.
        /// </summary>
        /// <param name="httpClient">An <see cref="HttpClient"/> whose <c>BaseAddress</c> may be left unset;
        /// the service prepends <see cref="AkeneoRestApiSettings.RestApiUrl"/> when needed.</param>
        /// <param name="settings">Connection settings including credentials and API base URL.</param>
        /// <param name="logger">Optional logger for HTTP request/response and token lifecycle tracing.</param>
        /// <param name="tokenCache">Optional shared token cache. Supply one instance per connection
        /// (as <c>AddAkeneoClient</c> does) so transient service instances share a single token.
        /// When omitted, the token is cached per service instance.</param>
        public AkeneoRestApiService(HttpClient httpClient, AkeneoRestApiSettings settings, ILogger<AkeneoRestApiService>? logger = null, AkeneoTokenCache? tokenCache = null)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(settings);
            settings.Validate();

            _httpClient = httpClient;
            _settings = settings;
            _logger = logger;
            _ownsTokenCache = tokenCache is null;
            _tokenCache = tokenCache ?? new AkeneoTokenCache();

            // Retry strategy aligned with Akeneo good-practices:
            // https://api.akeneo.com/documentation/good-practices.html#retry-strategy
            // - 5 retries (within the 5–8 recommended range)
            // - Exponential back-off starting at 500 ms, capped at 30 s, with jitter
            // - Retry-After header is respected automatically by HttpRetryStrategyOptions
            //
            // Two pipelines with different retry predicates:
            // - Idempotent (GET/HEAD/PUT/DELETE/PATCH — Akeneo PATCH is an upsert, so a replay
            //   yields the same state): 429, 408, and transient 5xx / network faults.
            // - Non-idempotent (POST — creates, job launches, media uploads): only 429 and 408,
            //   where the server by definition did not process the request. A 5xx or a dropped
            //   connection after the body was sent may mean the operation WAS applied, and
            //   replaying it could create duplicates.
            _idempotentPipeline = BuildRetryPipeline(retryTransient: true, logger);
            _nonIdempotentPipeline = BuildRetryPipeline(retryTransient: false, logger);
        }

        private static ResiliencePipeline<HttpResponseMessage> BuildRetryPipeline(bool retryTransient, ILogger? logger)
        {
            var retryOptions = new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 5,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500),
                MaxDelay = TimeSpan.FromSeconds(30),
                ShouldHandle = args =>
                {
                    var status = args.Outcome.Result?.StatusCode;
                    // Safe for every verb: the server did not process the request.
                    if (status is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout) // 429 / 408
                        return ValueTask.FromResult(true);
                    if (!retryTransient)
                        return ValueTask.FromResult(false);
                    // Idempotent verbs additionally retry transient 5xx and network faults.
                    return ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome));
                },
                OnRetry = args =>
                {
                    logger?.LogWarning(
                        "Retry {Attempt}/{Max} after {Delay:g} due to {Outcome}",
                        args.AttemptNumber + 1,
                        5,
                        args.RetryDelay,
                        args.Outcome.Result?.StatusCode.ToString() ?? args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            };

            return new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(retryOptions)
                .Build();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_ownsTokenCache)
                _tokenCache.Dispose();
        }

        #endregion


        #region Connection

        /// <summary>Identifies this connection (mapped from the API credentials id or username).</summary>
        public string ConnectionId => _settings.Id ?? _settings.Username;
        /// <summary>Human-readable name for this connection (mapped from the API credentials name or REST API URL).</summary>
        public string ConnectionName => _settings.Name ?? _settings.RestApiUrl;
        /// <summary>Base REST API URL for this connection.</summary>
        public string ConnectionUrl => _settings.RestApiUrl;

        #endregion


        #region Request methods

        private ResiliencePipeline<HttpResponseMessage> PipelineFor(HttpMethod method)
            => method == HttpMethod.Post ? _nonIdempotentPipeline : _idempotentPipeline;

        private async Task<HttpResponseMessage> ExecuteWithPipelineAsync(
            HttpMethod method,
            string requestUrl,
            Dictionary<string, string>? headers,
            string? jsonString,
            string token,
            CancellationToken ct,
            string contentType = "application/json")
        {
            return await PipelineFor(method).ExecuteAsync(async ct2 =>
            {
                using var request = new HttpRequestMessage(method, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (!request.Headers.Contains(header.Key))
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                if (!request.Headers.Contains("Accept") && method == HttpMethod.Get)
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");

                if (!string.IsNullOrEmpty(jsonString))
                    request.Content = new StringContent(jsonString, Encoding.UTF8, contentType);

                return await _httpClient.SendAsync(request, ct2).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
        }

        private async Task<string> PerformRequestString(HttpMethod method, string url, Dictionary<string, string>? headers, string? jsonString, CancellationToken ct = default)
        {
            var (_, body, _) = await PerformRequestWithMetaAsync(method, url, headers, jsonString, ct).ConfigureAwait(false);
            return body;
        }

        private async Task<(HttpStatusCode StatusCode, string Body, string? Location)> PerformRequestWithMetaAsync(HttpMethod method, string url, Dictionary<string, string>? headers, string? jsonString, CancellationToken ct = default, string contentType = "application/json")
        {
            ArgumentException.ThrowIfNullOrEmpty(url);

            var requestUrl = url.StartsWith("http") ? url : _settings.RestApiUrl + url;

            var token = await GetTokenAsync(ct: ct).ConfigureAwait(false);
            using var response = await ExecuteWithPipelineAsync(method, requestUrl, headers, jsonString, token, ct, contentType).ConfigureAwait(false);
            _logger?.LogDebug("HTTP {Method} {Url} → {StatusCode}", method.Method, requestUrl, (int)response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                token = await GetTokenAfter401Async(token, ct).ConfigureAwait(false);
                using var retryResponse = await ExecuteWithPipelineAsync(method, requestUrl, headers, jsonString, token, ct, contentType).ConfigureAwait(false);
                _logger?.LogDebug("HTTP {Method} {Url} → {StatusCode} (after token refresh)", method.Method, requestUrl, (int)retryResponse.StatusCode);
                return await ReadResponseWithMetaAsync(retryResponse, method.Method, requestUrl, ct).ConfigureAwait(false);
            }

            return await ReadResponseWithMetaAsync(response, method.Method, requestUrl, ct).ConfigureAwait(false);
        }

        private async Task<(HttpStatusCode, string, string?)> ReadResponseWithMetaAsync(HttpResponseMessage response, string method, string requestUrl, CancellationToken ct)
        {
            var (status, body) = await ReadResponseAsync(response, method, requestUrl, ct).ConfigureAwait(false);
            return (status, body, response.Headers.Location?.ToString());
        }

        private async Task<(HttpStatusCode, string)> ReadResponseAsync(HttpResponseMessage response, string method, string requestUrl, CancellationToken ct)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var (apiMessage, fieldErrors) = ParseAkeneoError(body);
                _logger?.LogError("API error {StatusCode} for {Method} {Url}: {ApiMessage}", (int)response.StatusCode, method, requestUrl, apiMessage);
                throw new AkeneoApiException(requestUrl, method, response.StatusCode, apiMessage, body, fieldErrors: fieldErrors);
            }

            return (response.StatusCode, await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false));
        }

        private async Task<byte[]> PerformRequestBinary(string url, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(url);

            var requestUrl = url.StartsWith("http") ? url : _settings.RestApiUrl + url;

            var token = await GetTokenAsync(ct: ct).ConfigureAwait(false);
            using var response = await ExecuteWithPipelineAsync(HttpMethod.Get, requestUrl, null, null, token, ct).ConfigureAwait(false);
            _logger?.LogDebug("HTTP GET {Url} → {StatusCode}", requestUrl, (int)response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                token = await GetTokenAfter401Async(token, ct).ConfigureAwait(false);
                using var retryResponse = await ExecuteWithPipelineAsync(HttpMethod.Get, requestUrl, null, null, token, ct).ConfigureAwait(false);
                _logger?.LogDebug("HTTP GET {Url} → {StatusCode} (after token refresh)", requestUrl, (int)retryResponse.StatusCode);
                return await ReadBinaryResponseAsync(retryResponse, requestUrl, ct).ConfigureAwait(false);
            }

            return await ReadBinaryResponseAsync(response, requestUrl, ct).ConfigureAwait(false);
        }

        private async Task<byte[]> ReadBinaryResponseAsync(HttpResponseMessage response, string requestUrl, CancellationToken ct)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var (apiMessage, fieldErrors) = ParseAkeneoError(body);
                _logger?.LogError("API error {StatusCode} for GET {Url}: {ApiMessage}", (int)response.StatusCode, requestUrl, apiMessage);
                throw new AkeneoApiException(requestUrl, "GET", response.StatusCode, apiMessage, body, fieldErrors: fieldErrors);
            }

            return await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Parses a human-readable error message and optional per-field validation errors from
        /// an Akeneo API response body. Handles the standard Akeneo JSON error envelope:
        /// <c>{"code": 422, "message": "...", "errors": [{"property": "...", "message": "..."}]}</c>
        /// </summary>
        private static (string message, IReadOnlyList<AkeneoFieldError>? fieldErrors) ParseAkeneoError(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return ("(empty response body)", null);

            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body);
                if (json == null)
                    return (body.Length > 500 ? body[..500] + "…" : body, null);

                var message = json.TryGetValue("message", out var msg)
                    ? msg.GetString() ?? body
                    : body.Length > 500 ? body[..500] + "…" : body;

                IReadOnlyList<AkeneoFieldError>? fieldErrors = null;
                if (json.TryGetValue("errors", out var errorsEl) && errorsEl.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<AkeneoFieldError>();
                    foreach (var item in errorsEl.EnumerateArray())
                    {
                        list.Add(new AkeneoFieldError
                        {
                            Property = item.TryGetProperty("property", out var prop) ? prop.GetString() : null,
                            Message  = item.TryGetProperty("message",  out var m)    ? m.GetString()    : null,
                        });
                    }
                    if (list.Count > 0) fieldErrors = list;
                }

                return (message, fieldErrors);
            }
            catch (JsonException) { }

            return (body.Length > 500 ? body[..500] + "…" : body, null);
        }

        #endregion


        #region Token management

        private static bool IsTokenValid(TokenData? token)
            => token != null && !string.IsNullOrEmpty(token.AccessToken) && DateTimeOffset.UtcNow < token.ExpiresAt;

        /// <summary>
        /// Returns a valid Bearer access token, refreshing it transparently when it has
        /// reached 75% of its lifetime or when <paramref name="forceRefresh"/> is <c>true</c>.
        /// Thread-safe: concurrent callers block on the token cache's semaphore so only one
        /// refresh request is ever in-flight at a time. When the service was created with a
        /// shared <see cref="AkeneoTokenCache"/> (as <c>AddAkeneoClient</c> does) the token is
        /// shared across all service instances for the connection.
        /// </summary>
        /// <param name="forceRefresh">When <c>true</c>, always fetches a new token regardless of cache state.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A valid Bearer access token string.</returns>
        /// <exception cref="AkeneoApiException">Thrown when the token endpoint returns an error or an unusable response.</exception>
        public async Task<string> GetTokenAsync(bool forceRefresh = false, CancellationToken ct = default)
        {
            if (!forceRefresh && IsTokenValid(_tokenCache.TokenData))
                return _tokenCache.TokenData!.AccessToken!;

            await _tokenCache.TokenLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (!forceRefresh)
                {
                    if (IsTokenValid(_tokenCache.TokenData))
                        return _tokenCache.TokenData!.AccessToken!;

                    var fromFile = await TryLoadTokenFromFileAsync(ct).ConfigureAwait(false);
                    if (IsTokenValid(fromFile))
                    {
                        _tokenCache.TokenData = fromFile;
                        return fromFile!.AccessToken!;
                    }
                }

                return await FetchAndStoreTokenAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                _tokenCache.TokenLock.Release();
            }
        }

        /// <summary>
        /// Called when a request came back 401 with <paramref name="failedToken"/>. If another
        /// caller already refreshed the token in the meantime, returns the new cached token
        /// instead of fetching yet another one — N concurrent 401s produce one refresh, not N.
        /// </summary>
        private async Task<string> GetTokenAfter401Async(string failedToken, CancellationToken ct)
        {
            _logger?.LogWarning("Received 401 Unauthorized, triggering token refresh for ClientId {ClientId}", _settings.ClientId);

            await _tokenCache.TokenLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var cached = _tokenCache.TokenData;
                if (IsTokenValid(cached) && cached!.AccessToken != failedToken)
                    return cached.AccessToken!;

                return await FetchAndStoreTokenAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                _tokenCache.TokenLock.Release();
            }
        }

        /// <summary>Fetches, persists (best effort) and caches a new token. Must be called under the token lock.</summary>
        private async Task<string> FetchAndStoreTokenAsync(CancellationToken ct)
        {
            _logger?.LogInformation("Fetching new token for ClientId {ClientId}", _settings.ClientId);
            var tokenData = await FetchNewTokenAsync(ct).ConfigureAwait(false);

            _logger?.LogInformation("Token acquired for ClientId {ClientId}, expires at {ExpiresAt:u}", _settings.ClientId, tokenData.ExpiresAt);

            var tokenFilePath = TokenFilePath();
            if (!string.IsNullOrEmpty(tokenFilePath))
            {
                // Best effort: a failure to persist the token must not fail the API call —
                // the token in memory is valid regardless.
                try
                {
                    var json = JsonSerializer.Serialize(tokenData);
                    var tempPath = tokenFilePath + ".tmp";
                    await File.WriteAllTextAsync(tempPath, json, ct).ConfigureAwait(false);
                    File.Move(tempPath, tokenFilePath, overwrite: true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    _logger?.LogWarning(ex, "Failed to persist token to {TokenFilePath} — continuing with the in-memory token", tokenFilePath);
                }
            }

            _tokenCache.TokenData = tokenData;
            return tokenData.AccessToken!;
        }

        private string? TokenFilePath()
            => _settings.TokenFilePath is { } tfp ? string.Format(tfp, _settings.ClientId) : null;

        /// <summary>Reads a previously persisted token, once per cache lifetime. Must be called under the token lock.</summary>
        private async Task<TokenData?> TryLoadTokenFromFileAsync(CancellationToken ct)
        {
            if (_tokenCache.FileLoadAttempted)
                return null;
            _tokenCache.FileLoadAttempted = true;

            var tokenFilePath = TokenFilePath();
            if (string.IsNullOrEmpty(tokenFilePath) || !File.Exists(tokenFilePath))
                return null;

            try
            {
                var loaded = JsonSerializer.Deserialize<TokenData>(await File.ReadAllTextAsync(tokenFilePath, ct).ConfigureAwait(false));
                if (loaded != null)
                    _logger?.LogDebug("Token loaded from file for ClientId {ClientId}", _settings.ClientId);
                return loaded;
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                _logger?.LogWarning(ex, "Failed to load token from file {TokenFilePath} — will fetch a new token", tokenFilePath);
                return null;
            }
        }

        private async Task<TokenData> FetchNewTokenAsync(CancellationToken ct = default)
        {
            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
            var requestUrl = _settings.RestApiUrl + _tokenUrl;

            // The token POST is replay-safe (it only issues a token), so it uses the full
            // transient-retry pipeline rather than the restricted POST pipeline.
            var response = await _idempotentPipeline.ExecuteAsync(async ct2 =>
            {
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", _settings.Username),
                    new KeyValuePair<string, string>("password", _settings.Password),
                });

                using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);
                request.Content = formData;

                return await _httpClient.SendAsync(request, ct2).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);

            using (response)
            {
                var responseContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var (apiMessage, _) = ParseAkeneoError(responseContent);
                    _logger?.LogError("Token request failed with {StatusCode} for ClientId {ClientId}: {ApiMessage}", (int)response.StatusCode, _settings.ClientId, apiMessage);
                    throw new AkeneoApiException(requestUrl, "POST", response.StatusCode, apiMessage, responseContent);
                }

                try
                {
                    var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);

                    var tokenData = new TokenData
                    {
                        AccessToken = tokenResponse!["access_token"].GetString(),
                        ExpiresIn = tokenResponse["expires_in"].GetInt32()
                    };

                    if (string.IsNullOrEmpty(tokenData.AccessToken))
                        throw new KeyNotFoundException("access_token was empty");

                    tokenData.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenData.ExpiresIn * _tokenEarlyExpirationFraction);

                    return tokenData;
                }
                catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
                {
                    throw new AkeneoApiException(requestUrl, "POST", response.StatusCode,
                        $"Token endpoint returned an unusable response: {ex.Message}", responseContent, innerException: ex);
                }
            }
        }

        #endregion


        #region Get (string)

        /// <summary>Performs an authenticated HTTP GET and returns the response body as a string.</summary>
        /// <param name="url">Relative or absolute URL. Relative URLs are prepended with <see cref="AkeneoRestApiSettings.RestApiUrl"/>.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpGetAsync(string url, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Get, url, null, null, ct).ConfigureAwait(false);
        }

        /// <summary>Performs an authenticated HTTP GET with additional request headers and returns the response body as a string.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpGetAsync(string url, Dictionary<string, string> headers, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Get, url, headers, null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get stream

        /// <summary>
        /// Performs an authenticated HTTP GET and returns the response body as a <see cref="Stream"/>
        /// without buffering it in memory (for large media downloads). Dispose the returned stream
        /// to release the underlying HTTP response.
        /// </summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A stream over the response body; disposing it disposes the HTTP response.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<Stream> HttpGetStreamAsync(string url, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(url);

            var requestUrl = url.StartsWith("http") ? url : _settings.RestApiUrl + url;

            var token = await GetTokenAsync(ct: ct).ConfigureAwait(false);
            var response = await ExecuteStreamWithPipelineAsync(requestUrl, token, ct).ConfigureAwait(false);
            _logger?.LogDebug("HTTP GET (stream) {Url} → {StatusCode}", requestUrl, (int)response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                token = await GetTokenAfter401Async(token, ct).ConfigureAwait(false);
                response = await ExecuteStreamWithPipelineAsync(requestUrl, token, ct).ConfigureAwait(false);
                _logger?.LogDebug("HTTP GET (stream) {Url} → {StatusCode} (after token refresh)", requestUrl, (int)response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                using (response)
                {
                    var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                    var (apiMessage, fieldErrors) = ParseAkeneoError(body);
                    _logger?.LogError("API error {StatusCode} for GET {Url}: {ApiMessage}", (int)response.StatusCode, requestUrl, apiMessage);
                    throw new AkeneoApiException(requestUrl, "GET", response.StatusCode, apiMessage, body, fieldErrors: fieldErrors);
                }
            }

            var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            return new ResponseOwningStream(stream, response);
        }

        private async Task<HttpResponseMessage> ExecuteStreamWithPipelineAsync(string requestUrl, string token, CancellationToken ct)
        {
            return await _idempotentPipeline.ExecuteAsync(async ct2 =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Headers-read: the body is streamed by the caller instead of buffered here.
                return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct2).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
        }

        /// <summary>A pass-through stream that keeps the HTTP response alive until the stream is disposed.</summary>
        private sealed class ResponseOwningStream : Stream
        {
            private readonly Stream _inner;
            private readonly HttpResponseMessage _response;

            public ResponseOwningStream(Stream inner, HttpResponseMessage response)
            {
                _inner = inner;
                _response = response;
            }

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => _inner.CanSeek;
            public override bool CanWrite => false;
            public override long Length => _inner.Length;
            public override long Position { get => _inner.Position; set => _inner.Position = value; }
            public override void Flush() => _inner.Flush();
            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
            public override int Read(Span<byte> buffer) => _inner.Read(buffer);
            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _inner.ReadAsync(buffer, cancellationToken);
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _inner.ReadAsync(buffer, offset, count, cancellationToken);
            public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _inner.Dispose();
                    _response.Dispose();
                }
                base.Dispose(disposing);
            }

            public override async ValueTask DisposeAsync()
            {
                await _inner.DisposeAsync().ConfigureAwait(false);
                _response.Dispose();
                await base.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region Get bytes (byte[])

        /// <summary>Performs an authenticated HTTP GET and returns the raw response bytes (e.g., for media file downloads).</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw response bytes.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<byte[]> HttpGetBytesAsync(string url, CancellationToken ct = default)
        {
            return await PerformRequestBinary(url, ct).ConfigureAwait(false);
        }

        #endregion

        #region Post (string)

        /// <summary>Performs an authenticated HTTP POST with a JSON body.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">JSON request body.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpPostAsync(string url, string content, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Post, url, null, content, ct).ConfigureAwait(false);
        }

        /// <summary>Performs an authenticated HTTP POST with a JSON body and additional headers.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">JSON request body (may be <c>null</c>).</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpPostAsync(string url, string? content, Dictionary<string, string>? headers, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Post, url, headers, content, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs an authenticated HTTP POST with a JSON body and returns the HTTP status code,
        /// response body, and the <c>Location</c> response header (the URI of a created resource, if any).
        /// </summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">JSON request body.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The HTTP status code, response body string, and Location header value (or <c>null</c>).</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<(HttpStatusCode StatusCode, string Body, string? Location)> HttpPostWithLocationAsync(string url, string content, CancellationToken ct = default)
        {
            return await PerformRequestWithMetaAsync(HttpMethod.Post, url, null, content, ct).ConfigureAwait(false);
        }

        #endregion

        #region Patch (string)

        /// <summary>Performs an authenticated HTTP PATCH with a JSON body.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">JSON request body.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string (typically empty for Akeneo PATCH responses).</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpPatchAsync(string url, string content, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Patch, url, null, content, ct).ConfigureAwait(false);
        }

        /// <summary>Performs an authenticated HTTP PATCH with a JSON body and additional headers.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">JSON request body (may be <c>null</c>).</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpPatchAsync(string url, string? content, Dictionary<string, string>? headers, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Patch, url, headers, content, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs an authenticated HTTP PATCH with an explicit request content type. Used for
        /// Akeneo's batch endpoints, which take newline-delimited JSON with
        /// <c>Content-Type: application/vnd.akeneo.collection+json</c>.
        /// </summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">Request body.</param>
        /// <param name="contentType">MIME type for the request body.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpPatchAsync(string url, string content, string contentType, CancellationToken ct = default)
        {
            var (_, body, _) = await PerformRequestWithMetaAsync(HttpMethod.Patch, url, null, content, ct, contentType).ConfigureAwait(false);
            return body;
        }

        /// <summary>Performs an authenticated HTTP PATCH and returns both the HTTP status code and response body.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">JSON request body.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The HTTP status code and response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<(HttpStatusCode StatusCode, string Body)> HttpPatchWithStatusAsync(string url, string content, CancellationToken ct = default)
        {
            var (status, body, _) = await PerformRequestWithMetaAsync(HttpMethod.Patch, url, null, content, ct).ConfigureAwait(false);
            return (status, body);
        }

        #endregion

        #region Delete (string)

        /// <summary>Performs an authenticated HTTP DELETE.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string (typically empty).</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpDeleteAsync(string url, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Delete, url, null, null, ct).ConfigureAwait(false);
        }

        /// <summary>Performs an authenticated HTTP DELETE with additional headers.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="headers">Additional headers to include in the request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpDeleteAsync(string url, Dictionary<string, string>? headers, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Delete, url, headers, null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Put (string)

        /// <summary>Performs an authenticated HTTP PUT with a JSON body.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">JSON request body.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpPutAsync(string url, string content, CancellationToken ct = default)
        {
            return await PerformRequestString(HttpMethod.Put, url, null, content, ct).ConfigureAwait(false);
        }

        #endregion

        #region Post multipart (file upload)

        /// <summary>Performs an authenticated HTTP POST with a multipart/form-data body for file uploads.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="fieldName">The multipart field name for the file part.</param>
        /// <param name="fileBytes">Raw file bytes to upload.</param>
        /// <param name="fileName">Original file name sent in the Content-Disposition header.</param>
        /// <param name="contentType">MIME type of the file (e.g. <c>image/jpeg</c>).</param>
        /// <param name="extraParts">Optional additional string form parts, e.g. the <c>product</c> JSON part
        /// required by <c>POST /media-files</c> to link the upload to a product attribute value.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The media file code extracted from the <c>asset-media-file-code</c> or <c>Location</c>
        /// response header (e.g. <c>3/b/5/a/3b5a8c...filename.png</c>).</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes, or on a 2xx response
        /// that carries no resolvable media-file code.</exception>
        public async Task<string> HttpPostMultipartAsync(string url, string fieldName, byte[] fileBytes, string fileName, string contentType, IReadOnlyDictionary<string, string>? extraParts = null, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(url);

            var requestUrl = url.StartsWith("http") ? url : _settings.RestApiUrl + url;

            var token = await GetTokenAsync(ct: ct).ConfigureAwait(false);
            using var response = await ExecuteMultipartWithPipelineAsync(requestUrl, fieldName, fileBytes, fileName, contentType, extraParts, token, ct).ConfigureAwait(false);
            _logger?.LogDebug("HTTP POST (multipart) {Url} → {StatusCode}", requestUrl, (int)response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                token = await GetTokenAfter401Async(token, ct).ConfigureAwait(false);
                using var retryResponse = await ExecuteMultipartWithPipelineAsync(requestUrl, fieldName, fileBytes, fileName, contentType, extraParts, token, ct).ConfigureAwait(false);
                _logger?.LogDebug("HTTP POST (multipart) {Url} → {StatusCode} (after token refresh)", requestUrl, (int)retryResponse.StatusCode);
                await ReadResponseAsync(retryResponse, "POST", requestUrl, ct).ConfigureAwait(false);
                return ExtractMediaFileCode(retryResponse, requestUrl);
            }

            await ReadResponseAsync(response, "POST", requestUrl, ct).ConfigureAwait(false);
            return ExtractMediaFileCode(response, requestUrl);
        }

        /// <summary>
        /// Performs an authenticated multipart/form-data POST and returns the raw response body
        /// (unlike <see cref="HttpPostMultipartAsync"/>, no media-file code is extracted — used for
        /// multipart endpoints that respond with a JSON body, e.g. UI-extension file updates).
        /// </summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="fieldName">The multipart field name for the file part.</param>
        /// <param name="fileBytes">Raw file bytes to upload.</param>
        /// <param name="fileName">Original file name sent in the Content-Disposition header.</param>
        /// <param name="contentType">MIME type of the file.</param>
        /// <param name="extraParts">Additional string form parts.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpPostMultipartForBodyAsync(string url, string fieldName, byte[] fileBytes, string fileName, string contentType, IReadOnlyDictionary<string, string>? extraParts = null, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(url);

            var requestUrl = url.StartsWith("http") ? url : _settings.RestApiUrl + url;

            var token = await GetTokenAsync(ct: ct).ConfigureAwait(false);
            using var response = await ExecuteMultipartWithPipelineAsync(requestUrl, fieldName, fileBytes, fileName, contentType, extraParts, token, ct).ConfigureAwait(false);
            _logger?.LogDebug("HTTP POST (multipart) {Url} → {StatusCode}", requestUrl, (int)response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                token = await GetTokenAfter401Async(token, ct).ConfigureAwait(false);
                using var retryResponse = await ExecuteMultipartWithPipelineAsync(requestUrl, fieldName, fileBytes, fileName, contentType, extraParts, token, ct).ConfigureAwait(false);
                var (_, retryBody) = await ReadResponseAsync(retryResponse, "POST", requestUrl, ct).ConfigureAwait(false);
                return retryBody;
            }

            var (_, body) = await ReadResponseAsync(response, "POST", requestUrl, ct).ConfigureAwait(false);
            return body;
        }

        // Akeneo's POST /api/rest/v1/asset-media-files and /api/rest/v1/media-files return 201 with an
        // empty body. The created file code is carried in two places (per the Akeneo REST API spec):
        //   • the dedicated 'asset-media-file-code' response header — the bare code, and
        //   • the 'Location' header — the URI of the created resource, whose trailing path is the code.
        // The instance may send an absolute Location (https://host/api/rest/v1/asset-media-files/<code>)
        // or a relative one. We prefer the explicit code header, then fall back to parsing Location.
        // If neither yields a code we throw rather than silently returning empty, so callers never
        // attach empty media to an asset.
        // Marker that ends every media-file endpoint path segment:
        //   .../v1/media-files/<code>, .../v1/asset-media-files/<code>,
        //   .../v1/category-media-files/<code>, .../v1/reference-entities-media-files/<code>.
        // The created code is everything after this marker.
        private const string MediaFilesMarker = "media-files/";

        private string ExtractMediaFileCode(HttpResponseMessage response, string requestUrl)
        {
            // 1) Preferred: the dedicated code header (bare code, no URI prefix).
            if (response.Headers.TryGetValues("asset-media-file-code", out var codeValues))
            {
                var code = codeValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(code))
                    return code;
            }

            // 2) Fall back to the Location header, handling both absolute and relative URIs and any of
            //    the media-file endpoints. The code is the path tail after the ".../<x>media-files/" marker.
            var location = response.Headers.Location?.ToString();
            if (!string.IsNullOrEmpty(location))
            {
                // Reduce an absolute URI to its path so matching works regardless of host.
                var path = Uri.TryCreate(location, UriKind.Absolute, out var abs) ? abs.AbsolutePath : location;

                var idx = path.IndexOf(MediaFilesMarker, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var extracted = path[(idx + MediaFilesMarker.Length)..];
                    if (!string.IsNullOrEmpty(extracted))
                        return extracted;
                }
            }

            // 3) 2xx but no resolvable code — fail loudly so callers don't proceed with empty media.
            var headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
            _logger?.LogError(
                "POST multipart {Url} succeeded ({StatusCode}) but no media-file code could be resolved "
                + "from the 'asset-media-file-code' or 'Location' headers.", requestUrl, (int)response.StatusCode);
            throw new AkeneoApiException(
                requestUrl, "POST", response.StatusCode,
                "Media file upload succeeded but the response carried no resolvable media-file code "
                + "(neither 'asset-media-file-code' nor a parseable 'Location' header was present).",
                responseHeaders: headers);
        }

        private async Task<HttpResponseMessage> ExecuteMultipartWithPipelineAsync(
            string requestUrl,
            string fieldName,
            byte[] fileBytes,
            string fileName,
            string contentType,
            IReadOnlyDictionary<string, string>? extraParts,
            string token,
            CancellationToken ct)
        {
            return await _nonIdempotentPipeline.ExecuteAsync(async ct2 =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var multipart = new MultipartFormDataContent();
                if (extraParts != null)
                {
                    foreach (var part in extraParts)
                        multipart.Add(new StringContent(part.Value), part.Key);
                }
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                multipart.Add(fileContent, fieldName, fileName);
                request.Content = multipart;

                return await _httpClient.SendAsync(request, ct2).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
        }

        #endregion


    }
}

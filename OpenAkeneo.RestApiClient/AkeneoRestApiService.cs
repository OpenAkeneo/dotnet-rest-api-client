using OpenAkeneo.RestApiClient.Models;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient
{
    /// <summary>
    /// Low-level HTTP client for the Akeneo REST API. Handles OAuth2 token acquisition,
    /// caching, and transparent refresh on 401 responses. Automatically retries transient
    /// failures and 429 Too Many Requests responses with exponential back-off and jitter
    /// via a Polly resilience pipeline. Inject an optional
    /// <see cref="ILogger{AkeneoRestApiService}"/> to trace all HTTP activity and token
    /// lifecycle events.
    /// </summary>
    public class AkeneoRestApiService : IAkeneoRestApiService, IDisposable
    {

        #region Classes, variables and constructors

        private class TokenData
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("expires_at")]
            public DateTimeOffset ExpiresAt { get; set; }

            [JsonPropertyName("error_details")]
            public string? ErrorDetails { get; set; }
        }


        private const string _tokenUrl = "/api/oauth/v1/token";
        private const double _tokenEarlyExpirationFraction = 0.75;
        private TokenData? _tokenData;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);


        private readonly HttpClient _httpClient;
        private readonly AkeneoRestApiSettings _settings;
        private readonly ILogger<AkeneoRestApiService>? _logger;
        private readonly ResiliencePipeline<HttpResponseMessage> _resiliencePipeline;


        /// <summary>
        /// Initialises the service with a pre-configured <see cref="HttpClient"/> and Akeneo credentials.
        /// </summary>
        /// <param name="httpClient">An <see cref="HttpClient"/> whose <c>BaseAddress</c> may be left unset;
        /// the service prepends <see cref="AkeneoRestApiSettings.RestApiUrl"/> when needed.</param>
        /// <param name="settings">Connection settings including credentials and API base URL.</param>
        /// <param name="logger">Optional logger for HTTP request/response and token lifecycle tracing.</param>
        public AkeneoRestApiService(HttpClient httpClient, AkeneoRestApiSettings settings, ILogger<AkeneoRestApiService>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(settings);
            settings.Validate();

            _httpClient = httpClient;
            _settings = settings;
            _logger = logger;

            // Retry strategy aligned with Akeneo good-practices:
            // https://api.akeneo.com/documentation/good-practices.html#retry-strategy
            // - 5 retries (within the 5–8 recommended range)
            // - Exponential back-off starting at 500 ms, capped at 30 s, with jitter
            // - Covers 429 (rate-limit), 408 (request timeout), and transient 5xx / network faults
            // - Retry-After header is respected automatically by HttpRetryStrategyOptions
            var retryOptions = new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 5,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500),
                MaxDelay = TimeSpan.FromSeconds(30),
                ShouldHandle = args =>
                {
                    // Include Akeneo-specified retryable codes on top of the defaults (429 + 5xx)
                    if (args.Outcome.Result?.StatusCode == HttpStatusCode.RequestTimeout) // 408
                        return ValueTask.FromResult(true);
                    bool isTransient = HttpClientResiliencePredicates.IsTransient(args.Outcome);
                    return ValueTask.FromResult(isTransient);
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

            _resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(retryOptions)
                .Build();
        }

        /// <inheritdoc/>
        public void Dispose() => _tokenLock.Dispose();

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

        private async Task<HttpResponseMessage> ExecuteWithPipelineAsync(
            HttpMethod method,
            string requestUrl,
            Dictionary<string, string>? headers,
            string? jsonString,
            string token,
            CancellationToken ct)
        {
            return await _resiliencePipeline.ExecuteAsync(async ct2 =>
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
                    request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                return await _httpClient.SendAsync(request, ct2).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
        }

        private async Task<string> PerformRequestString(HttpMethod method, string url, Dictionary<string, string>? headers, string? jsonString, CancellationToken ct = default)
        {
            var (_, body) = await PerformRequestStringWithStatus(method, url, headers, jsonString, ct).ConfigureAwait(false);
            return body;
        }

        private async Task<(HttpStatusCode StatusCode, string Body)> PerformRequestStringWithStatus(HttpMethod method, string url, Dictionary<string, string>? headers, string? jsonString, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(url);

            var requestUrl = url.StartsWith("http") ? url : _settings.RestApiUrl + url;

            var token = await GetTokenAsync(ct: ct).ConfigureAwait(false);
            using var response = await ExecuteWithPipelineAsync(method, requestUrl, headers, jsonString, token, ct).ConfigureAwait(false);
            _logger?.LogDebug("HTTP {Method} {Url} → {StatusCode}", method.Method, requestUrl, (int)response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger?.LogWarning("Received 401 Unauthorized, triggering token refresh for ClientId {ClientId}", _settings.ClientId);
                token = await GetTokenAsync(forceRefresh: true, ct: ct).ConfigureAwait(false);
                using var retryResponse = await ExecuteWithPipelineAsync(method, requestUrl, headers, jsonString, token, ct).ConfigureAwait(false);
                _logger?.LogDebug("HTTP {Method} {Url} → {StatusCode} (after token refresh)", method.Method, requestUrl, (int)retryResponse.StatusCode);
                return await ReadResponseAsync(retryResponse, method.Method, requestUrl, ct).ConfigureAwait(false);
            }

            return await ReadResponseAsync(response, method.Method, requestUrl, ct).ConfigureAwait(false);
        }

        private async Task<(HttpStatusCode, string)> ReadResponseAsync(HttpResponseMessage response, string method, string requestUrl, CancellationToken ct)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var apiMessage = TryParseAkeneoError(body);
                _logger?.LogError("API error {StatusCode} for {Method} {Url}: {ApiMessage}", (int)response.StatusCode, method, requestUrl, apiMessage);
                throw new AkeneoApiException(requestUrl, method, response.StatusCode, apiMessage, body);
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
                _logger?.LogWarning("Received 401 Unauthorized on binary request, triggering token refresh for ClientId {ClientId}", _settings.ClientId);
                token = await GetTokenAsync(forceRefresh: true, ct: ct).ConfigureAwait(false);
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
                var apiMessage = TryParseAkeneoError(body);
                _logger?.LogError("API error {StatusCode} for GET {Url}: {ApiMessage}", (int)response.StatusCode, requestUrl, apiMessage);
                throw new AkeneoApiException(requestUrl, "GET", response.StatusCode, apiMessage, body);
            }

            return await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to extract a human-readable error message from an Akeneo API response body.
        /// Returns the <c>"message"</c> field from a well-formed Akeneo JSON error object, or
        /// the raw body string (truncated to 500 characters) if parsing fails.
        /// </summary>
        private static string TryParseAkeneoError(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "(empty response body)";

            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body);
                if (json != null && json.TryGetValue("message", out var msg))
                    return msg.GetString() ?? body;
            }
            catch (JsonException) { }

            return body.Length > 500 ? body[..500] + "…" : body;
        }

        #endregion


        #region Token management

        /// <summary>
        /// Returns a valid Bearer access token, refreshing it transparently when it has
        /// reached 75% of its lifetime or when <paramref name="forceRefresh"/> is <c>true</c>.
        /// Thread-safe: concurrent callers block on a per-ClientId semaphore so only one
        /// refresh request is ever in-flight at a time.
        /// </summary>
        /// <param name="forceRefresh">When <c>true</c>, always fetches a new token regardless of cache state.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A valid Bearer access token string.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the token endpoint returns no access token.</exception>
        public async Task<string> GetTokenAsync(bool forceRefresh = false, CancellationToken ct = default)
        {
            var tokenFilePath = _settings.TokenFilePath is { } tfp ? string.Format(tfp, _settings.ClientId) : null;

            if (!forceRefresh)
            {
                var cached = _tokenData;

                if (!string.IsNullOrEmpty(tokenFilePath) && cached == null && File.Exists(tokenFilePath))
                {
                    try
                    {
                        cached = JsonSerializer.Deserialize<TokenData>(await File.ReadAllTextAsync(tokenFilePath, ct).ConfigureAwait(false));

                        if (cached != null)
                        {
                            _tokenData = cached;
                            _logger?.LogDebug("Token loaded from file for ClientId {ClientId}", _settings.ClientId);
                        }
                    }
                    catch (Exception ex) when (ex is IOException or JsonException)
                    {
                        _logger?.LogWarning(ex, "Failed to load token from file {TokenFilePath} — will fetch a new token", tokenFilePath);
                    }
                }

                if (cached != null && !string.IsNullOrEmpty(cached.AccessToken) && DateTimeOffset.UtcNow < cached.ExpiresAt)
                    return cached.AccessToken!;
            }

            await _tokenLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (!forceRefresh)
                {
                    var cached = _tokenData;
                    if (cached != null && !string.IsNullOrEmpty(cached.AccessToken) && DateTimeOffset.UtcNow < cached.ExpiresAt)
                        return cached.AccessToken!;
                }

                _logger?.LogInformation("Fetching new token for ClientId {ClientId}", _settings.ClientId);
                var tokenData = await FetchNewTokenAsync(ct).ConfigureAwait(false);

                if (string.IsNullOrEmpty(tokenData.AccessToken))
                    throw new UnauthorizedAccessException("Failed to get access token: " + tokenData.ErrorDetails);

                _logger?.LogInformation("Token acquired for ClientId {ClientId}, expires at {ExpiresAt:u}", _settings.ClientId, tokenData.ExpiresAt);

                if (!string.IsNullOrEmpty(tokenFilePath))
                {
                    var json = JsonSerializer.Serialize(tokenData);
                    var tempPath = tokenFilePath + ".tmp";
                    await File.WriteAllTextAsync(tempPath, json, ct).ConfigureAwait(false);
                    File.Move(tempPath, tokenFilePath, overwrite: true);
                }

                _tokenData = tokenData;

                return tokenData.AccessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private async Task<TokenData> FetchNewTokenAsync(CancellationToken ct = default)
        {
            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            var response = await _resiliencePipeline.ExecuteAsync(async ct2 =>
            {
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", _settings.Username),
                    new KeyValuePair<string, string>("password", _settings.Password),
                });

                using var request = new HttpRequestMessage(HttpMethod.Post, _settings.RestApiUrl + _tokenUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);
                request.Content = formData;

                return await _httpClient.SendAsync(request, ct2).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);

            using (response)
            {
                var responseContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return new TokenData { ErrorDetails = responseContent };

                try
                {
                    var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);

                    var tokenData = new TokenData
                    {
                        AccessToken = tokenResponse!["access_token"].GetString(),
                        ExpiresIn = tokenResponse["expires_in"].GetInt32()
                    };

                    tokenData.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenData.ExpiresIn * _tokenEarlyExpirationFraction);

                    return tokenData;
                }
                catch (Exception ex)
                {
                    return new TokenData { ErrorDetails = ex.Message };
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

        /// <summary>Performs an authenticated HTTP PATCH and returns both the HTTP status code and response body.</summary>
        /// <param name="url">Relative or absolute URL.</param>
        /// <param name="content">JSON request body.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The HTTP status code and response body string.</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<(HttpStatusCode StatusCode, string Body)> HttpPatchWithStatusAsync(string url, string content, CancellationToken ct = default)
        {
            return await PerformRequestStringWithStatus(HttpMethod.Patch, url, null, content, ct).ConfigureAwait(false);
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
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string (typically contains the created resource location or code).</returns>
        /// <exception cref="AkeneoApiException">Thrown on non-2xx status codes.</exception>
        public async Task<string> HttpPostMultipartAsync(string url, string fieldName, byte[] fileBytes, string fileName, string contentType, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(url);

            var requestUrl = url.StartsWith("http") ? url : _settings.RestApiUrl + url;

            var token = await GetTokenAsync(ct: ct).ConfigureAwait(false);
            using var response = await ExecuteMultipartWithPipelineAsync(requestUrl, fieldName, fileBytes, fileName, contentType, token, ct).ConfigureAwait(false);
            _logger?.LogDebug("HTTP POST (multipart) {Url} → {StatusCode}", requestUrl, (int)response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger?.LogWarning("Received 401 Unauthorized on multipart request, triggering token refresh for ClientId {ClientId}", _settings.ClientId);
                token = await GetTokenAsync(forceRefresh: true, ct: ct).ConfigureAwait(false);
                using var retryResponse = await ExecuteMultipartWithPipelineAsync(requestUrl, fieldName, fileBytes, fileName, contentType, token, ct).ConfigureAwait(false);
                _logger?.LogDebug("HTTP POST (multipart) {Url} → {StatusCode} (after token refresh)", requestUrl, (int)retryResponse.StatusCode);
                var (_, retryBody) = await ReadResponseAsync(retryResponse, "POST", requestUrl, ct).ConfigureAwait(false);
                return retryBody;
            }

            var (__, body) = await ReadResponseAsync(response, "POST", requestUrl, ct).ConfigureAwait(false);
            return body;
        }

        private async Task<HttpResponseMessage> ExecuteMultipartWithPipelineAsync(
            string requestUrl,
            string fieldName,
            byte[] fileBytes,
            string fileName,
            string contentType,
            string token,
            CancellationToken ct)
        {
            return await _resiliencePipeline.ExecuteAsync(async ct2 =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var multipart = new MultipartFormDataContent();
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

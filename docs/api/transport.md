# Transport (IAkeneoRestApiService) — OpenAkeneo.RestApiClient

The low-level HTTP service underneath `AkeneoContext` (reachable via `context.Service`).
Use it as an escape hatch for raw calls; token management, retries and 401 refresh apply
automatically. Generated from the compiled v0.9.2 surface — do not edit by hand.

**Domain notes:**
- URLs may be relative (`/api/rest/v1/...`, prefixed with `RestApiUrl`) or absolute.
- Retry policy: GET/PUT/DELETE/PATCH retry transient failures (5xx/429/408/network) with
  exponential back-off; **POST retries only 429/408** (responses proving the server did not
  process the request) so non-idempotent calls are never replayed.
- 401 triggers one automatic token refresh + retry. Token caching is shared across instances
  when constructed via `AddAkeneoClient` (singleton `AkeneoTokenCache`).
- `HttpPostMultipartAsync` returns the created **media-file code** (from response headers) and
  throws if none is resolvable; `HttpPostMultipartForBodyAsync` returns the raw body instead.

## `GetTokenAsync`

```csharp
Task<string> GetTokenAsync(bool forceRefresh = false, CancellationToken ct = default)
```

Returns a valid OAuth access token, fetching or refreshing as needed.

## `HttpDeleteAsync`

```csharp
Task<string> HttpDeleteAsync(string url, CancellationToken ct = default)
```

Performs a DELETE request and returns the response body as a string.

## `HttpDeleteAsync`

```csharp
Task<string> HttpDeleteAsync(string url, Dictionary<string, string>? headers, CancellationToken ct = default)
```

Performs a DELETE request with additional headers and returns the response body as a string.

## `HttpGetAsync`

```csharp
Task<string> HttpGetAsync(string url, CancellationToken ct = default)
```

Performs a GET request and returns the response body as a string.

## `HttpGetAsync`

```csharp
Task<string> HttpGetAsync(string url, Dictionary<string, string> headers, CancellationToken ct = default)
```

Performs a GET request with additional headers and returns the response body as a string.

## `HttpGetBytesAsync`

```csharp
Task<byte[]> HttpGetBytesAsync(string url, CancellationToken ct = default)
```

Performs a GET request and returns the response body as raw bytes.

## `HttpGetStreamAsync`

```csharp
Task<Stream> HttpGetStreamAsync(string url, CancellationToken ct = default)
```

Performs a GET request and returns the response body as an unbuffered stream (dispose it to release the HTTP response).

## `HttpPatchAsync`

```csharp
Task<string> HttpPatchAsync(string url, string content, CancellationToken ct = default)
```

Performs a PATCH request and returns the response body as a string.

## `HttpPatchAsync`

```csharp
Task<string> HttpPatchAsync(string url, string? content, Dictionary<string, string>? headers, CancellationToken ct = default)
```

Performs a PATCH request with additional headers and returns the response body as a string.

## `HttpPatchAsync`

```csharp
Task<string> HttpPatchAsync(string url, string content, string contentType, CancellationToken ct = default)
```

Performs a PATCH request with an explicit content type (e.g. Akeneo's `application/vnd.akeneo.collection+json` batch format).

## `HttpPatchWithStatusAsync`

```csharp
Task<ValueTuple<HttpStatusCode, string>> HttpPatchWithStatusAsync(string url, string content, CancellationToken ct = default)
```

Performs a PATCH request and returns the HTTP status code and response body.

## `HttpPostAsync`

```csharp
Task<string> HttpPostAsync(string url, string content, CancellationToken ct = default)
```

Performs a POST request and returns the response body as a string.

## `HttpPostAsync`

```csharp
Task<string> HttpPostAsync(string url, string? content, Dictionary<string, string>? headers, CancellationToken ct = default)
```

Performs a POST request with additional headers and returns the response body as a string.

## `HttpPostMultipartAsync`

```csharp
Task<string> HttpPostMultipartAsync(string url, string fieldName, byte[] fileBytes, string fileName, string contentType, IReadOnlyDictionary<string, string>? extraParts = null, CancellationToken ct = default)
```

Performs a POST request with a multipart/form-data body (file upload) and returns the created media-file code resolved from the response headers. `extraParts` carries additional string form parts (e.g. the `product` part required by `POST /media-files`).

## `HttpPostMultipartForBodyAsync`

```csharp
Task<string> HttpPostMultipartForBodyAsync(string url, string fieldName, byte[] fileBytes, string fileName, string contentType, IReadOnlyDictionary<string, string>? extraParts = null, CancellationToken ct = default)
```

Performs a multipart/form-data POST and returns the raw response body (no media-file code extraction).

## `HttpPostWithLocationAsync`

```csharp
Task<ValueTuple<HttpStatusCode, string, string>> HttpPostWithLocationAsync(string url, string content, CancellationToken ct = default)
```

Performs a POST request and returns the HTTP status code, response body, and Location header.

## `HttpPutAsync`

```csharp
Task<string> HttpPutAsync(string url, string content, CancellationToken ct = default)
```

Performs a PUT request with a JSON body and returns the response body as a string.


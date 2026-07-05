**Domain notes:**
- URLs may be relative (`/api/rest/v1/...`, prefixed with `RestApiUrl`) or absolute.
- Retry policy: GET/PUT/DELETE/PATCH retry transient failures (5xx/429/408/network) with
  exponential back-off; **POST retries only 429/408** (responses proving the server did not
  process the request) so non-idempotent calls are never replayed.
- 401 triggers one automatic token refresh + retry. Token caching is shared across instances
  when constructed via `AddAkeneoClient` (singleton `AkeneoTokenCache`).
- `HttpPostMultipartAsync` returns the created **media-file code** (from response headers) and
  throws if none is resolvable; `HttpPostMultipartForBodyAsync` returns the raw body instead.
# Token Management

Tokens are acquired and refreshed automatically — no manual handling is needed for normal use. The client fetches a token on the first API call, caches it in memory, and proactively refreshes it at 75% of its lifetime. On a `401 Unauthorized` response it transparently fetches a new token and retries once.

These methods are available when explicit control is required.

## Access the token

```csharp
// Returns the cached token, or fetches a new one if expired
var token = await context.Service.GetTokenAsync();
Console.WriteLine($"Token: {token[..8]}…");
```

## Force a token refresh

```csharp
// Fetches a new token regardless of whether the cached one is still valid
var freshToken = await context.Service.GetTokenAsync(forceRefresh: true);
```

## Disk-based token cache

Set `TokenFilePath` in your settings to persist the token across process restarts. The `{0}` placeholder is replaced with the `ClientId`:

```json
{
  "AkeneoSettings": {
    "TokenFilePath": "akeneo_token.{0}.json"
  }
}
```

> Add `akeneo_token.*.json` to your `.gitignore` to avoid committing cached tokens. This is already included in the default `.gitignore` for this project.

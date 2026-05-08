# Error Handling

All API errors are thrown as `AkeneoApiException`. Use C# exception filters to handle specific HTTP status codes cleanly.

```csharp
using System.Net;

try
{
    var product = await context.GetProductUuidAsync("00000000-0000-0000-0000-000000000000");
    Console.WriteLine($"Found: {product.Uuid}");
}
catch (AkeneoApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine($"Product not found: {ex.ApiMessage}");
}
catch (AkeneoApiException ex)
{
    Console.WriteLine($"API error {(int)ex.StatusCode} at {ex.RequestUrl}: {ex.ApiMessage}");
}
```

## Available properties on AkeneoApiException

| Property | Description |
|----------|-------------|
| `StatusCode` | The HTTP status code returned by Akeneo |
| `RequestUrl` | The URL of the failed request |
| `ApiMessage` | The error message from the Akeneo response body |

## Retries

Transient errors (`429`, `408`, `5xx`, network faults) are retried automatically by the built-in Polly pipeline before an exception is thrown. See [Resilience and Retries](../README.md#resilience-and-retries) for details.

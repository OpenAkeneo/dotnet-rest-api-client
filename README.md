# OpenAkeneo.RestApiClient

[![NuGet](https://img.shields.io/nuget/v/OpenAkeneo.RestApiClient)](https://www.nuget.org/packages/OpenAkeneo.RestApiClient)
[![NuGet Downloads](https://img.shields.io/nuget/dt/OpenAkeneo.RestApiClient)](https://www.nuget.org/packages/OpenAkeneo.RestApiClient)
[![Build](https://github.com/OpenAkeneo/dotnet-rest-api-client/actions/workflows/ci.yml/badge.svg)](https://github.com/OpenAkeneo/dotnet-rest-api-client/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/OpenAkeneo/dotnet-rest-api-client/blob/main/LICENSE)

Unofficial .NET client library for the [Akeneo PIM](https://www.akeneo.com/) REST API — typed models, automatic OAuth token management, streaming pagination, and Polly-based retry handling.

- **Note:** This project is currently in development, APIs may change between minor versions until 1.0. Use at your own risk!
- **Note:** Akeneo is a registered trademark of Akeneo SA. This project is not affiliated with Akeneo SA. 
- **Note:** This project was developed with assistance from AI tools. Please consider this fact against your AI governance policy before using this library.

---

## Why this exists

Akeneo does not publish an official .NET SDK. The options available are either outdated, incomplete, or tightly coupled to specific project structures. This library was built to fill that gap — a clean, modern .NET client that handles the OAuth lifecycle, retries, and HAL pagination so you can focus on working with your PIM data rather than the HTTP layer.

---

## Features

- Two-layer architecture: low-level HTTP service + high-level typed context
- Automatic OAuth token acquisition, in-memory caching, and proactive refresh at 75% of token lifetime
- Transparent 401 retry — fetches a new token and retries once without any extra code
- `IAsyncEnumerable<T>` streaming for all list resources — handles pagination automatically
- Polly resilience pipeline: 5 retries with exponential back-off, jitter, and `Retry-After` support
- `CancellationToken` support on every method
- Optional disk-based token cache for persistence across process restarts

---

## Installation

```bash
dotnet add package OpenAkeneo.RestApiClient
```

Or search for `OpenAkeneo.RestApiClient` in the NuGet Package Manager.

---

## Quickstart

```csharp
// 1. Load settings
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = configuration.GetSection("AkeneoSettings").Get<AkeneoRestApiSettings>();

// 2. Create the client
var context = new AkeneoContext(settings);

// 3. Use it
await foreach (var product in context.StreamProductUuidsAsync())
{
    Console.WriteLine($"{product.Uuid} — enabled: {product.Enabled}");
}
```

---

## Configuration

Copy `appsettings.example.json` to `appsettings.json` and fill in your credentials:

```json
{
  "AkeneoSettings": {
    "Id": "MyConnection",
    "Name": "My Akeneo Connection",
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret",
    "Username": "your_api_username",
    "Password": "your_api_password",
    "RestApiUrl": "https://your-instance.cloud.akeneo.com",
    "TokenFilePath": ""
  }
}
```

`TokenFilePath` is optional. When set (e.g. `"akeneo_token.{0}.json"`), the OAuth token is cached to disk and reused across restarts. Leave empty to cache in memory only.

---

## Architecture

| Layer | Class | Responsibility |
|-------|-------|----------------|
| Low-level | `AkeneoRestApiService` | OAuth, HTTP, retries, token cache |
| High-level | `AkeneoContext` | Typed methods for all Akeneo resources |

For most use cases, only `AkeneoContext` is needed.

---

## Usage

### Token Management

Tokens are acquired automatically on the first call. Explicit control is also available:

```csharp
var token = await apiService.GetTokenAsync();
var freshToken = await apiService.GetTokenAsync(forceRefresh: true);
```

Tokens are refreshed automatically at 75% of their lifetime. On a `401 Unauthorized` response the service transparently fetches a new token and retries once.

---

### Error Handling

All API errors throw `AkeneoApiException`:

```csharp
try
{
    var product = await context.GetProductUuidAsync("non-existent-uuid");
}
catch (AkeneoApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine($"Product not found: {ex.ApiMessage}");
}
catch (AkeneoApiException ex)
{
    Console.WriteLine($"API error {ex.StatusCode} at {ex.RequestUrl}: {ex.ApiMessage}");
}
```

---

### Pagination Patterns

Every list resource exposes three access patterns:

| Method | Description |
|--------|-------------|
| `StreamXxxAsync()` | `IAsyncEnumerable<T>` — yields items one-by-one, fetches pages on demand. Best for large catalogs. |
| `GetXxxListAsync(page, limit)` | Returns a single page with HAL navigation links. |
| `GetXxxListFullAsync()` | Buffers all pages into a `List<T>`. Convenient but may use significant memory on large catalogs. |

---

### System Information

```csharp
var info = await context.GetSystemInformationAsync();
Console.WriteLine($"Akeneo version: {info.Version}");
```

---

### Products (UUID-based)

The preferred modern API uses product UUIDs.

```csharp
// Stream all products
await foreach (var product in context.StreamProductUuidsAsync())
    Console.WriteLine($"{product.Uuid} — enabled: {product.Enabled}");

// Stream with a search filter (JSON-encoded Akeneo search syntax)
var search = """{"enabled":[{"operator":"=","value":true}]}""";
await foreach (var product in context.StreamProductUuidsAsync(search: search))
    Console.WriteLine(product.Uuid);

// Single product by UUID
var product = await context.GetProductUuidAsync("a4f47e32-b29c-4f3d-a0b2-123456789abc");

// Create or update
await context.CreateOrUpdateProductUuidAsync(new ProductUuid
{
    Uuid = "a4f47e32-b29c-4f3d-a0b2-123456789abc",
    Enabled = true,
    Family = "clothing"
});

// Draft (requires Workflow feature)
var draft = await context.GetProductUuidDraftAsync("a4f47e32-b29c-4f3d-a0b2-123456789abc");
```

---

### Products (Identifier-based)

The legacy identifier (SKU) API is also fully supported.

```csharp
await foreach (var product in context.StreamProductIdentifiersAsync())
    Console.WriteLine(product.Identifier);

var product = await context.GetProductIdentifierAsync("my-sku-001");

await context.CreateOrUpdateProductIdentifierAsync(new ProductIdentifier
{
    Identifier = "my-sku-001",
    Family = "clothing",
    Enabled = true
});
```

---

### Product Models

```csharp
await foreach (var model in context.StreamProductModelsAsync())
    Console.WriteLine($"{model.Code} — family: {model.Family}");

var model = await context.GetProductModelAsync("summer_collection_2024");

await context.CreateOrUpdateProductModelAsync(new ProductModel
{
    Code = "summer_collection_2024",
    Family = "clothing",
    FamilyVariant = "clothing_color_size"
});
```

---

### Product Media Files

```csharp
await foreach (var file in context.StreamProductMediaFilesAsync())
    Console.WriteLine($"{file.Code} — {file.MimeType}");

byte[] bytes = await context.DownloadProductMediaFileAsync("f/f/c/f/ffcf299bae0e4aeb0b85ea232722cf2a5efea125_image.jpg");
File.WriteAllBytes("output.jpg", bytes);
```

---

### Families & Variants

```csharp
await foreach (var family in context.StreamFamiliesAsync())
    Console.WriteLine(family.Code);

var family = await context.GetFamilyAsync("digital_cameras");

await context.CreateOrUpdateFamilyAsync(new Family
{
    Code = "my_family",
    AttributeAsLabel = "name",
    Labels = new() { ["en_US"] = "My Family" }
});

await foreach (var variant in context.StreamFamilyVariantsAsync("clothing"))
    Console.WriteLine(variant.Code);

var variant = await context.GetFamilyVariantAsync("clothing", "clothing_color_size");
```

---

### Attributes

```csharp
await foreach (var attr in context.StreamAttributesAsync())
    Console.WriteLine($"{attr.Code} — {attr.Type}");

// With a type filter
var search = """{"type":[{"operator":"IN","value":["pim_catalog_simpleselect"]}]}""";
var selectAttrs = await context.GetAttributeListFullAsync(search: search);

var attr = await context.GetAttributeAsync("accessories_care_instructions");
```

---

### Attribute Options & Groups

```csharp
var options = await context.GetAttributeOptionListFullAsync("color");
var option  = await context.GetAttributeOptionAsync("color", "red");

await foreach (var group in context.StreamAttributeGroupsAsync())
    Console.WriteLine(group.Code);
```

---

### Categories

```csharp
await foreach (var cat in context.StreamCategoriesAsync())
    Console.WriteLine($"{cat.Code} — parent: {cat.Parent}");

var cat = await context.GetCategoryAsync("master");

byte[] bytes = await context.DownloadCategoryMediaFileAsync("c/7/3/c/c73cc4c27c46f22447bcda64db3345269e29ecf4_banner.png");
```

---

### Channels, Locales, Currencies, Measurement Families

```csharp
var channels            = await context.GetChannelListFullAsync();
var locales             = await context.GetLocaleListFullAsync();
var currencies          = await context.GetCurrencyListFullAsync();
var measurementFamilies = await context.GetMeasurementFamilyListAsync();
```

---

### Association Types

```csharp
var all   = await context.GetAssociationTypeListFullAsync();
var assoc = await context.GetAssociationTypeAsync("x_sell");
```

---

### Reference Entities

```csharp
var entities   = await context.GetReferenceEntityListAsync();
var entity     = await context.GetReferenceEntityAsync("brand");
var attributes = await context.GetReferenceEntityAttributeListAsync("brand");
var records    = await context.GetReferenceEntityRecordListAsync("brand");
var record     = await context.GetReferenceEntityRecordAsync("brand", "nike");

byte[] bytes = await context.DownloadReferenceEntityMediaFileAsync("f/f/c/f/ffcf299bae0e4aeb0b85ea232722cf2a5efea125_logo.png");
```

---

### Assets

```csharp
var assetFamilies = await context.GetAssetFamilyListAsync();
var assetFamily   = await context.GetAssetFamilyAsync("packshots");
var attrs         = await context.GetAssetAttributeListAsync("packshots");

byte[] bytes = await context.DownloadAssetMediaFileAsync("path/to/asset_file.jpg");
```

---

### Jobs

```csharp
var result    = await context.LaunchExportJobAsync("csv_product_export");
var execution = await context.GetJobExecutionAsync(result.JobExecutionId);
Console.WriteLine($"Status: {execution.Status}");

var importResult = await context.LaunchImportJobAsync("csv_product_import");
```

---

### Catalogs

```csharp
var catalogs    = await context.GetCatalogListAsync();
var catalog     = await context.GetCatalogAsync("my-catalog-id");
var productPage = await context.GetCatalogProductUuidListAsync("my-catalog-id", page: 1, limit: 100);
```

---

### Cancellation Token Support

Every method accepts an optional `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

await foreach (var product in context.StreamProductUuidsAsync(ct: cts.Token))
    Console.WriteLine(product.Uuid);
```

---

### Resilience and Retries

`AkeneoRestApiService` uses a Polly resilience pipeline aligned with [Akeneo's recommended retry strategy](https://api.akeneo.com/documentation/good-practices.html#retry-strategy):

- 5 retries with exponential back-off starting at 500 ms, capped at 30 s, with jitter
- Handles `429 Too Many Requests`, `408 Request Timeout`, transient `5xx` errors, and network faults
- Respects the `Retry-After` response header automatically
- Logs retries at `Warning` level when a logger is provided

No additional configuration is required.

---

## API Reference

Full Akeneo REST API documentation: [api.akeneo.com](https://api.akeneo.com/api-reference.html)

---

## License

[MIT](LICENSE)

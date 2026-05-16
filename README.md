# OpenAkeneo.RestApiClient

[![NuGet](https://img.shields.io/nuget/v/OpenAkeneo.RestApiClient)](https://www.nuget.org/packages/OpenAkeneo.RestApiClient)
[![NuGet Downloads](https://img.shields.io/nuget/dt/OpenAkeneo.RestApiClient)](https://www.nuget.org/packages/OpenAkeneo.RestApiClient)
[![Build](https://github.com/OpenAkeneo/dotnet-rest-api-client/actions/workflows/ci.yml/badge.svg)](https://github.com/OpenAkeneo/dotnet-rest-api-client/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/OpenAkeneo/dotnet-rest-api-client/blob/main/LICENSE)

Unofficial .NET client library for the [Akeneo PIM](https://www.akeneo.com/) REST API — typed models, automatic OAuth token management, streaming pagination, and Polly-based retry handling.

> Note: This project is currently in development, APIs may change between minor versions until 1.0. Use at your own risk!

> Note: Akeneo is a registered trademark of Akeneo SA. This project is not affiliated with Akeneo SA.

> Note: This project was developed with assistance from AI tools. Please consider this fact against your AI governance policy before using this library.

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

> **Security note:** `TokenFilePath` should point to a directory with restrictive ACLs. Never place it on a shared filesystem or in a web-accessible path.

---

## Architecture

| Layer | Class | Responsibility |
|-------|-------|----------------|
| Low-level | `AkeneoRestApiService` | OAuth, HTTP, retries, token cache |
| High-level | `AkeneoContext` | Typed methods for all Akeneo resources |

For most use cases, only `AkeneoContext` is needed.

---

## Dependency Injection (ASP.NET Core / hosted services)

For long-running applications, use `AddAkeneoClient` so `IHttpClientFactory` manages handler rotation, preventing stale-DNS and socket exhaustion:

```csharp
// Program.cs
var settings = builder.Configuration.GetSection("AkeneoSettings").Get<AkeneoRestApiSettings>();

builder.Services.AddAkeneoClient(settings);
```

Then inject `AkeneoContext` into your services:

```csharp
public class ProductSyncService
{
    private readonly AkeneoContext _akeneo;

    public ProductSyncService(AkeneoContext akeneo)
    {
        _akeneo = akeneo;
    }

    public async Task SyncAsync(CancellationToken ct)
    {
        await foreach (var product in _akeneo.StreamProductUuidsAsync(ct: ct))
        {
            // process product
        }
    }
}
```

For scripts and console apps where DI is not used, `new AkeneoContext(settings)` is the simpler option.

---

## Usage

### Token Management

Tokens are acquired automatically on the first call. Explicit control is also available:

```csharp
var token = await context.Service.GetTokenAsync();
var freshToken = await context.Service.GetTokenAsync(forceRefresh: true);
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

### Search Filter Syntax

Many list methods accept a `search` parameter. The value is a JSON-encoded Akeneo search filter following the [Akeneo filter syntax](https://api.akeneo.com/documentation/filter.html):

```csharp
// Products enabled in the ecommerce channel
var search = """{"enabled":[{"operator":"=","value":true}]}""";

// Products updated since a date
var search = """{"updated":[{"operator":">","value":"2024-01-01 00:00:00"}]}""";

// Products in a specific category (including children)
var search = """{"categories":[{"operator":"IN CHILDREN","value":["master"]}]}""";

// Attributes of a specific type
var search = """{"type":[{"operator":"IN","value":["pim_catalog_simpleselect","pim_catalog_multiselect"]}]}""";
```

The `search` value must be URL-safe JSON — the library handles encoding automatically.

---

### System Information

```csharp
var info = await context.GetSystemInformationAsync();
Console.WriteLine($"Akeneo version: {info.Version}, edition: {info.Edition}");
```

---

### Products (UUID-based)

The preferred modern API uses product UUIDs.

```csharp
// Stream all products
await foreach (var product in context.StreamProductUuidsAsync())
    Console.WriteLine($"{product.Uuid} — enabled: {product.Enabled}");

// Stream with search filter, scoped to a channel with specific locales
var search = """{"enabled":[{"operator":"=","value":true}]}""";
await foreach (var product in context.StreamProductUuidsAsync(
    search: search,
    scope: "ecommerce",
    locales: "en_US,fr_FR"))
{
    Console.WriteLine(product.Uuid);
}

// Include CDN share links for asset_collection attribute values
await foreach (var product in context.StreamProductUuidsAsync(withAssetShareLinks: true))
{
    // product.Values["my_asset_collection"][0].GetLinkedData<Dictionary<string, AssetCollectionLinkedDataEntry>>()
}

// Single product by UUID
var product = await context.GetProductUuidAsync("a4f47e32-b29c-4f3d-a0b2-123456789abc");

// Create or update (PATCH)
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

var draft = await context.GetProductIdentifierDraftAsync("my-sku-001");
```

---

### Reading Product Attribute Values

Product attribute values live in `product.Values`, keyed by attribute code. Each entry is a list of `ProductValue` objects (one per locale/scope combination).

```csharp
var product = await context.GetProductUuidAsync("a4f47e32-b29c-4f3d-a0b2-123456789abc");

if (product.Values != null && product.Values.TryGetValue("name", out var nameValues))
{
    // Get the en_US value (non-scopable attribute)
    var enValue = nameValues.FirstOrDefault(v => v.Locale == "en_US");
    Console.WriteLine(enValue?.GetStringData()); // returns string directly
}

// Numeric attribute
if (product.Values.TryGetValue("weight", out var weightValues))
{
    var metric = weightValues.FirstOrDefault(v => v.Locale == null && v.Scope == null)
                             ?.GetData<MetricValue>();
    Console.WriteLine($"{metric?.Amount} {metric?.Unit}"); // e.g. "1.5 KILOGRAM"
}

// Boolean attribute
if (product.Values.TryGetValue("is_new", out var isNewValues))
{
    bool? isNew = isNewValues.FirstOrDefault()?.GetData<bool>();
}

// Multiselect attribute (list of option codes)
if (product.Values.TryGetValue("color", out var colorValues))
{
    var codes = colorValues.FirstOrDefault()?.GetData<List<string>>();
}

// Price collection
if (product.Values.TryGetValue("price", out var priceValues))
{
    var prices = priceValues.FirstOrDefault()
                            ?.GetData<List<Dictionary<string, object?>>>();
    // Each dict has "amount" and "currency" keys
}

// Asset collection — get CDN share links (requires withAssetShareLinks: true)
if (product.Values.TryGetValue("packshots", out var assetValues))
{
    var links = assetValues.FirstOrDefault()
        ?.GetLinkedData<Dictionary<string, AssetCollectionLinkedDataEntry>>();

    foreach (var (assetCode, entry) in links ?? [])
    foreach (var link in entry.ShareLinks ?? [])
        Console.WriteLine($"{assetCode}: {link.Links?.Self?.Href}");
}
```

`GetStringData()` covers text, textarea, identifier, date, file, image, and simpleselect attributes. For all other types use `GetData<T>()` with the appropriate type.

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

var draft = await context.GetProductModelDraftAsync("summer_collection_2024");
```

---

### Product Media Files

```csharp
await foreach (var file in context.StreamProductMediaFilesAsync())
    Console.WriteLine($"{file.Code} — {file.MimeType}");

var fileMeta = await context.GetProductMediaFileAsync("f/f/c/f/ffcf2...125_image.jpg");

byte[] bytes = await context.DownloadProductMediaFileAsync("f/f/c/f/ffcf2...125_image.jpg");
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

await context.CreateOrUpdateFamilyVariantAsync("clothing", new FamilyVariant
{
    Code = "clothing_color_size",
    Labels = new() { ["en_US"] = "Color and size" }
});
```

---

### Attributes

```csharp
await foreach (var attr in context.StreamAttributesAsync())
    Console.WriteLine($"{attr.Code} — {attr.Type}");

// With a type filter
var search = """{"type":[{"operator":"IN","value":["pim_catalog_simpleselect"]}]}""";
var selectAttrs = await context.GetAttributeListFullAsync(search: search);

// Include table select options (for pim_catalog_table attributes)
var tableAttrs = await context.GetAttributeListFullAsync(withTableSelectOptions: true);

var attr = await context.GetAttributeAsync("accessories_care_instructions");

await context.CreateOrUpdateAttributeAsync(new AkeneoAttribute
{
    Code = "my_text_attr",
    Type = "pim_catalog_text",
    Group = "general"
});
```

---

### Attribute Options & Groups

```csharp
// Options
await foreach (var option in context.StreamAttributeOptionsAsync("color"))
    Console.WriteLine($"{option.Code}");

var options = await context.GetAttributeOptionListFullAsync("color");
var option  = await context.GetAttributeOptionAsync("color", "red");

await context.CreateOrUpdateAttributeOptionAsync("color", new AttributeOption
{
    Code = "navy",
    Labels = new() { ["en_US"] = "Navy Blue" }
});

// Groups
await foreach (var group in context.StreamAttributeGroupsAsync())
    Console.WriteLine(group.Code);

await context.CreateOrUpdateAttributeGroupAsync(new AttributeGroup
{
    Code = "marketing",
    Labels = new() { ["en_US"] = "Marketing" }
});
```

---

### Categories

```csharp
await foreach (var cat in context.StreamCategoriesAsync())
    Console.WriteLine($"{cat.Code} — parent: {cat.Parent}");

// Include enriched attributes (category media, labels)
await foreach (var cat in context.StreamCategoriesAsync(withEnrichedAttributes: true))
    Console.WriteLine(cat.Code);

var cat = await context.GetCategoryAsync("master");

await context.CreateOrUpdateCategoryAsync(new Category
{
    Code = "sale",
    Parent = "master",
    Labels = new() { ["en_US"] = "Sale" }
});

byte[] bytes = await context.DownloadCategoryMediaFileAsync("c/7/3/c/c73cc4...ecf4_banner.png");
```

---

### Channels, Locales, Currencies, Measurement Families

```csharp
// Channels
var channels = await context.GetChannelListFullAsync();
var channel  = await context.GetChannelAsync("ecommerce");
await context.CreateOrUpdateChannelAsync(new Channel { Code = "b2b", /* ... */ });

// Locales
var locales = await context.GetLocaleListFullAsync();
var locale  = await context.GetLocaleAsync("en_US");

// Currencies
var currencies = await context.GetCurrencyListFullAsync();
var currency   = await context.GetCurrencyAsync("USD");

// Measurement families (no paging — returns all at once)
var measurementFamilies = await context.GetMeasurementFamilyListAsync();
```

---

### Association Types

```csharp
var all   = await context.GetAssociationTypeListFullAsync();
var assoc = await context.GetAssociationTypeAsync("x_sell");

await context.CreateOrUpdateAssociationTypeAsync(new AssociationType
{
    Code = "bundle",
    Labels = new() { ["en_US"] = "Bundle" }
});
```

---

### Reference Entities

```csharp
// Entity definitions
var entities = await context.GetReferenceEntityListFullAsync();
var entity   = await context.GetReferenceEntityAsync("brand");
await context.CreateOrUpdateReferenceEntityAsync(new ReferenceEntity { Code = "brand" });

// Entity attributes
var attributes = await context.GetReferenceEntityAttributeListAsync("brand");
var attribute  = await context.GetReferenceEntityAttributeAsync("brand", "description");
await context.CreateOrUpdateReferenceEntityAttributeAsync("brand", new ReferenceEntityAttribute
{
    Code = "description",
    Type = "text"
});

// Attribute options
var options = await context.GetReferenceEntityAttributeOptionListAsync("brand", "country");
await context.CreateOrUpdateReferenceEntityAttributeOptionAsync("brand", "country",
    new ReferenceEntityAttributeOption { Code = "se", Labels = new() { ["en_US"] = "Sweden" } });

// Records
await foreach (var record in context.StreamReferenceEntityRecordsAsync("brand"))
    Console.WriteLine(record.Code);

var records = await context.GetReferenceEntityRecordListFullAsync("brand");
var record  = await context.GetReferenceEntityRecordAsync("brand", "nike");
await context.CreateOrUpdateReferenceEntityRecordAsync("brand", new ReferenceEntityRecord
{
    Code = "acme",
    Values = new() { ["name"] = new() { new ReferenceEntityRecordValue { Data = "Acme Corp", Locale = "en_US" } } }
});

// Media files
byte[] bytes = await context.DownloadReferenceEntityMediaFileAsync("f/f/c/f/...logo.png");
```

---

### Assets

```csharp
// Asset families
var families  = await context.GetAssetFamilyListFullAsync();
var family    = await context.GetAssetFamilyAsync("packshots");
await context.CreateOrUpdateAssetFamilyAsync(new AssetFamily { Code = "packshots" });

// Asset family attributes
var attrs = await context.GetAssetAttributeListAsync("packshots");
var attr  = await context.GetAssetAttributeAsync("packshots", "media_file");
await context.CreateOrUpdateAssetAttributeAsync("packshots", new AssetAttribute
{
    Code = "alt_text",
    Type = "text"
});

// Asset attribute options
var options = await context.GetAssetAttributeOptionListAsync("packshots", "orientation");
await context.CreateOrUpdateAssetAttributeOptionAsync("packshots", "orientation",
    new AssetAttributeOption { Code = "landscape" });

// Assets within a family
await foreach (var asset in context.StreamAssetsAsync("packshots"))
    Console.WriteLine(asset.Code);

var asset = await context.GetAssetAsync("packshots", "front_view");
await context.CreateOrUpdateAssetAsync("packshots", new Asset { Code = "front_view" });

// Download asset binary
byte[] bytes = await context.DownloadAssetMediaFileAsync("path/to/asset_file.jpg");
```

---

### Jobs

```csharp
// List available jobs
var jobs = await context.GetJobListFullAsync();
var job  = await context.GetJobAsync("csv_product_export");

// Launch export
var result = await context.LaunchExportJobAsync("csv_product_export");
Console.WriteLine($"Launched job execution #{result.JobExecutionId}");

// Dry run
var dryRun = await context.LaunchExportJobAsync("csv_product_export", isDryRun: true);

// Launch import
var importResult = await context.LaunchImportJobAsync("csv_product_import");

// Poll execution status
var execution = await context.GetJobExecutionAsync(result.JobExecutionId);
Console.WriteLine($"Status: {execution.Status}");

// Browse execution history
await foreach (var exec in context.StreamJobExecutionsAsync())
    Console.WriteLine($"{exec.JobLabel} — {exec.Status}");
```

---

### Workflows

```csharp
// Workflow definitions
await foreach (var workflow in context.StreamWorkflowsAsync())
    Console.WriteLine(workflow.Code);

var workflow = await context.GetWorkflowAsync("product_review");

// Workflow tasks (items awaiting action)
await foreach (var task in context.StreamWorkflowTasksAsync())
    Console.WriteLine($"{task.Uuid} — {task.Status}");

// Include attribute values on tasks
var tasks = await context.GetWorkflowTaskListFullAsync(withAttributes: true);

var task = await context.GetWorkflowTaskAsync("task-uuid-here");

// Step assignees
var assignees = await context.GetWorkflowStepAssigneeListFullAsync("step-uuid-here");
```

---

### Catalogs

```csharp
// List catalogs
await foreach (var catalog in context.StreamCatalogsAsync())
    Console.WriteLine($"{catalog.Id} — {catalog.Name}");

var catalog = await context.GetCatalogAsync("my-catalog-id");

// Stream all product UUIDs from a catalog
await foreach (var uuid in context.StreamCatalogProductUuidsAsync("my-catalog-id"))
    Console.WriteLine(uuid);

// Stream full product objects from a catalog
await foreach (var product in context.StreamCatalogProductsAsync("my-catalog-id"))
    Console.WriteLine(product.Uuid);

// Single product from catalog
var product = await context.GetCatalogProductAsync("my-catalog-id", "product-uuid");

// Mapped products (returns raw JSON string — use when catalog has a product mapping configured)
var mappedJson   = await context.GetCatalogMappedProductListAsync("my-catalog-id");
var mappedModels = await context.GetCatalogMappedModelListAsync("my-catalog-id");
var mappedVars   = await context.GetCatalogMappedVariantListAsync("my-catalog-id");

// Mapping schema
var schema = await context.GetCatalogMappingSchemaAsync("my-catalog-id");
```

---

### Utilities

```csharp
// System information
var info = await context.GetSystemInformationAsync();
Console.WriteLine($"Akeneo {info.Edition} {info.Version}");

// API overview (available endpoints)
var overview = await context.GetApiOverviewAsync();

// User channel and locale permissions
var channelPerms = await context.GetUserChannelsPermissionsAsync("user-uuid");
var localePerms  = await context.GetUserLocalesPermissionsAsync("user-uuid");

// Extensions
await foreach (var ext in context.StreamExtensionsAsync())
    Console.WriteLine(ext.Code);

// Modelization suggestions
var suggestions = await context.GetModelizationSuggestionListAsync();
var suggestion  = await context.GetModelizationSuggestionAsync("suggestion-uuid");
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

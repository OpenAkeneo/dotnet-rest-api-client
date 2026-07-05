# Catalogs — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Catalogs domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.0 surface — do not edit by hand.

**Domain notes:**
- "Catalogs for Apps" — requires an app token with the feature; classic connection tokens get
  403/404 on every endpoint here.
- Catalog product listings paginate by `search_after` only.
- `GetCatalogMapped*ListAsync` return **raw JSON strings** (the shape depends on the catalog's
  mapping schema, so no typed model exists).

## `GetCatalogListAsync`

```csharp
Task<CatalogList> GetCatalogListAsync(int page = 1, int limit = 100, CancellationToken ct = default)
```

Returns a page of catalogs (Catalog for Apps feature).

- `page` — 1-based page number.
- `limit` — Items per page (1–100).

Returns: A `CatalogList` with HAL navigation links.

## `StreamCatalogsAsync`

```csharp
IAsyncEnumerable<Catalog> StreamCatalogsAsync(CancellationToken ct = default)
```

Streams all catalogs, following HAL pagination automatically.

## `GetCatalogListFullAsync`

```csharp
Task<List<Catalog>> GetCatalogListFullAsync(CancellationToken ct = default)
```

Returns all catalogs as a materialised list.

Returns: A list of all `Catalog` objects.

## `GetCatalogAsync`

```csharp
Task<Catalog> GetCatalogAsync(string id, CancellationToken ct = default)
```

Returns a single catalog by its ID.

- `id` — The catalog UUID.

Returns: The matching `Catalog`.

## `GetCatalogProductUuidListAsync`

```csharp
Task<CatalogProductUuidList> GetCatalogProductUuidListAsync(string catalogId, int limit = 100, string? searchAfter = null, CancellationToken ct = default)
```

Returns a page of product UUIDs belonging to a catalog.

- `catalogId` — The catalog UUID.
- `limit` — Items per page (1–100).
- `searchAfter` — Cursor for keyset pagination. This endpoint paginates only via `search_after`.

Returns: A `CatalogProductUuidList` with HAL navigation links.

## `GetCatalogProductListAsync`

```csharp
Task<ProductUuidList> GetCatalogProductListAsync(string catalogId, int limit = 100, string? searchAfter = null, CancellationToken ct = default)
```

Returns a page of full product objects belonging to a catalog.

- `catalogId` — The catalog UUID.
- `limit` — Items per page (1–100).
- `searchAfter` — Cursor for keyset pagination. This endpoint paginates only via `search_after`.

Returns: A `ProductUuidList` with HAL navigation links.

## `StreamCatalogProductUuidsAsync`

```csharp
IAsyncEnumerable<string> StreamCatalogProductUuidsAsync(string catalogId, CancellationToken ct = default)
```

Streams all product UUIDs belonging to a catalog, following HAL pagination automatically.

- `catalogId` — The catalog UUID.

Returns: An async stream of product UUID strings.

## `GetCatalogProductUuidListFullAsync`

```csharp
Task<List<string>> GetCatalogProductUuidListFullAsync(string catalogId, CancellationToken ct = default)
```

Returns all product UUIDs belonging to a catalog as a materialised list.

- `catalogId` — The catalog UUID.

Returns: A list of all product UUID strings.

## `StreamCatalogProductsAsync`

```csharp
IAsyncEnumerable<ProductUuid> StreamCatalogProductsAsync(string catalogId, CancellationToken ct = default)
```

Streams all full product objects belonging to a catalog, following HAL pagination automatically.

- `catalogId` — The catalog UUID.

Returns: An async stream of `ProductUuid` objects.

## `GetCatalogProductListFullAsync`

```csharp
Task<List<ProductUuid>> GetCatalogProductListFullAsync(string catalogId, CancellationToken ct = default)
```

Returns all full product objects belonging to a catalog as a materialised list.

- `catalogId` — The catalog UUID.

Returns: A list of all `ProductUuid` objects.

## `GetCatalogProductAsync`

```csharp
Task<ProductUuid> GetCatalogProductAsync(string catalogId, string uuid, CancellationToken ct = default)
```

Returns a single catalog product by its UUID.

- `catalogId` — The catalog UUID.
- `uuid` — The product UUID.

Returns: The matching `ProductUuid`.

## `GetCatalogMappedProductListAsync`

```csharp
Task<string> GetCatalogMappedProductListAsync(string catalogId, int limit = 100, string? searchAfter = null, CancellationToken ct = default)
```

Returns a raw JSON string of mapped products for a catalog. The schema depends on the catalog's mapping configuration.

- `catalogId` — The catalog UUID.
- `limit` — Items per page (1–100).
- `searchAfter` — Cursor for keyset pagination.

Returns: Raw JSON response string.

## `GetCatalogMappedModelListAsync`

```csharp
Task<string> GetCatalogMappedModelListAsync(string catalogId, int limit = 100, string? searchAfter = null, CancellationToken ct = default)
```

Returns a raw JSON string of mapped product models for a catalog.

- `catalogId` — The catalog UUID.
- `limit` — Items per page (1–100).
- `searchAfter` — Cursor for keyset pagination.

Returns: Raw JSON response string.

## `GetCatalogMappedVariantListAsync`

```csharp
Task<string> GetCatalogMappedVariantListAsync(string catalogId, string modelCode, int limit = 100, string? searchAfter = null, CancellationToken ct = default)
```

Returns a raw JSON string of mapped variants of a product model for a catalog.

- `catalogId` — The catalog UUID.
- `modelCode` — The product model code whose variants to retrieve.
- `limit` — Items per page (1–100).
- `searchAfter` — Cursor for keyset pagination.

Returns: Raw JSON response string.

## `GetCatalogMappingSchemaAsync`

```csharp
Task<CatalogMappingSchema> GetCatalogMappingSchemaAsync(string catalogId, CancellationToken ct = default)
```

Returns the product mapping schema defined for a catalog.

- `catalogId` — The catalog UUID.

Returns: The `CatalogMappingSchema` for the catalog.

## `CreateCatalogAsync`

```csharp
Task<Catalog> CreateCatalogAsync(Catalog catalog, CancellationToken ct = default)
```

Creates a new catalog and returns the created entity.

- `catalog` — The catalog to create (must include at least `name`).

Returns: The created `Catalog`.

## `UpdateCatalogAsync`

```csharp
Task<Catalog> UpdateCatalogAsync(string catalogId, Catalog catalog, CancellationToken ct = default)
```

Updates a catalog's properties (e.g. name, enabled) and returns the updated entity.

- `catalogId` — The catalog UUID.
- `catalog` — The properties to update.

Returns: The updated `Catalog`.

## `DeleteCatalogAsync`

```csharp
Task DeleteCatalogAsync(string catalogId, CancellationToken ct = default)
```

Deletes a catalog.

- `catalogId` — The catalog UUID to delete.

## `DuplicateCatalogAsync`

```csharp
Task<Catalog> DuplicateCatalogAsync(string catalogId, CancellationToken ct = default)
```

Duplicates an existing catalog and returns the new catalog.

- `catalogId` — The UUID of the catalog to duplicate.

Returns: The newly created duplicate `Catalog`.

## `SetCatalogMappingSchemaAsync`

```csharp
Task<string> SetCatalogMappingSchemaAsync(string catalogId, string schemaJson, CancellationToken ct = default)
```

Creates or replaces the product mapping schema for a catalog.

- `catalogId` — The catalog UUID.
- `schemaJson` — The mapping schema as a raw JSON string.

Returns: Response body string (typically empty on success).

## `DeleteCatalogMappingSchemaAsync`

```csharp
Task DeleteCatalogMappingSchemaAsync(string catalogId, CancellationToken ct = default)
```

Deletes the product mapping schema for a catalog.

- `catalogId` — The catalog UUID.


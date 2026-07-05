# Products — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Products domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

**Domain notes:**
- Two product keys exist: **UUID** (`ProductUuid`, modern) and **identifier/SKU**
  (`ProductIdentifier`, legacy). Same shape otherwise; pick one consistently.
- Streamers use `search_after` internally (no 10k ceiling). Manual `page`/`limit` paging is
  capped at 10 000 items by Akeneo.
- `Enabled` is `bool?`: `null` = server default (**enabled**). See llms.txt pattern 5.
- `CreateProductUuidAsync` with `Uuid = null` lets the server generate the UUID (resolved from
  the 201 Location header). `CreateOrUpdate*` requires the key to be set.
- `UploadProductMediaFileAsync` requires `productJson` or `productModelJson` (exactly one) —
  the API rejects uploads without a link target. Returns the media-file code.
- Drafts/proposals (`Get*DraftAsync`, `Submit*ProposalAsync`) need the Enterprise workflow
  feature; expect 4xx on unlicensed tenants.
- `AkeneoAttribute.Type` literals (instance-confirmed): `pim_catalog_text`, `pim_catalog_textarea`,
  `pim_catalog_identifier`, `pim_catalog_number`, `pim_catalog_boolean`, `pim_catalog_date`,
  `pim_catalog_simpleselect`, `pim_catalog_multiselect`, `pim_catalog_price_collection`,
  `pim_catalog_metric`, `pim_catalog_file`, `pim_catalog_image`, `pim_catalog_asset_collection`,
  `pim_catalog_table`, `pim_reference_data_simpleselect`, `pim_reference_data_multiselect`.
- Server-managed (returned on reads, omit from writes): `Created`, `Updated`, `QualityScores`,
  `Completenesses` on products; `AttributeType` and `LinkedData` on `ProductValue`.

## `StreamProductUuidsAsync`

```csharp
IAsyncEnumerable<ProductUuid> StreamProductUuidsAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Streams all UUID-based products, following keyset (`search_after`) pagination automatically — not subject to Akeneo's 10 000-item page-pagination limit.

- `search` — Optional JSON-encoded Akeneo search filter.
- `scope` — Optional channel scope for attribute completeness filtering.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

## `GetProductUuidListFullAsync`

```csharp
Task<List<ProductUuid>> GetProductUuidListFullAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Returns all UUID-based products as a materialised list.

- `search` — Optional JSON-encoded search filter.
- `scope` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

Returns: A list of all `ProductUuid` objects in the catalog.

## `GetProductUuidListAsync`

```csharp
Task<ProductUuidList> GetProductUuidListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a UUID-based product page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `ProductUuidList` with HAL navigation links.

## `GetProductUuidListAsync`

```csharp
Task<ProductUuidList> GetProductUuidListAsync(int page = 1, int limit = 100, string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Returns a single page of UUID-based products.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `scope` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

Returns: A paginated `ProductUuidList` with HAL navigation links.

## `GetProductUuidAsync`

```csharp
Task<ProductUuid> GetProductUuidAsync(string uuid, CancellationToken ct = default)
```

Returns a single product by its UUID.

- `uuid` — Product UUID.

Returns: The matching `ProductUuid`.

## `GetProductUuidDraftAsync`

```csharp
Task<ProductUuid> GetProductUuidDraftAsync(string uuid, CancellationToken ct = default)
```

Returns the current draft of a product by its UUID (requires the Workflow feature).

- `uuid` — Product UUID.

Returns: The draft `ProductUuid`.

## `CreateOrUpdateProductUuidAsync`

```csharp
Task<ProductUuid> CreateOrUpdateProductUuidAsync(ProductUuid product, CancellationToken ct = default)
```

Creates or updates a product via HTTP PATCH then returns the refreshed entity.

- `product` — The product to create or update. `Uuid` must be set.

Returns: The updated `ProductUuid` as returned by the API.

## `CreateProductUuidAsync`

```csharp
Task<ProductUuid> CreateProductUuidAsync(ProductUuid product, CancellationToken ct = default)
```

Creates a new UUID-based product via HTTP POST and returns the created entity. `Uuid` may be left `null` — Akeneo then generates the UUID, which is resolved from the 201 `Location` response header.

- `product` — The product to create.

Returns: The created `ProductUuid`.

## `DeleteProductUuidAsync`

```csharp
Task DeleteProductUuidAsync(string uuid, CancellationToken ct = default)
```

Deletes a UUID-based product.

- `uuid` — The product UUID to delete.

## `SubmitProductUuidProposalAsync`

```csharp
Task SubmitProductUuidProposalAsync(string uuid, CancellationToken ct = default)
```

Submits a draft of a UUID-based product for approval (requires the Workflow feature).

- `uuid` — The product UUID.

## `SearchProductUuidsAsync`

```csharp
Task<ProductUuidList> SearchProductUuidsAsync(string searchBody, CancellationToken ct = default)
```

Searches UUID-based products using a POST body (supports large search payloads).

- `searchBody` — JSON search payload as defined by the Akeneo API.

Returns: A paginated `ProductUuidList` with HAL navigation links.

## `StreamProductIdentifiersAsync`

```csharp
IAsyncEnumerable<ProductIdentifier> StreamProductIdentifiersAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Streams all identifier-based products (legacy API), following keyset (`search_after`) pagination automatically — not subject to Akeneo's 10 000-item page-pagination limit.

- `search` — Optional JSON-encoded Akeneo search filter.
- `scope` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

## `GetProductIdentifierListFullAsync`

```csharp
Task<List<ProductIdentifier>> GetProductIdentifierListFullAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Returns all identifier-based products as a materialised list.

- `search` — Optional JSON-encoded search filter.
- `scope` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

Returns: A list of all `ProductIdentifier` objects in the catalog.

## `GetProductIdentifierListAsync`

```csharp
Task<ProductIdentifierList> GetProductIdentifierListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns an identifier-based product page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `ProductIdentifierList` with HAL navigation links.

## `GetProductIdentifierListAsync`

```csharp
Task<ProductIdentifierList> GetProductIdentifierListAsync(int page = 1, int limit = 100, string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Returns a single page of identifier-based products.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `scope` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

Returns: A paginated `ProductIdentifierList` with HAL navigation links.

## `GetProductIdentifierAsync`

```csharp
Task<ProductIdentifier> GetProductIdentifierAsync(string identifier, CancellationToken ct = default)
```

Returns a single product by its identifier (SKU).

- `identifier` — The product identifier (SKU).

Returns: The matching `ProductIdentifier`.

## `GetProductIdentifierDraftAsync`

```csharp
Task<ProductIdentifier> GetProductIdentifierDraftAsync(string identifier, CancellationToken ct = default)
```

Returns the current draft of a product by its identifier (requires the Workflow feature).

- `identifier` — The product identifier (SKU).

Returns: The draft `ProductIdentifier`.

## `CreateOrUpdateProductIdentifierAsync`

```csharp
Task<ProductIdentifier> CreateOrUpdateProductIdentifierAsync(ProductIdentifier product, CancellationToken ct = default)
```

Creates or updates an identifier-based product via HTTP PATCH then returns the refreshed entity.

- `product` — The product to create or update. `Identifier` must be set.

Returns: The updated `ProductIdentifier` as returned by the API.

## `CreateProductIdentifierAsync`

```csharp
Task<ProductIdentifier> CreateProductIdentifierAsync(ProductIdentifier product, CancellationToken ct = default)
```

Creates a new identifier-based product via HTTP POST and returns the created entity.

- `product` — The product to create.

Returns: The created `ProductIdentifier`.

## `DeleteProductIdentifierAsync`

```csharp
Task DeleteProductIdentifierAsync(string identifier, CancellationToken ct = default)
```

Deletes an identifier-based product.

- `identifier` — The product identifier (SKU) to delete.

## `SubmitProductIdentifierProposalAsync`

```csharp
Task SubmitProductIdentifierProposalAsync(string identifier, CancellationToken ct = default)
```

Submits a draft of an identifier-based product for approval (requires the Workflow feature).

- `identifier` — The product identifier (SKU).

## `StreamProductModelsAsync`

```csharp
IAsyncEnumerable<ProductModel> StreamProductModelsAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Streams all product models, following keyset (`search_after`) pagination automatically — not subject to Akeneo's 10 000-item page-pagination limit.

- `search` — Optional JSON-encoded Akeneo search filter.
- `scope` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

## `GetProductModelListFullAsync`

```csharp
Task<List<ProductModel>> GetProductModelListFullAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Returns all product models as a materialised list.

- `search` — Optional JSON-encoded search filter.
- `scope` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

Returns: A list of all `ProductModel` objects in the catalog.

## `GetProductModelListAsync`

```csharp
Task<ProductModelList> GetProductModelListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a product model page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `ProductModelList` with HAL navigation links.

## `GetProductModelListAsync`

```csharp
Task<ProductModelList> GetProductModelListAsync(int page = 1, int limit = 100, string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
```

Returns a single page of product models.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `scope` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `withAssetShareLinks` — When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

Returns: A paginated `ProductModelList` with HAL navigation links.

## `GetProductModelAsync`

```csharp
Task<ProductModel> GetProductModelAsync(string code, CancellationToken ct = default)
```

Returns a single product model by its code.

- `code` — The product model code.

Returns: The matching `ProductModel`.

## `GetProductModelDraftAsync`

```csharp
Task<ProductModel> GetProductModelDraftAsync(string code, CancellationToken ct = default)
```

Returns the current draft of a product model by its code (requires the Workflow feature).

- `code` — The product model code.

Returns: The draft `ProductModel`.

## `CreateOrUpdateProductModelAsync`

```csharp
Task<ProductModel> CreateOrUpdateProductModelAsync(ProductModel productModel, CancellationToken ct = default)
```

Creates or updates a product model via HTTP PATCH then returns the refreshed entity.

- `productModel` — The product model to create or update. `Code` must be set.

Returns: The updated `ProductModel` as returned by the API.

## `CreateProductModelAsync`

```csharp
Task<ProductModel> CreateProductModelAsync(ProductModel productModel, CancellationToken ct = default)
```

Creates a new product model via HTTP POST and returns the created entity.

- `productModel` — The product model to create.

Returns: The created `ProductModel`.

## `DeleteProductModelAsync`

```csharp
Task DeleteProductModelAsync(string code, CancellationToken ct = default)
```

Deletes a product model by its code.

- `code` — The product model code to delete.

## `SubmitProductModelProposalAsync`

```csharp
Task SubmitProductModelProposalAsync(string code, CancellationToken ct = default)
```

Submits a draft of a product model for approval (requires the Workflow feature).

- `code` — The product model code.

## `StreamProductMediaFilesAsync`

```csharp
IAsyncEnumerable<ProductMediaFile> StreamProductMediaFilesAsync(CancellationToken ct = default)
```

Streams all product media files, following HAL pagination automatically.

## `GetProductMediaFileListFullAsync`

```csharp
Task<List<ProductMediaFile>> GetProductMediaFileListFullAsync(CancellationToken ct = default)
```

Returns all product media files as a materialised list.

Returns: A list of all `ProductMediaFile` objects.

## `GetProductMediaFileListAsync`

```csharp
Task<ProductMediaFileList> GetProductMediaFileListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a product media file page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `ProductMediaFileList` with HAL navigation links.

## `GetProductMediaFileListAsync`

```csharp
Task<ProductMediaFileList> GetProductMediaFileListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of product media files.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `withCount` — Include total count in API response.

Returns: A paginated `ProductMediaFileList` with HAL navigation links.

## `GetProductMediaFileAsync`

```csharp
Task<ProductMediaFile> GetProductMediaFileAsync(string code, CancellationToken ct = default)
```

Returns the metadata for a single product media file by its code.

- `code` — The media file code (may contain path segments separated by `/`).

Returns: The matching `ProductMediaFile`.

## `DownloadProductMediaFileAsync`

```csharp
Task<byte[]> DownloadProductMediaFileAsync(string code, CancellationToken ct = default)
```

Downloads the binary content of a product media file.

- `code` — The media file code (may contain path segments separated by `/`).

Returns: Raw file bytes.

## `DownloadProductMediaFileStreamAsync`

```csharp
Task<Stream> DownloadProductMediaFileStreamAsync(string code, CancellationToken ct = default)
```

Downloads a product media file as an unbuffered stream (for large files). Dispose the stream to release the HTTP response.

- `code` — The media file code (may contain path segments separated by `/`).

Returns: A stream over the file content.

## `UploadProductMediaFileAsync`

```csharp
Task<string> UploadProductMediaFileAsync(byte[] fileBytes, string fileName, string contentType, string? productJson = null, string? productModelJson = null, CancellationToken ct = default)
```

Uploads a product media file and returns the created media-file code (resolved from the 201 response headers). Akeneo links the upload to the attribute value described by `productJson` or `productModelJson` — the API requires exactly one of the two.

- `fileBytes` — Raw file bytes.
- `fileName` — Original file name (e.g. `photo.jpg`).
- `contentType` — MIME type (e.g. `image/jpeg`).
- `productJson` — JSON object with the target product reference, e.g. `{"identifier":"my-sku","attribute":"picture","scope":null,"locale":null}`.
- `productModelJson` — JSON object with the target product model reference, e.g. `{"code":"my-model","attribute":"picture","scope":null,"locale":null}`.

Returns: The created media-file code.


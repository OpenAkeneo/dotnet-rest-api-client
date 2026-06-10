# OpenAkeneo .NET Client — API Reference

Compact reference for the `AkeneoContext` class in `OpenAkeneo.RestApiClient`.
All methods are `async` and accept an optional `CancellationToken ct` (omitted below for brevity).

- **Index file**: See `OpenAkeneo.RestApiClient.ApiIndex.md` for a line-number lookup table.
- **README**: See `README.md` for installation, DI setup, usage examples, and attribute value handling.
- **Akeneo API docs**: https://api.akeneo.com/api-reference.html

---

## Common Patterns

### Pagination
Most list resources offer three method variants:
| Pattern | Signature | Description |
|---------|-----------|-------------|
| **Stream** | `IAsyncEnumerable<T> Stream…Async(…)` | Lazily follows all pages. Best for large datasets. |
| **Full list** | `Task<List<T>> Get…ListFullAsync(…)` | Materialises every item into a `List<T>`. Convenience wrapper around Stream. |
| **Single page** | `Task<…List> Get…ListAsync(page, limit, …)` | Returns one page with HAL navigation links. |

Common pagination parameters (not repeated per-method below):
- `page` (int): 1-based page number.
- `limit` (int): Results per page, 1–100.
- `withCount` (bool): Include total count (may impact perf on large catalogs).

Some resources (reference entities, asset families) use **keyset/cursor pagination** via `searchAfter` instead of page numbers.

### Error Handling
All methods throw `AkeneoApiException` on non-success HTTP responses.
| Property | Type | Description |
|----------|------|-------------|
| `StatusCode` | `HttpStatusCode` | HTTP status code (401, 403, 404, 422, etc.) |
| `ApiMessage` | `string` | Akeneo error message from response body |
| `RequestUrl` | `string` | The URL that failed |
| `RequestMethod` | `string` | HTTP method used |
| `ResponseBody` | `string?` | Full response body for debugging |

Common HTTP status codes:
- `401` — Authentication required (missing/expired token).
- `403` — Insufficient permissions or payload too large.
- `404` — Resource not found.
- `422` — Validation error in request body.

### Create-or-Update (Upsert)
`CreateOrUpdate…Async()` methods use HTTP PATCH. Akeneo returns 201 on creation (body deserialized directly) or 204 on update (followed by a GET to return the current state).

### Dictionary Overloads
Most `Get…ListAsync` methods have an additional overload accepting `Dictionary<string, string> queryParameters` for advanced/custom query strings not covered by the typed parameters.

### Attribute Value Types
Product/model `values` follow this structure: `{ "attr_code": [{ locale, scope, data }] }`

| Attribute Type | `data` Shape |
|----------------|-------------|
| `pim_catalog_text`, `pim_catalog_textarea`, `pim_catalog_identifier` | `string` |
| `pim_catalog_boolean` | `bool` |
| `pim_catalog_number` | `string` (numeric) |
| `pim_catalog_date` | `string` (ISO 8601) |
| `pim_catalog_simpleselect` | `string` (option code) |
| `pim_catalog_multiselect` | `string[]` (option codes) |
| `pim_catalog_metric` | `{ amount: string, unit: string }` |
| `pim_catalog_price_collection` | `[{ amount: string, currency: string }]` |
| `pim_catalog_image`, `pim_catalog_file` | `string` (media file code) |
| `pim_catalog_asset_collection` | `string[]` (asset codes) |
| `pim_catalog_table` | `[{ column_code: value, … }]` |
| `akeneo_reference_entity`, `akeneo_reference_entity_collection` | `string` / `string[]` (record codes) |

---

## Product UUID
Products identified by UUID (recommended).
**Model:** `ProductUuid`

### `StreamProductUuidsAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `IAsyncEnumerable<ProductUuid>`
Streams all UUID-based products, following HAL pagination automatically.
**Parameters:**
- `search` (string?): Optional JSON-encoded Akeneo search filter.
- `scope` (string?): Optional channel scope for attribute completeness filtering.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductUuidListFullAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `Task<List<ProductUuid>>`
Returns all UUID-based products as a materialised list. A list of all ProductUuid objects in the catalog.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `scope` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductUuidListAsync(int page, int limit, string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `Task<ProductUuidList>`
Returns a single page of UUID-based products. A paginated ProductUuidList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `scope` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductUuidListAsync(Dictionary<string, string> queryParameters)`
**Returns:** `Task<ProductUuidList>`
Returns a UUID-based product page using an arbitrary set of pre-built query parameters. A paginated ProductUuidList with HAL navigation links.
**Parameters:**
- `queryParameters` (Dictionary<string, string>): Raw query-string key/value pairs sent to the Akeneo API.

### `GetProductUuidAsync(string uuid)`
**Returns:** `Task<ProductUuid>`
Returns a single product by its UUID. The matching ProductUuid.
**Parameters:**
- `uuid` (string): Product UUID.

### `GetProductUuidDraftAsync(string uuid)`
**Returns:** `Task<ProductUuid>`
Returns the current draft of a product by its UUID (requires the Workflow feature). The draft ProductUuid.
**Parameters:**
- `uuid` (string): Product UUID.

### `CreateOrUpdateProductUuidAsync(ProductUuid product)`
**Returns:** `Task<ProductUuid>`
Creates or updates a product via HTTP PATCH then returns the refreshed entity. The updated ProductUuid as returned by the API.
**Parameters:**
- `product` (ProductUuid): The product to create or update. Uuid must be set.

### `CreateProductUuidAsync(ProductUuid product)`
**Returns:** `Task<ProductUuid>`
Creates a new UUID-based product via HTTP POST and returns the created entity. The created ProductUuid.
**Parameters:**
- `product` (ProductUuid): The product to create.

### `DeleteProductUuidAsync(string uuid)`
**Returns:** `Task`
Deletes a UUID-based product.
**Parameters:**
- `uuid` (string): The product UUID to delete.

### `SubmitProductUuidProposalAsync(string uuid)`
**Returns:** `Task`
Submits a draft of a UUID-based product for approval (requires the Workflow feature).
**Parameters:**
- `uuid` (string): The product UUID.

### `SearchProductUuidsAsync(string searchBody)`
**Returns:** `Task<ProductUuidList>`
Searches UUID-based products using a POST body (supports large search payloads). A paginated ProductUuidList with HAL navigation links.
**Parameters:**
- `searchBody` (string): JSON search payload as defined by the Akeneo API.

## Product Identifier
Products identified by SKU (legacy).
**Model:** `ProductIdentifier`

### `StreamProductIdentifiersAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `IAsyncEnumerable<ProductIdentifier>`
Streams all identifier-based products (legacy API), following HAL pagination automatically.
**Parameters:**
- `search` (string?): Optional JSON-encoded Akeneo search filter.
- `scope` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductIdentifierListFullAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `Task<List<ProductIdentifier>>`
Returns all identifier-based products as a materialised list. A list of all ProductIdentifier objects in the catalog.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `scope` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductIdentifierListAsync(int page, int limit, string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `Task<ProductIdentifierList>`
Returns a single page of identifier-based products. A paginated ProductIdentifierList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `scope` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductIdentifierListAsync(Dictionary<string, string> queryParameters)`
**Returns:** `Task<ProductIdentifierList>`
Returns an identifier-based product page using an arbitrary set of pre-built query parameters. A paginated ProductIdentifierList with HAL navigation links.
**Parameters:**
- `queryParameters` (Dictionary<string, string>): Raw query-string key/value pairs sent to the Akeneo API.

### `GetProductIdentifierAsync(string identifier)`
**Returns:** `Task<ProductIdentifier>`
Returns a single product by its identifier (SKU). The matching ProductIdentifier.
**Parameters:**
- `identifier` (string): The product identifier (SKU).

### `GetProductIdentifierDraftAsync(string identifier)`
**Returns:** `Task<ProductIdentifier>`
Returns the current draft of a product by its identifier (requires the Workflow feature). The draft ProductIdentifier.
**Parameters:**
- `identifier` (string): The product identifier (SKU).

### `CreateOrUpdateProductIdentifierAsync(ProductIdentifier product)`
**Returns:** `Task<ProductIdentifier>`
Creates or updates an identifier-based product via HTTP PATCH then returns the refreshed entity. The updated ProductIdentifier as returned by the API.
**Parameters:**
- `product` (ProductIdentifier): The product to create or update. Identifier must be set.

### `CreateProductIdentifierAsync(ProductIdentifier product)`
**Returns:** `Task<ProductIdentifier>`
Creates a new identifier-based product via HTTP POST and returns the created entity. The created ProductIdentifier.
**Parameters:**
- `product` (ProductIdentifier): The product to create.

### `DeleteProductIdentifierAsync(string identifier)`
**Returns:** `Task`
Deletes an identifier-based product.
**Parameters:**
- `identifier` (string): The product identifier (SKU) to delete.

### `SubmitProductIdentifierProposalAsync(string identifier)`
**Returns:** `Task`
Submits a draft of an identifier-based product for approval (requires the Workflow feature).
**Parameters:**
- `identifier` (string): The product identifier (SKU).

## Product Model
Parent models for variant products.
**Model:** `ProductModel`

### `StreamProductModelsAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `IAsyncEnumerable<ProductModel>`
Streams all product models, following HAL pagination automatically.
**Parameters:**
- `search` (string?): Optional JSON-encoded Akeneo search filter.
- `scope` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductModelListFullAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `Task<List<ProductModel>>`
Returns all product models as a materialised list. A list of all ProductModel objects in the catalog.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `scope` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductModelListAsync(int page, int limit, string? search, string? scope, string? locales, bool withAssetShareLinks)`
**Returns:** `Task<ProductModelList>`
Returns a single page of product models. A paginated ProductModelList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `scope` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `withAssetShareLinks` (bool): When `true`, populates `linked_data` on asset_collection values with public CDN share link URLs.

### `GetProductModelAsync(string code)`
**Returns:** `Task<ProductModel>`
Returns a single product model by its code. The matching ProductModel.
**Parameters:**
- `code` (string): The product model code.

### `GetProductModelDraftAsync(string code)`
**Returns:** `Task<ProductModel>`
Returns the current draft of a product model by its code (requires the Workflow feature). The draft ProductModel.
**Parameters:**
- `code` (string): The product model code.

### `CreateOrUpdateProductModelAsync(ProductModel productModel)`
**Returns:** `Task<ProductModel>`
Creates or updates a product model via HTTP PATCH then returns the refreshed entity. The updated ProductModel as returned by the API.
**Parameters:**
- `productModel` (ProductModel): The product model to create or update. Code must be set.

### `CreateProductModelAsync(ProductModel productModel)`
**Returns:** `Task<ProductModel>`
Creates a new product model via HTTP POST and returns the created entity. The created ProductModel.
**Parameters:**
- `productModel` (ProductModel): The product model to create.

### `DeleteProductModelAsync(string code)`
**Returns:** `Task`
Deletes a product model by its code.
**Parameters:**
- `code` (string): The product model code to delete.

### `SubmitProductModelProposalAsync(string code)`
**Returns:** `Task`
Submits a draft of a product model for approval (requires the Workflow feature).
**Parameters:**
- `code` (string): The product model code.

## Product Media File
Binary media files attached to products.
**Model:** `ProductMediaFile`

### `StreamProductMediaFilesAsync()`
**Returns:** `IAsyncEnumerable<ProductMediaFile>`
Streams all product media files, following HAL pagination automatically.

### `GetProductMediaFileListFullAsync()`
**Returns:** `Task<List<ProductMediaFile>>`
Returns all product media files as a materialised list. A list of all ProductMediaFile objects.

### `GetProductMediaFileListAsync(int page, int limit, bool withCount)`
**Returns:** `Task<ProductMediaFileList>`
Returns a single page of product media files. A paginated ProductMediaFileList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `withCount` (bool): Include total count in API response.

### `GetProductMediaFileAsync(string code)`
**Returns:** `Task<ProductMediaFile>`
Returns the metadata for a single product media file by its code. The matching ProductMediaFile.
**Parameters:**
- `code` (string): The media file code (may contain path segments separated by `/`).

### `DownloadProductMediaFileAsync(string code)`
**Returns:** `Task<byte[]>`
Downloads the binary content of a product media file. Raw file bytes.
**Parameters:**
- `code` (string): The media file code (may contain path segments separated by `/`).

### `UploadProductMediaFileAsync(byte[] fileBytes, string fileName, string contentType, string? productJson)`
**Returns:** `Task<string>`
/// Uploads a product media file. Returns the response body (typically empty; the new media file /// code is available via the `Location` header which Akeneo includes in the 201 response). /// To associate the file with a product attribute value, include a `productJson` /// part describing the target product, attribute, locale, and scope — see Akeneo docs for the schema. /// Response body string (usually empty on success).
**Parameters:**
- `fileBytes` (byte[]): Raw file bytes.
- `fileName` (string): Original file name (e.g. `photo.jpg`).
- `contentType` (string): MIME type (e.g. `image/jpeg`).
- `productJson` (string?): /// Optional JSON object with the target product reference, e.g. /// `{"identifier":"my-sku","attribute":"picture","scope":null,"locale":null}`. /// When provided, Akeneo links the upload to that attribute value automatically. ///

## Attribute
Product attributes (text, number, select, etc.).
**Model:** `AkeneoAttribute`

### `StreamAttributesAsync(string? search, bool withCount, bool withTableSelectOptions)`
**Returns:** `IAsyncEnumerable<AkeneoAttribute>`
Streams all attributes from the Akeneo catalog one at a time, following HAL pagination automatically.
**Parameters:**
- `search` (string?): Optional JSON-encoded Akeneo search filter.
- `withCount` (bool): When `true`, the response includes the total item count.
- `withTableSelectOptions` (bool): When `true`, table attribute select options are included.

### `GetAttributeListFullAsync(string? search, bool withCount, bool withTableSelectOptions)`
**Returns:** `Task<List<AkeneoAttribute>>`
Returns all attributes as a materialised list by exhausting StreamAttributesAsync. A list of all AkeneoAttribute objects.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total count in API response.
- `withTableSelectOptions` (bool): Include table attribute select options.

### `GetAttributeListAsync(int page, int limit, string? search, bool withCount, bool withTableSelectOptions)`
**Returns:** `Task<AttributeList>`
Returns a single page of attributes. A paginated AttributeList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total count in API response.
- `withTableSelectOptions` (bool): Include table attribute select options.

### `GetAttributeAsync(string attributeCode)`
**Returns:** `Task<AkeneoAttribute>`
Returns a single attribute by its code. The matching AkeneoAttribute.
**Parameters:**
- `attributeCode` (string): The Akeneo attribute code.

### `CreateOrUpdateAttributeAsync(AkeneoAttribute attribute)`
**Returns:** `Task<AkeneoAttribute>`
Creates or updates an attribute via HTTP PATCH then returns the refreshed entity. The updated AkeneoAttribute as returned by the API.
**Parameters:**
- `attribute` (AkeneoAttribute): The attribute to create or update. Code must be set.

### `CreateAttributeAsync(AkeneoAttribute attribute)`
**Returns:** `Task<AkeneoAttribute>`
Creates a new attribute via HTTP POST and returns the created entity. The created AkeneoAttribute.
**Parameters:**
- `attribute` (AkeneoAttribute): The attribute to create.

## Attribute Option
Options for simple/multi-select attributes.
**Model:** `AttributeOption`

### `StreamAttributeOptionsAsync(string attributeCode, bool withCount)`
**Returns:** `IAsyncEnumerable<AttributeOption>`
Streams all options for a given attribute, following HAL pagination automatically.
**Parameters:**
- `attributeCode` (string): The attribute whose options to enumerate.
- `withCount` (bool): When `true`, the response includes the total item count.

### `GetAttributeOptionListFullAsync(string attributeCode, bool withCount)`
**Returns:** `Task<List<AttributeOption>>`
Returns all options for a given attribute as a materialised list. A list of all AttributeOption objects.
**Parameters:**
- `attributeCode` (string): The attribute code.
- `withCount` (bool): Include total count in API response.

### `GetAttributeOptionListAsync(string attributeCode, int page, int limit, bool withCount)`
**Returns:** `Task<AttributeOptionList>`
Returns a single page of options for a given attribute. A paginated AttributeOptionList with HAL navigation links.
**Parameters:**
- `attributeCode` (string): The attribute code.
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `withCount` (bool): Include total count in API response.

### `GetAttributeOptionListAsync(string attributeCode, Dictionary<string, string> queryParameters)`
**Returns:** `Task<AttributeOptionList>`
Returns an attribute option page using an arbitrary set of pre-built query parameters. A paginated AttributeOptionList with HAL navigation links.
**Parameters:**
- `attributeCode` (string): The attribute code.
- `queryParameters` (Dictionary<string, string>): Raw query-string key/value pairs sent to the Akeneo API.

### `GetAttributeOptionAsync(string attributeCode, string attributeOptionCode)`
**Returns:** `Task<AttributeOption>`
Returns a single attribute option by its code. The matching AttributeOption.
**Parameters:**
- `attributeCode` (string): The attribute code.
- `attributeOptionCode` (string): The option code.

### `CreateOrUpdateAttributeOptionAsync(string attributeCode, AttributeOption attributeOption)`
**Returns:** `Task<AttributeOption>`
Creates or updates an attribute option via HTTP PATCH then returns the refreshed entity. The updated AttributeOption as returned by the API.
**Parameters:**
- `attributeCode` (string): The attribute code.
- `attributeOption` (AttributeOption): The option to create or update. Code must be set.

### `CreateAttributeOptionAsync(string attributeCode, AttributeOption attributeOption)`
**Returns:** `Task<AttributeOption>`
Creates a new attribute option via HTTP POST and returns the created entity. The created AttributeOption.
**Parameters:**
- `attributeCode` (string): The attribute code.
- `attributeOption` (AttributeOption): The option to create.

## Attribute Group
Logical groupings of attributes.
**Model:** `AttributeGroup`

### `StreamAttributeGroupsAsync(string? search, bool withCount)`
**Returns:** `IAsyncEnumerable<AttributeGroup>`
Streams all attribute groups, following HAL pagination automatically.
**Parameters:**
- `search` (string?): Optional JSON-encoded Akeneo search filter.
- `withCount` (bool): When `true`, the response includes the total item count.

### `GetAttributeGroupListFullAsync(string? search, bool withCount)`
**Returns:** `Task<List<AttributeGroup>>`
Returns all attribute groups as a materialised list. A list of all AttributeGroup objects.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total count in API response.

### `GetAttributeGroupListAsync(int page, int limit, string? search, bool withCount)`
**Returns:** `Task<AttributeGroupList>`
Returns a single page of attribute groups. A paginated AttributeGroupList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total count in API response.

### `GetAttributeGroupAsync(string attributeCode)`
**Returns:** `Task<AttributeGroup>`
Returns a single attribute group by its code. The matching AttributeGroup.
**Parameters:**
- `attributeCode` (string): The attribute group code.

### `CreateOrUpdateAttributeGroupAsync(AttributeGroup attributeGroup)`
**Returns:** `Task<AttributeGroup>`
Creates or updates an attribute group via HTTP PATCH then returns the refreshed entity. The updated AttributeGroup as returned by the API.
**Parameters:**
- `attributeGroup` (AttributeGroup): The group to create or update. Code must be set.

### `CreateAttributeGroupAsync(AttributeGroup attributeGroup)`
**Returns:** `Task<AttributeGroup>`
Creates a new attribute group via HTTP POST and returns the created entity. The created AttributeGroup.
**Parameters:**
- `attributeGroup` (AttributeGroup): The group to create.

## Family
Product families defining required attributes.
**Model:** `Family`

### `GetFamilyListFullAsync(string? search, bool withCount)`
**Returns:** `Task<List<Family>>`
Returns all product families as a materialised list. A list of all Family objects.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total item count in API response.

### `GetFamilyListAsync(int page, int limit, string? search, bool withCount)`
**Returns:** `Task<FamilyList>`
Returns a single page of product families. A paginated FamilyList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total item count in API response.

### `GetFamilyAsync(string familyCode)`
**Returns:** `Task<Family>`
Returns a single family by its code. The matching Family.
**Parameters:**
- `familyCode` (string): The family code.

### `CreateOrUpdateFamilyAsync(Family family)`
**Returns:** `Task<Family>`
Creates or updates a product family via HTTP PATCH then returns the refreshed entity. The updated Family as returned by the API.
**Parameters:**
- `family` (Family): The family to create or update. Code must be set.

### `CreateFamilyAsync(Family family)`
**Returns:** `Task<Family>`
Creates a new product family via HTTP POST and returns the created entity. The created Family.
**Parameters:**
- `family` (Family): The family to create.

## Family Variant
Variant axis definitions within families.
**Model:** `FamilyVariant`

### `StreamFamilyVariantsAsync(string familyCode, bool withCount)`
**Returns:** `IAsyncEnumerable<FamilyVariant>`
Streams all variants of a given product family, following HAL pagination automatically.
**Parameters:**
- `familyCode` (string): The family code whose variants to enumerate.
- `withCount` (bool): Include total item count in API response.

### `GetFamilyVariantListFullAsync(string familyCode, bool withCount)`
**Returns:** `Task<List<FamilyVariant>>`
Returns all variants of a given product family as a materialised list. A list of all FamilyVariant objects.
**Parameters:**
- `familyCode` (string): The family code.
- `withCount` (bool): Include total item count in API response.

### `GetFamilyVariantListAsync(string familyCode, int page, int limit, bool withCount)`
**Returns:** `Task<FamilyVariantList>`
Returns a single page of variants for a given family. A paginated FamilyVariantList with HAL navigation links.
**Parameters:**
- `familyCode` (string): The family code.
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `withCount` (bool): Include total item count in API response.

### `GetFamilyVariantListAsync(string familyCode, Dictionary<string, string> queryParameters)`
**Returns:** `Task<FamilyVariantList>`
Returns a family variant page using an arbitrary set of pre-built query parameters. A paginated FamilyVariantList with HAL navigation links.
**Parameters:**
- `familyCode` (string): The family code.
- `queryParameters` (Dictionary<string, string>): Raw query-string key/value pairs sent to the Akeneo API.

### `GetFamilyVariantAsync(string familyCode, string code)`
**Returns:** `Task<FamilyVariant>`
Returns a single family variant by its code. The matching FamilyVariant.
**Parameters:**
- `familyCode` (string): The family code.
- `code` (string): The variant code.

### `CreateOrUpdateFamilyVariantAsync(string familyCode, FamilyVariant variant)`
**Returns:** `Task<FamilyVariant>`
Creates or updates a family variant via HTTP PATCH then returns the refreshed entity. The updated FamilyVariant as returned by the API.
**Parameters:**
- `familyCode` (string): The family code.
- `variant` (FamilyVariant): The variant to create or update. Code must be set.

### `CreateFamilyVariantAsync(string familyCode, FamilyVariant variant)`
**Returns:** `Task<FamilyVariant>`
Creates a new family variant via HTTP POST and returns the created entity. The created FamilyVariant.
**Parameters:**
- `familyCode` (string): The family code to create the variant under.
- `variant` (FamilyVariant): The variant to create.

## Category
Product classification tree nodes.
**Model:** `Category`

### `StreamCategoriesAsync(string? search, bool withCount, bool withPosition, bool withEnrichedAttributes)`
**Returns:** `IAsyncEnumerable<Category>`
Streams all categories, following HAL pagination automatically.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total item count in API response.
- `withPosition` (bool): Include category position in tree.
- `withEnrichedAttributes` (bool): Include enriched category attribute values.

### `GetCategoryListFullAsync(string? search, bool withCount, bool withPosition, bool withEnrichedAttributes)`
**Returns:** `Task<List<Category>>`
Returns all categories as a materialised list. A list of all Category objects.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total item count in API response.
- `withPosition` (bool): Include category position in tree.
- `withEnrichedAttributes` (bool): Include enriched category attribute values.

### `GetCategoryListAsync(int page, int limit, string? search, bool withCount, bool withPosition, bool withEnrichedAttributes)`
**Returns:** `Task<CategoryList>`
Returns a single page of categories. A paginated CategoryList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `withCount` (bool): Include total item count in API response.
- `withPosition` (bool): Include category position in tree.
- `withEnrichedAttributes` (bool): Include enriched category attribute values.

### `GetCategoryAsync(string code, bool withPosition, bool withEnrichedAttributes)`
**Returns:** `Task<Category>`
Returns a single category by its code. The matching Category.
**Parameters:**
- `code` (string): The category code.
- `withPosition` (bool): Include category position in tree.
- `withEnrichedAttributes` (bool): Include enriched category attribute values.

### `GetCategoryAsync(string code, Dictionary<string, string> queryParameters)`
**Returns:** `Task<Category>`
Returns a single category by its code using an arbitrary set of pre-built query parameters. The matching Category.
**Parameters:**
- `code` (string): The category code.
- `queryParameters` (Dictionary<string, string>): Raw query-string key/value pairs sent to the Akeneo API.

### `DownloadCategoryMediaFileAsync(string filePath)`
**Returns:** `Task<byte[]>`
Downloads the binary content of a category media file. Raw file bytes.
**Parameters:**
- `filePath` (string): The media file path as returned by the category attribute value.

### `CreateOrUpdateCategoryAsync(Category category)`
**Returns:** `Task<Category>`
Creates or updates a category via HTTP PATCH then returns the refreshed entity. The updated Category as returned by the API.
**Parameters:**
- `category` (Category): The category to create or update. Code must be set.

### `CreateCategoryAsync(Category category)`
**Returns:** `Task<Category>`
Creates a new category via HTTP POST and returns the created entity. The created Category.
**Parameters:**
- `category` (Category): The category to create.

### `UploadCategoryMediaFileAsync(byte[] fileBytes, string fileName, string contentType)`
**Returns:** `Task<string>`
Uploads a category media file and returns the created file code from the response. Response body string (typically the created file code).
**Parameters:**
- `fileBytes` (byte[]): Raw file bytes.
- `fileName` (string): Original file name (e.g. `banner.jpg`).
- `contentType` (string): MIME type (e.g. `image/jpeg`).

## Channel
Sales channels (scopes) with locales and currencies.
**Model:** `Channel`

### `StreamChannelsAsync(bool withCount)`
**Returns:** `IAsyncEnumerable<Channel>`
Streams all channels, following HAL pagination automatically.
**Parameters:**
- `withCount` (bool): Include total item count in API response.

### `GetChannelListFullAsync(bool withCount)`
**Returns:** `Task<List<Channel>>`
Returns all channels as a materialised list. A list of all Channel objects.
**Parameters:**
- `withCount` (bool): Include total item count in API response.

### `GetChannelListAsync(int page, int limit, bool withCount)`
**Returns:** `Task<ChannelList>`
Returns a single page of channels. A paginated ChannelList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `withCount` (bool): Include total item count in API response.

### `GetChannelAsync(string channelCode)`
**Returns:** `Task<Channel>`
Returns a single channel by its code. The matching Channel.
**Parameters:**
- `channelCode` (string): The channel code.

### `CreateOrUpdateChannelAsync(Channel channel)`
**Returns:** `Task<Channel>`
Creates or updates a channel via HTTP PATCH then returns the refreshed entity. The updated Channel as returned by the API.
**Parameters:**
- `channel` (Channel): The channel to create or update. Code must be set.

### `CreateChannelAsync(Channel channel)`
**Returns:** `Task<Channel>`
Creates a new channel via HTTP POST and returns the created entity. The created Channel.
**Parameters:**
- `channel` (Channel): The channel to create.

## Locale
Available locales (e.g. en_US, fr_FR).
**Model:** `Locale`

### `StreamLocalesAsync(bool withCount)`
**Returns:** `IAsyncEnumerable<Locale>`
Streams all locales, following HAL pagination automatically.
**Parameters:**
- `withCount` (bool): Include total item count in API response.

### `GetLocaleListFullAsync(bool withCount)`
**Returns:** `Task<List<Locale>>`
Returns all locales as a materialised list. A list of all Locale objects.
**Parameters:**
- `withCount` (bool): Include total item count in API response.

### `GetLocaleListAsync(int page, int limit, bool withCount)`
**Returns:** `Task<LocaleList>`
Returns a single page of locales. A paginated LocaleList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `withCount` (bool): Include total item count in API response.

### `GetLocaleAsync(string localeCode)`
**Returns:** `Task<Locale>`
Returns a single locale by its code. The matching Locale.
**Parameters:**
- `localeCode` (string): The locale code (e.g. `en_US`).

## Currency
Available currencies (ISO 4217).
**Model:** `Currency`

### `StreamCurrenciesAsync(bool withCount)`
**Returns:** `IAsyncEnumerable<Currency>`
Streams all currencies, following HAL pagination automatically.
**Parameters:**
- `withCount` (bool): Include total item count in API response.

### `GetCurrencyListFullAsync(bool withCount)`
**Returns:** `Task<List<Currency>>`
Returns all currencies as a materialised list. A list of all Currency objects.
**Parameters:**
- `withCount` (bool): Include total item count in API response.

### `GetCurrencyListAsync(int page, int limit, bool withCount)`
**Returns:** `Task<CurrencyList>`
Returns a single page of currencies. A paginated CurrencyList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `withCount` (bool): Include total item count in API response.

### `GetCurrencyAsync(string currencyCode)`
**Returns:** `Task<Currency>`
Returns a single currency by its code. The matching Currency.
**Parameters:**
- `currencyCode` (string): The ISO 4217 currency code (e.g. `EUR`).

## Measurement Family
Units of measure (weight, length, etc.).
**Model:** `MeasurementFamily`

### `GetMeasurementFamilyListAsync()`
**Returns:** `Task<List<MeasurementFamily>>`
/// Returns the full list of measurement families. /// The Akeneo API returns all families in a single non-paginated array, so no paging is required. /// All MeasurementFamily objects defined in the catalog.

### `CreateOrUpdateMeasurementFamiliesAsync(List<MeasurementFamily> measurementFamilies)`
**Returns:** `Task<string>`
/// Creates or updates measurement families in bulk via HTTP PATCH. /// The Akeneo API accepts an array of measurement family objects and returns an array of per-item status results. /// Response body string (JSON array with per-item status codes and errors).
**Parameters:**
- `measurementFamilies` (List<MeasurementFamily>): The list of measurement families to create or update.

## Association Type
Product association types (cross-sell, up-sell, etc.).
**Model:** `AssociationType`

### `StreamAssociationTypesAsync(bool withCount)`
**Returns:** `IAsyncEnumerable<AssociationType>`
Streams all association types, following HAL pagination automatically.
**Parameters:**
- `withCount` (bool): When `true`, the response includes the total item count.

### `GetAssociationTypeListFullAsync(bool withCount)`
**Returns:** `Task<List<AssociationType>>`
Returns all association types as a materialised list. A list of all AssociationType objects.
**Parameters:**
- `withCount` (bool): Include total count in API response.

### `GetAssociationTypeListAsync(int page, int limit, bool withCount)`
**Returns:** `Task<AssociationTypeList>`
Returns a single page of association types. A paginated AssociationTypeList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `withCount` (bool): Include total count in API response.

### `GetAssociationTypeAsync(string attributeCode)`
**Returns:** `Task<AssociationType>`
Returns a single association type by its code. The matching AssociationType.
**Parameters:**
- `attributeCode` (string): The association type code.

### `CreateOrUpdateAssociationTypeAsync(AssociationType associationType)`
**Returns:** `Task<AssociationType>`
Creates or updates an association type via HTTP PATCH then returns the refreshed entity. The updated AssociationType as returned by the API.
**Parameters:**
- `associationType` (AssociationType): The association type to create or update. Code must be set.

### `CreateAssociationTypeAsync(AssociationType associationType)`
**Returns:** `Task<AssociationType>`
Creates a new association type via HTTP POST and returns the created entity. The created AssociationType.
**Parameters:**
- `associationType` (AssociationType): The association type to create.

## Reference Entity
Structured reference data entities.
**Model:** `ReferenceEntity`

### `GetReferenceEntityListFullAsync()`
**Returns:** `Task<List<ReferenceEntity>>`
Returns all reference entities as a materialised list by following keyset pagination automatically. A list of all ReferenceEntity objects.

### `GetReferenceEntityListAsync(string? searchAfter)`
**Returns:** `Task<ReferenceEntityList>`
Returns a page of reference entities, optionally starting after a cursor value. A ReferenceEntityList with HAL navigation links.
**Parameters:**
- `searchAfter` (string?): Cursor value for keyset pagination.

### `GetReferenceEntityAsync(string referenceEntityCode)`
**Returns:** `Task<ReferenceEntity>`
Returns a single reference entity by its code. The matching ReferenceEntity.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.

### `CreateOrUpdateReferenceEntityAsync(ReferenceEntity referenceEntity)`
**Returns:** `Task<ReferenceEntity>`
Creates or updates a reference entity via HTTP PATCH then returns the refreshed entity. The updated ReferenceEntity as returned by the API.
**Parameters:**
- `referenceEntity` (ReferenceEntity): The reference entity to create or update. Code must be set.

## Reference Entity Attribute
Attributes on reference entities.
**Model:** `ReferenceEntityAttribute`

### `GetReferenceEntityAttributeListAsync(string referenceEntityCode)`
**Returns:** `Task<ReferenceEntityAttributeList>`
Returns all attributes for a given reference entity. A ReferenceEntityAttributeList.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.

### `GetReferenceEntityAttributeAsync(string referenceEntityCode, string attributeCode)`
**Returns:** `Task<ReferenceEntityAttribute>`
Returns a single attribute for a reference entity. The matching ReferenceEntityAttribute.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `attributeCode` (string): The attribute code.

### `CreateOrUpdateReferenceEntityAttributeAsync(string referenceEntityCode, ReferenceEntityAttribute attribute)`
**Returns:** `Task<ReferenceEntityAttribute>`
Creates or updates a reference entity attribute via HTTP PATCH then returns the refreshed entity. The updated ReferenceEntityAttribute as returned by the API.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `attribute` (ReferenceEntityAttribute): The attribute to create or update.

## Reference Entity Attribute Option
Options for reference entity select attributes.
**Model:** `ReferenceEntityAttributeOption`

### `GetReferenceEntityAttributeOptionListAsync(string referenceEntityCode, string attributeCode)`
**Returns:** `Task<ReferenceEntityAttributeOptionList>`
Returns all options for a given reference entity attribute. A ReferenceEntityAttributeOptionList.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `attributeCode` (string): The attribute code.

### `GetReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, string optionCode)`
**Returns:** `Task<ReferenceEntityAttributeOption>`
Returns a single option for a reference entity attribute. The matching ReferenceEntityAttributeOption.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `attributeCode` (string): The attribute code.
- `optionCode` (string): The option code.

### `CreateOrUpdateReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, ReferenceEntityAttributeOption option)`
**Returns:** `Task<ReferenceEntityAttributeOption>`
Creates or updates a reference entity attribute option via HTTP PATCH then returns the refreshed entity. The updated ReferenceEntityAttributeOption as returned by the API.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `attributeCode` (string): The attribute code.
- `option` (ReferenceEntityAttributeOption): The option to create or update.

## Reference Entity Record
Data records within reference entities.
**Model:** `ReferenceEntityRecord`

### `StreamReferenceEntityRecordsAsync(string referenceEntityCode, string? search, string? channel, string? locales, string? searchAfter)`
**Returns:** `IAsyncEnumerable<ReferenceEntityRecord>`
Streams all records for a given reference entity, following keyset pagination automatically. An async stream of ReferenceEntityRecord objects.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `search` (string?): Optional JSON-encoded search filter.
- `channel` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `searchAfter` (string?): Optional cursor to resume streaming from a known position.

### `GetReferenceEntityRecordListFullAsync(string referenceEntityCode, string? search, string? channel, string? locales, string? searchAfter)`
**Returns:** `Task<List<ReferenceEntityRecord>>`
Returns all records for a given reference entity as a materialised list by following keyset pagination automatically. A list of all ReferenceEntityRecord objects.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `search` (string?): Optional JSON-encoded search filter.
- `channel` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `searchAfter` (string?): Optional cursor to start from a known position.

### `GetReferenceEntityRecordListAsync(string referenceEntityCode, string? search, string? channel, string? locales, string? searchAfter)`
**Returns:** `Task<ReferenceEntityRecordList>`
Returns a page of records for a given reference entity. A ReferenceEntityRecordList with HAL navigation links.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `search` (string?): Optional JSON-encoded search filter.
- `channel` (string?): Optional channel scope.
- `locales` (string?): Optional comma-separated locale codes.
- `searchAfter` (string?): Cursor for keyset pagination.

### `GetReferenceEntityRecordListAsync(string referenceEntityCode, Dictionary<string, string> queryParameters)`
**Returns:** `Task<ReferenceEntityRecordList>`
Returns a reference entity record page using an arbitrary set of pre-built query parameters. A ReferenceEntityRecordList with HAL navigation links.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `queryParameters` (Dictionary<string, string>): Raw query-string key/value pairs sent to the Akeneo API.

### `GetReferenceEntityRecordAsync(string referenceEntityCode, string recordCode)`
**Returns:** `Task<ReferenceEntityRecord>`
Returns a single record for a reference entity. The matching ReferenceEntityRecord.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `recordCode` (string): The record code.

### `CreateOrUpdateReferenceEntityRecordAsync(string referenceEntityCode, ReferenceEntityRecord record)`
**Returns:** `Task<ReferenceEntityRecord>`
Creates or updates a reference entity record via HTTP PATCH then returns the refreshed entity. The updated ReferenceEntityRecord as returned by the API.
**Parameters:**
- `referenceEntityCode` (string): The reference entity code.
- `record` (ReferenceEntityRecord): The record to create or update. Code must be set.

## Reference Entity Media File
Binary media for reference entity records.

### `DownloadReferenceEntityMediaFileAsync(string mediaFileCode)`
**Returns:** `Task<byte[]>`
Downloads the binary content of a reference entity media file. Raw file bytes.
**Parameters:**
- `mediaFileCode` (string): The media file code as returned by the record attribute value.

### `UploadReferenceEntityMediaFileAsync(byte[] fileBytes, string fileName, string contentType)`
**Returns:** `Task<string>`
Uploads a reference entity media file and returns the created file code from the response. Response body string (contains the created file code).
**Parameters:**
- `fileBytes` (byte[]): Raw file bytes.
- `fileName` (string): Original file name (e.g. `portrait.jpg`).
- `contentType` (string): MIME type (e.g. `image/jpeg`).

## Asset Family
Asset family definitions (DAM-like).
**Model:** `AssetFamily`

### `GetAssetFamilyListFullAsync()`
**Returns:** `Task<List<AssetFamily>>`
Returns all asset families as a materialised list by following keyset pagination automatically. A list of all AssetFamily objects.

### `GetAssetFamilyListAsync(string? searchAfter)`
**Returns:** `Task<AssetFamilyList>`
Returns a page of asset families, optionally starting after a cursor value. An AssetFamilyList with HAL navigation links.
**Parameters:**
- `searchAfter` (string?): Cursor value for keyset pagination (use the last code from the previous page).

### `GetAssetFamilyAsync(string assetFamilyCode)`
**Returns:** `Task<AssetFamily>`
Returns a single asset family by its code. The matching AssetFamily.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.

### `CreateOrUpdateAssetFamilyAsync(AssetFamily assetFamily)`
**Returns:** `Task<AssetFamily>`
Creates or updates an asset family via HTTP PATCH then returns the refreshed entity. The updated AssetFamily as returned by the API.
**Parameters:**
- `assetFamily` (AssetFamily): The asset family to create or update. Code must be set.

## Asset Attribute
Attributes on asset families.
**Model:** `AssetAttribute`

### `GetAssetAttributeListAsync(string assetFamilyCode)`
**Returns:** `Task<AssetAttributeList>`
Returns all attributes for a given asset family. An AssetAttributeList.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.

### `GetAssetAttributeAsync(string assetFamilyCode, string assetAttributeCode)`
**Returns:** `Task<AssetAttribute>`
Returns a single attribute for an asset family. The matching AssetAttribute.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `assetAttributeCode` (string): The attribute code.

### `CreateOrUpdateAssetAttributeAsync(string assetFamilyCode, AssetAttribute assetAttribute)`
**Returns:** `Task<AssetAttribute>`
Creates or updates an asset attribute via HTTP PATCH then returns the refreshed entity. The updated AssetAttribute as returned by the API.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `assetAttribute` (AssetAttribute): The attribute to create or update.

## Asset Attribute Option
Options for asset select attributes.
**Model:** `AssetAttributeOption`

### `GetAssetAttributeOptionListAsync(string assetFamilyCode, string assetAttributeCode)`
**Returns:** `Task<AssetAttributeOptionList>`
Returns all options for a given asset attribute. An AssetAttributeOptionList.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `assetAttributeCode` (string): The attribute code.

### `GetAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, string optionCode)`
**Returns:** `Task<AssetAttributeOption>`
Returns a single option for an asset attribute. The matching AssetAttributeOption.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `assetAttributeCode` (string): The attribute code.
- `optionCode` (string): The option code.

### `CreateOrUpdateAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, AssetAttributeOption option)`
**Returns:** `Task<AssetAttributeOption>`
Creates or updates an asset attribute option via HTTP PATCH then returns the refreshed entity. The updated AssetAttributeOption as returned by the API.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `assetAttributeCode` (string): The attribute code.
- `option` (AssetAttributeOption): The option to create or update.

## Asset Media File
Binary media for assets.

### `DownloadAssetMediaFileAsync(string mediaFileCode)`
**Returns:** `Task<byte[]>`
Downloads the binary content of an asset media file. Raw file bytes.
**Parameters:**
- `mediaFileCode` (string): The media file code (as returned by the asset value data).

### `UploadAssetMediaFileAsync(byte[] fileBytes, string fileName, string contentType)`
**Returns:** `Task<string>`
Uploads an asset media file and returns the created file code from the response. Response body string (contains the created file code).
**Parameters:**
- `fileBytes` (byte[]): Raw file bytes.
- `fileName` (string): Original file name (e.g. `image.jpg`).
- `contentType` (string): MIME type (e.g. `image/jpeg`).

## Asset
Individual asset records within families.
**Model:** `Asset`

### `StreamAssetsAsync(string assetFamilyCode, string? search, string? searchAfter)`
**Returns:** `IAsyncEnumerable<Asset>`
Streams all assets in a given family, following HAL pagination automatically.
**Parameters:**
- `assetFamilyCode` (string): The asset family code to list assets from.
- `search` (string?): Optional JSON-encoded search filter.
- `searchAfter` (string?): Cursor for keyset pagination.

### `GetAssetListFullAsync(string assetFamilyCode, string? search, string? searchAfter)`
**Returns:** `Task<List<Asset>>`
Returns all assets in a given family as a materialised list. A list of all Asset objects in the family.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `search` (string?): Optional JSON-encoded search filter.
- `searchAfter` (string?): Cursor for keyset pagination.

### `GetAssetListAsync(string assetFamilyCode, int limit, string? search, string? searchAfter)`
**Returns:** `Task<AssetList>`
Returns a single page of assets for a given family. A paginated AssetList with HAL navigation links.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `searchAfter` (string?): Cursor for keyset pagination (use the value from the previous page's `next` link).

### `GetAssetListAsync(string assetFamilyCode, Dictionary<string, string> queryParameters)`
**Returns:** `Task<AssetList>`
Returns an asset page using an arbitrary set of pre-built query parameters. A paginated AssetList with HAL navigation links.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `queryParameters` (Dictionary<string, string>): Raw query-string key/value pairs sent to the Akeneo API.

### `GetAssetAsync(string assetFamilyCode, string code)`
**Returns:** `Task<Asset>`
Returns a single asset by its code within a given family. The matching Asset.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `code` (string): The asset code.

### `CreateOrUpdateAssetAsync(string assetFamilyCode, Asset asset)`
**Returns:** `Task<Asset>`
Creates or updates an asset via HTTP PATCH then returns the refreshed entity. The updated Asset as returned by the API.
**Parameters:**
- `assetFamilyCode` (string): The asset family code.
- `asset` (Asset): The asset to create or update. Code must be set.

## Catalog
Catalog for Apps — curated product selections.
**Model:** `Catalog`

### `GetCatalogListAsync(int page, int limit)`
**Returns:** `Task<CatalogList>`
Returns a page of catalogs (Catalog for Apps feature). A CatalogList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).

### `StreamCatalogsAsync()`
**Returns:** `IAsyncEnumerable<Catalog>`
Streams all catalogs, following HAL pagination automatically.

### `GetCatalogListFullAsync()`
**Returns:** `Task<List<Catalog>>`
Returns all catalogs as a materialised list. A list of all Catalog objects.

### `GetCatalogAsync(string id)`
**Returns:** `Task<Catalog>`
Returns a single catalog by its ID. The matching Catalog.
**Parameters:**
- `id` (string): The catalog UUID.

### `GetCatalogProductUuidListAsync(string catalogId, int limit, string? searchAfter)`
**Returns:** `Task<CatalogProductUuidList>`
Returns a page of product UUIDs belonging to a catalog. A CatalogProductUuidList with HAL navigation links.
**Parameters:**
- `catalogId` (string): The catalog UUID.
- `limit` (int): Items per page (1–100).
- `searchAfter` (string?): Cursor for keyset pagination. This endpoint paginates only via `search_after`.

### `GetCatalogProductListAsync(string catalogId, int limit, string? searchAfter)`
**Returns:** `Task<ProductUuidList>`
Returns a page of full product objects belonging to a catalog. A ProductUuidList with HAL navigation links.
**Parameters:**
- `catalogId` (string): The catalog UUID.
- `limit` (int): Items per page (1–100).
- `searchAfter` (string?): Cursor for keyset pagination. This endpoint paginates only via `search_after`.

### `StreamCatalogProductUuidsAsync(string catalogId)`
**Returns:** `IAsyncEnumerable<string>`
Streams all product UUIDs belonging to a catalog, following HAL pagination automatically. An async stream of product UUID strings.
**Parameters:**
- `catalogId` (string): The catalog UUID.

### `GetCatalogProductUuidListFullAsync(string catalogId)`
**Returns:** `Task<List<string>>`
Returns all product UUIDs belonging to a catalog as a materialised list. A list of all product UUID strings.
**Parameters:**
- `catalogId` (string): The catalog UUID.

### `StreamCatalogProductsAsync(string catalogId)`
**Returns:** `IAsyncEnumerable<ProductUuid>`
Streams all full product objects belonging to a catalog, following HAL pagination automatically. An async stream of ProductUuid objects.
**Parameters:**
- `catalogId` (string): The catalog UUID.

### `GetCatalogProductListFullAsync(string catalogId)`
**Returns:** `Task<List<ProductUuid>>`
Returns all full product objects belonging to a catalog as a materialised list. A list of all ProductUuid objects.
**Parameters:**
- `catalogId` (string): The catalog UUID.

### `GetCatalogProductAsync(string catalogId, string uuid)`
**Returns:** `Task<ProductUuid>`
Returns a single catalog product by its UUID. The matching ProductUuid.
**Parameters:**
- `catalogId` (string): The catalog UUID.
- `uuid` (string): The product UUID.

### `GetCatalogMappedProductListAsync(string catalogId, int limit, string? searchAfter)`
**Returns:** `Task<string>`
Returns a raw JSON string of mapped products for a catalog. The schema depends on the catalog's mapping configuration. Raw JSON response string.
**Parameters:**
- `catalogId` (string): The catalog UUID.
- `limit` (int): Items per page (1–100).
- `searchAfter` (string?): Cursor for keyset pagination.

### `GetCatalogMappedModelListAsync(string catalogId, int limit, string? searchAfter)`
**Returns:** `Task<string>`
Returns a raw JSON string of mapped product models for a catalog. Raw JSON response string.
**Parameters:**
- `catalogId` (string): The catalog UUID.
- `limit` (int): Items per page (1–100).
- `searchAfter` (string?): Cursor for keyset pagination.

### `GetCatalogMappedVariantListAsync(string catalogId, string modelCode, int limit, string? searchAfter)`
**Returns:** `Task<string>`
Returns a raw JSON string of mapped variants of a product model for a catalog. Raw JSON response string.
**Parameters:**
- `catalogId` (string): The catalog UUID.
- `modelCode` (string): The product model code whose variants to retrieve.
- `limit` (int): Items per page (1–100).
- `searchAfter` (string?): Cursor for keyset pagination.

### `GetCatalogMappingSchemaAsync(string catalogId)`
**Returns:** `Task<CatalogMappingSchema>`
Returns the product mapping schema defined for a catalog. The CatalogMappingSchema for the catalog.
**Parameters:**
- `catalogId` (string): The catalog UUID.

### `CreateCatalogAsync(Catalog catalog)`
**Returns:** `Task<Catalog>`
Creates a new catalog and returns the created entity. The created Catalog.
**Parameters:**
- `catalog` (Catalog): The catalog to create (must include at least `name`).

### `UpdateCatalogAsync(string catalogId, Catalog catalog)`
**Returns:** `Task<Catalog>`
Updates a catalog's properties (e.g. name, enabled) and returns the updated entity. The updated Catalog.
**Parameters:**
- `catalogId` (string): The catalog UUID.
- `catalog` (Catalog): The properties to update.

### `DeleteCatalogAsync(string catalogId)`
**Returns:** `Task`
Deletes a catalog.
**Parameters:**
- `catalogId` (string): The catalog UUID to delete.

### `DuplicateCatalogAsync(string catalogId)`
**Returns:** `Task<Catalog>`
Duplicates an existing catalog and returns the new catalog. The newly created duplicate Catalog.
**Parameters:**
- `catalogId` (string): The UUID of the catalog to duplicate.

### `SetCatalogMappingSchemaAsync(string catalogId, string schemaJson)`
**Returns:** `Task<string>`
Creates or replaces the product mapping schema for a catalog. Response body string (typically empty on success).
**Parameters:**
- `catalogId` (string): The catalog UUID.
- `schemaJson` (string): The mapping schema as a raw JSON string.

### `DeleteCatalogMappingSchemaAsync(string catalogId)`
**Returns:** `Task`
Deletes the product mapping schema for a catalog.
**Parameters:**
- `catalogId` (string): The catalog UUID.

## Job
Import/export job definitions and executions.
**Model:** `Job`

### `LaunchExportJobAsync(string code, bool isDryRun)`
**Returns:** `Task<JobExecutionResult>`
Launches an export job by its code. A JobExecutionResult containing the new execution ID.
**Parameters:**
- `code` (string): The export job code.
- `isDryRun` (bool): When `true`, the job runs in dry-run mode without committing changes.

### `LaunchImportJobAsync(string code, bool isDryRun, string? importMode)`
**Returns:** `Task<JobExecutionResult>`
Launches an import job by its code. A JobExecutionResult containing the new execution ID.
**Parameters:**
- `code` (string): The import job code.
- `isDryRun` (bool): When `true`, the job runs in dry-run mode without committing changes.
- `importMode` (string?): Optional import mode override (e.g. `"add_or_update"`).

## Workflow
Enterprise workflow definitions (optional feature).
**Model:** `Workflow`

### `StreamWorkflowsAsync()`
**Returns:** `IAsyncEnumerable<Workflow>`
/// Streams all workflows, following HAL pagination automatically. /// Note: Workflows are an optional Akeneo Enterprise feature. The endpoint may not respond /// if the feature is disabled; apply a caller-side timeout when consuming this stream. ///

### `GetWorkflowListFullAsync()`
**Returns:** `Task<List<Workflow>>`
Returns all workflows as a materialised list. A list of all Workflow objects.

### `GetWorkflowListAsync(int page, int limit)`
**Returns:** `Task<WorkflowList>`
Returns a single page of workflows. A paginated WorkflowList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).

### `GetWorkflowAsync(string uuid)`
**Returns:** `Task<Workflow>`
Returns a single workflow by its UUID. The matching Workflow.
**Parameters:**
- `uuid` (string): The workflow UUID.

## Workflow Step Assignee
Users assigned to workflow steps.
**Model:** `WorkflowStepAssignee`

### `StreamWorkflowStepAssigneesAsync(string stepUuid)`
**Returns:** `IAsyncEnumerable<WorkflowStepAssignee>`
Streams all assignees for a given workflow step, following HAL pagination automatically.
**Parameters:**
- `stepUuid` (string): The workflow step UUID.

### `GetWorkflowStepAssigneeListFullAsync(string stepUuid)`
**Returns:** `Task<List<WorkflowStepAssignee>>`
Returns all assignees for a workflow step as a materialised list. A list of all WorkflowStepAssignee objects.
**Parameters:**
- `stepUuid` (string): The workflow step UUID.

### `GetWorkflowStepAssigneeListAsync(string stepUuid, int page, int limit)`
**Returns:** `Task<WorkflowStepAssigneeList>`
Returns a single page of assignees for a workflow step. A paginated WorkflowStepAssigneeList with HAL navigation links.
**Parameters:**
- `stepUuid` (string): The workflow step UUID.
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).

### `GetWorkflowStepAssigneeListAsync(string stepUuid, Dictionary<string, string> queryParameters)`
**Returns:** `Task<WorkflowStepAssigneeList>`
Returns a workflow step assignee page using an arbitrary set of pre-built query parameters. A paginated WorkflowStepAssigneeList with HAL navigation links.
**Parameters:**
- `stepUuid` (string): The workflow step UUID.
- `queryParameters` (Dictionary<string, string>): Raw query-string key/value pairs sent to the Akeneo API.

## Workflow Task
Individual workflow task instances.
**Model:** `WorkflowTask`

### `StreamWorkflowTasksAsync(string? search, bool withAttributes)`
**Returns:** `IAsyncEnumerable<WorkflowTask>`
Streams all workflow tasks, following HAL pagination automatically.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `withAttributes` (bool): When `true`, includes attribute values in the response.

### `GetWorkflowTaskListFullAsync(string? search, bool withAttributes)`
**Returns:** `Task<List<WorkflowTask>>`
Returns all workflow tasks as a materialised list. A list of all WorkflowTask objects.
**Parameters:**
- `search` (string?): Optional JSON-encoded search filter.
- `withAttributes` (bool): When `true`, includes attribute values in the response.

### `GetWorkflowTaskListAsync(int page, int limit, string? search, bool withAttributes)`
**Returns:** `Task<WorkflowTaskList>`
Returns a single page of workflow tasks. A paginated WorkflowTaskList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).
- `search` (string?): Optional JSON-encoded search filter.
- `withAttributes` (bool): When `true`, includes attribute values in the response.

### `GetWorkflowTaskAsync(string uuid)`
**Returns:** `Task<WorkflowTask>`
Returns a single workflow task by its UUID. The matching WorkflowTask.
**Parameters:**
- `uuid` (string): The workflow task UUID.

## System & Permissions
System info, API overview, permissions, extensions, and modelization suggestions.

### `GetSystemInformationAsync()`
**Returns:** `Task<SystemInformation>`
Returns system information for the Akeneo instance (version, edition, etc.). A SystemInformation object.

### `GetUserChannelsPermissionsAsync(string userUuid)`
**Returns:** `Task<UserChannelsPermissions>`
Returns the channel permissions for a given user. A UserChannelsPermissions object.
**Parameters:**
- `userUuid` (string): The user UUID.

### `GetUserLocalesPermissionsAsync(string userUuid)`
**Returns:** `Task<UserLocalesPermissions>`
Returns the locale permissions for a given user. A UserLocalesPermissions object.
**Parameters:**
- `userUuid` (string): The user UUID.

### `GetApiOverviewAsync()`
**Returns:** `Task<ApiOverview>`
Returns the Akeneo REST API overview (root HAL response with top-level navigation links). An ApiOverview object.

### `GetExtensionListAsync()`
**Returns:** `Task<List<Extension>>`
/// Returns all UI extensions associated with the current token. /// Unlike most Akeneo list endpoints, `GET /ui-extensions` is not paginated and /// returns a bare JSON array, so this method returns the full list directly. /// A list of all Extension objects.

### `GetModelizationSuggestionListAsync(int page, int limit)`
**Returns:** `Task<ModelizationSuggestionList>`
Returns a page of Data Architect modelization suggestions. A ModelizationSuggestionList with HAL navigation links.
**Parameters:**
- `page` (int): 1-based page number.
- `limit` (int): Items per page (1–100).

### `GetModelizationSuggestionAsync(string uuid)`
**Returns:** `Task<ModelizationSuggestion>`
Returns a single Data Architect modelization suggestion by its UUID. The matching ModelizationSuggestion.
**Parameters:**
- `uuid` (string): The suggestion UUID.

---

## Key Schemas

These are the primary .NET model types used as method parameters and return values.
All models use `System.Text.Json` serialization with `JsonPropertyName` attributes mapping to Akeneo's snake_case JSON.

### ProductUuid
| Property | Type | Description |
|----------|------|-------------|
| `Uuid` | `string` | Product UUID |
| `Enabled` | `bool` | Whether the product is enabled |
| `Family` | `string?` | Family code |
| `Categories` | `List<string>` | Category codes |
| `Groups` | `List<string>` | Group codes |
| `Parent` | `string?` | Parent product model code |
| `Values` | `Dictionary<string, List<ProductValue>>` | `{ "attr_code": [{ Locale, Scope, Data, LinkedData }] }` |
| `Associations` | `Dictionary<string, Association>` | `{ "assoc_type": { Products, ProductModels, Groups } }` |
| `QuantifiedAssociations` | `Dictionary<string, QuantifiedAssociation>` | `{ "assoc_type": { Products: [{ Uuid, Quantity }] } }` |
| `Created` | `string` | ISO 8601 timestamp |
| `Updated` | `string` | ISO 8601 timestamp |
| `Metadata` | `JsonElement?` | Workflow status, etc. |
| `QualityScores` | `List<QualityScore>?` | Per-channel/locale quality scores |
| `Completenesses` | `List<Completeness>?` | Per-channel/locale completeness |

### ProductIdentifier
| Property | Type | Description |
|----------|------|-------------|
| `Identifier` | `string` | Product SKU |
| `Uuid` | `string?` | Product UUID (also available on legacy endpoint) |
| `Enabled` | `bool` | Whether the product is enabled |
| `Family` | `string?` | Family code |
| `Categories` | `List<string>` | Category codes |
| `Values` | `Dictionary<string, List<ProductValue>>` | Same structure as ProductUuid.Values |

### ProductModel
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Product model code |
| `Family` | `string?` | Family code |
| `FamilyVariant` | `string` | Family variant code |
| `Parent` | `string?` | Parent model code |
| `Categories` | `List<string>` | Category codes |
| `Values` | `Dictionary<string, List<ProductValue>>` | Attribute values |

### AkeneoAttribute
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Attribute code |
| `Type` | `string` | Attribute type (pim_catalog_text, etc.) |
| `Labels` | `Dictionary<string, string>` | Localized labels |
| `Group` | `string` | Attribute group code |
| `Localizable` | `bool` | Whether values vary by locale |
| `Scopable` | `bool` | Whether values vary by channel |
| `Unique` | `bool` | Whether values must be unique |

### AttributeOption
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Option code |
| `Attribute` | `string` | Parent attribute code |
| `SortOrder` | `int` | Display order |
| `Labels` | `Dictionary<string, string>` | Localized labels |

### Family
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Family code |
| `AttributeAsLabel` | `string` | Attribute used as product label |
| `AttributeAsImage` | `string?` | Attribute used as product image |
| `Attributes` | `List<string>` | Attribute codes in this family |
| `AttributeRequirements` | `Dictionary<string, List<string>>` | Required attrs per channel |
| `Labels` | `Dictionary<string, string>` | Localized labels |

### Category
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Category code |
| `Parent` | `string?` | Parent category code |
| `Labels` | `Dictionary<string, string>` | Localized labels |
| `Position` | `int` | Sort position in tree |

### Channel
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Channel code |
| `Locales` | `List<string>` | Enabled locale codes |
| `Currencies` | `List<string>` | Enabled currency codes |
| `CategoryTree` | `string` | Root category code |
| `Labels` | `Dictionary<string, string>` | Localized labels |

### ReferenceEntity
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Entity code |
| `Labels` | `Dictionary<string, string>` | Localized labels |
| `Image` | `string?` | Image media file code |

### ReferenceEntityRecord
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Record code |
| `Values` | `Dictionary<string, List<RecordValue>>` | Attribute values |

### AssetFamily
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Family code |
| `Labels` | `Dictionary<string, string>` | Localized labels |
| `AttributeAsMainMedia` | `string?` | Main media attribute code |

### Asset
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Asset code |
| `AssetFamilyCode` | `string` | Parent family code |
| `Values` | `Dictionary<string, List<AssetValue>>` | Attribute values |

### Catalog
| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Catalog UUID |
| `Name` | `string` | Display name |
| `Enabled` | `bool` | Whether the catalog is active |

### ProductMediaFile
| Property | Type | Description |
|----------|------|-------------|
| `Code` | `string` | Media file code |
| `OriginalFilename` | `string` | Original file name |
| `MimeType` | `string` | MIME type |
| `Size` | `int` | File size in bytes |
| `Extension` | `string` | File extension |

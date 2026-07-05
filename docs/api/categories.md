# Categories — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Categories domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.0 surface — do not edit by hand.

**Domain notes:**
- Categories are not deletable via the API. New categories need an existing `Parent` code.
- `UploadCategoryMediaFileAsync` requires `categoryJson`
  (`{"code":"...","attribute_code":"...","channel":...,"locale":...}`) and the enriched-categories
  feature (template with attributes) on the tenant.
- `withPosition`/`withEnrichedAttributes` enrichments are opt-in query flags on reads.

## `StreamCategoriesAsync`

```csharp
IAsyncEnumerable<Category> StreamCategoriesAsync(string? search = null, bool withCount = false, bool withPosition = false, bool withEnrichedAttributes = false, CancellationToken ct = default)
```

Streams all categories, following HAL pagination automatically.

- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total item count in API response.
- `withPosition` — Include category position in tree.
- `withEnrichedAttributes` — Include enriched category attribute values.

## `GetCategoryListFullAsync`

```csharp
Task<List<Category>> GetCategoryListFullAsync(string? search = null, bool withCount = false, bool withPosition = false, bool withEnrichedAttributes = false, CancellationToken ct = default)
```

Returns all categories as a materialised list.

- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total item count in API response.
- `withPosition` — Include category position in tree.
- `withEnrichedAttributes` — Include enriched category attribute values.

Returns: A list of all `Category` objects.

## `GetCategoryListAsync`

```csharp
Task<CategoryList> GetCategoryListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a category page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `CategoryList` with HAL navigation links.

## `GetCategoryListAsync`

```csharp
Task<CategoryList> GetCategoryListAsync(int page = 1, int limit = 100, string? search = null, bool withCount = false, bool withPosition = false, bool withEnrichedAttributes = false, CancellationToken ct = default)
```

Returns a single page of categories.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total item count in API response.
- `withPosition` — Include category position in tree.
- `withEnrichedAttributes` — Include enriched category attribute values.

Returns: A paginated `CategoryList` with HAL navigation links.

## `GetCategoryAsync`

```csharp
Task<Category> GetCategoryAsync(string code, Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a single category by its code using an arbitrary set of pre-built query parameters.

- `code` — The category code.
- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: The matching `Category`.

## `GetCategoryAsync`

```csharp
Task<Category> GetCategoryAsync(string code, bool withPosition = false, bool withEnrichedAttributes = false, CancellationToken ct = default)
```

Returns a single category by its code.

- `code` — The category code.
- `withPosition` — Include category position in tree.
- `withEnrichedAttributes` — Include enriched category attribute values.

Returns: The matching `Category`.

## `DownloadCategoryMediaFileAsync`

```csharp
Task<byte[]> DownloadCategoryMediaFileAsync(string filePath, CancellationToken ct = default)
```

Downloads the binary content of a category media file.

- `filePath` — The media file path as returned by the category attribute value.

Returns: Raw file bytes.

## `DownloadCategoryMediaFileStreamAsync`

```csharp
Task<Stream> DownloadCategoryMediaFileStreamAsync(string filePath, CancellationToken ct = default)
```

Downloads a category media file as an unbuffered stream (for large files). Dispose the stream to release the HTTP response.

- `filePath` — The media file path as returned by the category attribute value.

Returns: A stream over the file content.

## `CreateOrUpdateCategoryAsync`

```csharp
Task<Category> CreateOrUpdateCategoryAsync(Category category, CancellationToken ct = default)
```

Creates or updates a category via HTTP PATCH then returns the refreshed entity.

- `category` — The category to create or update. `Code` must be set.

Returns: The updated `Category` as returned by the API.

## `CreateCategoryAsync`

```csharp
Task<Category> CreateCategoryAsync(Category category, CancellationToken ct = default)
```

Creates a new category via HTTP POST and returns the created entity.

- `category` — The category to create.

Returns: The created `Category`.

## `UploadCategoryMediaFileAsync`

```csharp
Task<string> UploadCategoryMediaFileAsync(byte[] fileBytes, string fileName, string contentType, string categoryJson, CancellationToken ct = default)
```

Uploads a category media file and returns the created file code (resolved from the 201 response headers). The API requires the `categoryJson` part describing which enriched-category attribute value the file belongs to.

- `fileBytes` — Raw file bytes.
- `fileName` — Original file name (e.g. `banner.jpg`).
- `contentType` — MIME type (e.g. `image/jpeg`).
- `categoryJson` — JSON object with the target category reference (required by the API), e.g. `{"code":"ecomm","attribute_code":"image_1","channel":"ecommerce","locale":"en_US"}`.

Returns: The created media-file code.


# Reference Entities — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Reference Entities domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

**Domain notes:**
- Hierarchy: reference **entity** → **attributes** → attribute **options** → **records**.
  Nothing in this domain is deletable via the API — plan codes accordingly.
- `ReferenceEntityAttribute.Type` literals (instance-confirmed): `text`, `text_area`, `number`,
  `yes_no`, `single_option`, `multiple_options`, `image`, `reference_entity_single_link`,
  `reference_entity_multiple_links`. `ValidationRule` literals (text types): `none`, `email`,
  `url`, `regexp`.
- Record value `Data` shapes: `text`/`text_area`/`single_option` → `string`; `number` →
  `long`/`double`; `yes_no` → `bool`; `multiple_options`/`reference_entity_multiple_links` →
  list of `string`; `image` → media-file code; `reference_entity_single_link` → record code.
- Server-managed on `ReferenceEntityRecord`: `ReferenceEntityCode`, `Created`, `Updated` —
  omit from writes.
- All listings paginate by `search_after` cursor only.
- The cross-entity endpoint (`GetAllReferenceEntityRecordListAsync` /
  `StreamAllReferenceEntityRecordsAsync`) is **search-index backed and eventually consistent**:
  records created moments ago may take seconds to appear. The per-entity records list is
  strongly consistent — prefer it for read-after-write.

## `GetReferenceEntityListFullAsync`

```csharp
Task<List<ReferenceEntity>> GetReferenceEntityListFullAsync(CancellationToken ct = default)
```

Returns all reference entities as a materialised list by following keyset pagination automatically.

Returns: A list of all `ReferenceEntity` objects.

## `GetReferenceEntityListAsync`

```csharp
Task<ReferenceEntityList> GetReferenceEntityListAsync(string? searchAfter = null, CancellationToken ct = default)
```

Returns a page of reference entities, optionally starting after a cursor value.

- `searchAfter` — Cursor value for keyset pagination.

Returns: A `ReferenceEntityList` with HAL navigation links.

## `GetReferenceEntityListAsync`

```csharp
Task<ReferenceEntityList> GetReferenceEntityListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a reference entity page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A `ReferenceEntityList` with HAL navigation links.

## `GetReferenceEntityAsync`

```csharp
Task<ReferenceEntity> GetReferenceEntityAsync(string referenceEntityCode, CancellationToken ct = default)
```

Returns a single reference entity by its code.

- `referenceEntityCode` — The reference entity code.

Returns: The matching `ReferenceEntity`.

## `CreateOrUpdateReferenceEntityAsync`

```csharp
Task<ReferenceEntity> CreateOrUpdateReferenceEntityAsync(ReferenceEntity referenceEntity, CancellationToken ct = default)
```

Creates or updates a reference entity via HTTP PATCH then returns the refreshed entity.

- `referenceEntity` — The reference entity to create or update. `Code` must be set.

Returns: The updated `ReferenceEntity` as returned by the API.

## `GetReferenceEntityAttributeListAsync`

```csharp
Task<ReferenceEntityAttributeList> GetReferenceEntityAttributeListAsync(string referenceEntityCode, CancellationToken ct = default)
```

Returns all attributes for a given reference entity.

- `referenceEntityCode` — The reference entity code.

Returns: A `ReferenceEntityAttributeList`.

## `GetReferenceEntityAttributeAsync`

```csharp
Task<ReferenceEntityAttribute> GetReferenceEntityAttributeAsync(string referenceEntityCode, string attributeCode, CancellationToken ct = default)
```

Returns a single attribute for a reference entity.

- `referenceEntityCode` — The reference entity code.
- `attributeCode` — The attribute code.

Returns: The matching `ReferenceEntityAttribute`.

## `CreateOrUpdateReferenceEntityAttributeAsync`

```csharp
Task<ReferenceEntityAttribute> CreateOrUpdateReferenceEntityAttributeAsync(string referenceEntityCode, ReferenceEntityAttribute attribute, CancellationToken ct = default)
```

Creates or updates a reference entity attribute via HTTP PATCH then returns the refreshed entity.

- `referenceEntityCode` — The reference entity code.
- `attribute` — The attribute to create or update.

Returns: The updated `ReferenceEntityAttribute` as returned by the API.

## `GetReferenceEntityAttributeOptionListAsync`

```csharp
Task<ReferenceEntityAttributeOptionList> GetReferenceEntityAttributeOptionListAsync(string referenceEntityCode, string attributeCode, CancellationToken ct = default)
```

Returns all options for a given reference entity attribute.

- `referenceEntityCode` — The reference entity code.
- `attributeCode` — The attribute code.

Returns: A `ReferenceEntityAttributeOptionList`.

## `GetReferenceEntityAttributeOptionAsync`

```csharp
Task<ReferenceEntityAttributeOption> GetReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, string optionCode, CancellationToken ct = default)
```

Returns a single option for a reference entity attribute.

- `referenceEntityCode` — The reference entity code.
- `attributeCode` — The attribute code.
- `optionCode` — The option code.

Returns: The matching `ReferenceEntityAttributeOption`.

## `CreateOrUpdateReferenceEntityAttributeOptionAsync`

```csharp
Task<ReferenceEntityAttributeOption> CreateOrUpdateReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, ReferenceEntityAttributeOption option, CancellationToken ct = default)
```

Creates or updates a reference entity attribute option via HTTP PATCH then returns the refreshed entity.

- `referenceEntityCode` — The reference entity code.
- `attributeCode` — The attribute code.
- `option` — The option to create or update.

Returns: The updated `ReferenceEntityAttributeOption` as returned by the API.

## `StreamReferenceEntityRecordsAsync`

```csharp
IAsyncEnumerable<ReferenceEntityRecord> StreamReferenceEntityRecordsAsync(string referenceEntityCode, string? search = null, string? channel = null, string? locales = null, string? searchAfter = null, CancellationToken ct = default)
```

Streams all records for a given reference entity, following keyset pagination automatically.

- `referenceEntityCode` — The reference entity code.
- `search` — Optional JSON-encoded search filter.
- `channel` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `searchAfter` — Optional cursor to resume streaming from a known position.

Returns: An async stream of `ReferenceEntityRecord` objects.

## `GetReferenceEntityRecordListFullAsync`

```csharp
Task<List<ReferenceEntityRecord>> GetReferenceEntityRecordListFullAsync(string referenceEntityCode, string? search = null, string? channel = null, string? locales = null, string? searchAfter = null, CancellationToken ct = default)
```

Returns all records for a given reference entity as a materialised list by following keyset pagination automatically.

- `referenceEntityCode` — The reference entity code.
- `search` — Optional JSON-encoded search filter.
- `channel` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `searchAfter` — Optional cursor to start from a known position.

Returns: A list of all `ReferenceEntityRecord` objects.

## `GetReferenceEntityRecordListAsync`

```csharp
Task<ReferenceEntityRecordList> GetReferenceEntityRecordListAsync(string referenceEntityCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a reference entity record page using an arbitrary set of pre-built query parameters.

- `referenceEntityCode` — The reference entity code.
- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A `ReferenceEntityRecordList` with HAL navigation links.

## `GetReferenceEntityRecordListAsync`

```csharp
Task<ReferenceEntityRecordList> GetReferenceEntityRecordListAsync(string referenceEntityCode, string? search = null, string? channel = null, string? locales = null, string? searchAfter = null, CancellationToken ct = default)
```

Returns a page of records for a given reference entity.

- `referenceEntityCode` — The reference entity code.
- `search` — Optional JSON-encoded search filter.
- `channel` — Optional channel scope.
- `locales` — Optional comma-separated locale codes.
- `searchAfter` — Cursor for keyset pagination.

Returns: A `ReferenceEntityRecordList` with HAL navigation links.

## `GetReferenceEntityRecordAsync`

```csharp
Task<ReferenceEntityRecord> GetReferenceEntityRecordAsync(string referenceEntityCode, string recordCode, CancellationToken ct = default)
```

Returns a single record for a reference entity.

- `referenceEntityCode` — The reference entity code.
- `recordCode` — The record code.

Returns: The matching `ReferenceEntityRecord`.

## `CreateOrUpdateReferenceEntityRecordAsync`

```csharp
Task<ReferenceEntityRecord> CreateOrUpdateReferenceEntityRecordAsync(string referenceEntityCode, ReferenceEntityRecord record, CancellationToken ct = default)
```

Creates or updates a reference entity record via HTTP PATCH then returns the refreshed entity.

- `referenceEntityCode` — The reference entity code.
- `record` — The record to create or update. `Code` must be set.

Returns: The updated `ReferenceEntityRecord` as returned by the API.

## `GetAllReferenceEntityRecordListAsync`

```csharp
Task<ReferenceEntityRecordList> GetAllReferenceEntityRecordListAsync(Dictionary<string, string>? queryParameters = null, CancellationToken ct = default)
```

Returns a page of records across all reference entities (`GET /reference-entities/records`). Supports the `reference_entity`, `search`, `channel`, `locales` and `search_after` filters via `queryParameters`.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A `ReferenceEntityRecordList` with HAL navigation links.

## `StreamAllReferenceEntityRecordsAsync`

```csharp
IAsyncEnumerable<ReferenceEntityRecord> StreamAllReferenceEntityRecordsAsync(string? referenceEntity = null, string? search = null, CancellationToken ct = default)
```

Streams records across all reference entities, following keyset pagination automatically.

- `referenceEntity` — Optional reference entity code filter.
- `search` — Optional JSON-encoded search filter.

## `DownloadReferenceEntityMediaFileAsync`

```csharp
Task<byte[]> DownloadReferenceEntityMediaFileAsync(string mediaFileCode, CancellationToken ct = default)
```

Downloads the binary content of a reference entity media file.

- `mediaFileCode` — The media file code as returned by the record attribute value.

Returns: Raw file bytes.

## `DownloadReferenceEntityMediaFileStreamAsync`

```csharp
Task<Stream> DownloadReferenceEntityMediaFileStreamAsync(string mediaFileCode, CancellationToken ct = default)
```

Downloads a reference entity media file as an unbuffered stream (for large files). Dispose the stream to release the HTTP response.

- `mediaFileCode` — The media file code (may contain path segments separated by `/`).

Returns: A stream over the file content.

## `UploadReferenceEntityMediaFileAsync`

```csharp
Task<string> UploadReferenceEntityMediaFileAsync(byte[] fileBytes, string fileName, string contentType, CancellationToken ct = default)
```

Uploads a reference entity media file and returns the created file code (resolved from the 201 response headers).

- `fileBytes` — Raw file bytes.
- `fileName` — Original file name (e.g. `portrait.jpg`).
- `contentType` — MIME type (e.g. `image/jpeg`).

Returns: The created media-file code.


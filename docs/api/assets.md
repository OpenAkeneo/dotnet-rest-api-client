# Assets — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Assets domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

**Domain notes:**
- Hierarchy: asset **family** → asset **attributes** → attribute **options** → **assets**.
  Families and attributes are not deletable via the API; assets are.
- `AssetAttribute.Type` literals (instance-confirmed): `text`, `media_file`, `number`,
  `boolean`, `single_option`, `multiple_options`. `MediaType` literals (for `media_file`):
  `image`, `pdf`, `youtube`, `vimeo`, `other`.
- `AssetValue.Data` shapes by attribute type: `text`/`single_option` → `string`;
  `media_file` → `string` media-file code (plus `Links` download hrefs and `LinkedData` file
  metadata — both server-managed, never write them); `number` → `long`/`double`;
  `boolean` → `bool`; `multiple_options` → list of `string`.
- Server-managed on `Asset`: `Created`, `Updated` — returned on reads, omit from writes.
- Asset lists paginate by `search_after` cursor only (no page numbers).
- `UploadAssetMediaFileAsync` needs no link target; attach the returned code to an asset value
  afterwards via `CreateOrUpdateAssetAsync`.
- The cross-family media-file code from an upload round-trips byte-for-byte through
  `DownloadAssetMediaFileAsync`/`...StreamAsync`.

## `GetAssetFamilyListFullAsync`

```csharp
Task<List<AssetFamily>> GetAssetFamilyListFullAsync(CancellationToken ct = default)
```

Returns all asset families as a materialised list by following keyset pagination automatically.

Returns: A list of all `AssetFamily` objects.

## `StreamAssetFamiliesAsync`

```csharp
IAsyncEnumerable<AssetFamily> StreamAssetFamiliesAsync(CancellationToken ct = default)
```

Streams all asset families, following keyset pagination automatically.

## `GetAssetFamilyListAsync`

```csharp
Task<AssetFamilyList> GetAssetFamilyListAsync(string? searchAfter = null, CancellationToken ct = default)
```

Returns a page of asset families, optionally starting after a cursor value.

- `searchAfter` — Cursor value for keyset pagination (use the last code from the previous page).

Returns: An `AssetFamilyList` with HAL navigation links.

## `GetAssetFamilyListAsync`

```csharp
Task<AssetFamilyList> GetAssetFamilyListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns an asset family page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: An `AssetFamilyList` with HAL navigation links.

## `GetAssetFamilyAsync`

```csharp
Task<AssetFamily> GetAssetFamilyAsync(string assetFamilyCode, CancellationToken ct = default)
```

Returns a single asset family by its code.

- `assetFamilyCode` — The asset family code.

Returns: The matching `AssetFamily`.

## `CreateOrUpdateAssetFamilyAsync`

```csharp
Task<AssetFamily> CreateOrUpdateAssetFamilyAsync(AssetFamily assetFamily, CancellationToken ct = default)
```

Creates or updates an asset family via HTTP PATCH then returns the refreshed entity.

- `assetFamily` — The asset family to create or update. `Code` must be set.

Returns: The updated `AssetFamily` as returned by the API.

## `GetAssetAttributeListAsync`

```csharp
Task<AssetAttributeList> GetAssetAttributeListAsync(string assetFamilyCode, CancellationToken ct = default)
```

Returns all attributes for a given asset family.

- `assetFamilyCode` — The asset family code.

Returns: An `AssetAttributeList`.

## `GetAssetAttributeAsync`

```csharp
Task<AssetAttribute> GetAssetAttributeAsync(string assetFamilyCode, string assetAttributeCode, CancellationToken ct = default)
```

Returns a single attribute for an asset family.

- `assetFamilyCode` — The asset family code.
- `assetAttributeCode` — The attribute code.

Returns: The matching `AssetAttribute`.

## `CreateOrUpdateAssetAttributeAsync`

```csharp
Task<AssetAttribute> CreateOrUpdateAssetAttributeAsync(string assetFamilyCode, AssetAttribute assetAttribute, CancellationToken ct = default)
```

Creates or updates an asset attribute via HTTP PATCH then returns the refreshed entity.

- `assetFamilyCode` — The asset family code.
- `assetAttribute` — The attribute to create or update.

Returns: The updated `AssetAttribute` as returned by the API.

## `GetAssetAttributeOptionListAsync`

```csharp
Task<AssetAttributeOptionList> GetAssetAttributeOptionListAsync(string assetFamilyCode, string assetAttributeCode, CancellationToken ct = default)
```

Returns all options for a given asset attribute.

- `assetFamilyCode` — The asset family code.
- `assetAttributeCode` — The attribute code.

Returns: An `AssetAttributeOptionList`.

## `GetAssetAttributeOptionAsync`

```csharp
Task<AssetAttributeOption> GetAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, string optionCode, CancellationToken ct = default)
```

Returns a single option for an asset attribute.

- `assetFamilyCode` — The asset family code.
- `assetAttributeCode` — The attribute code.
- `optionCode` — The option code.

Returns: The matching `AssetAttributeOption`.

## `CreateOrUpdateAssetAttributeOptionAsync`

```csharp
Task<AssetAttributeOption> CreateOrUpdateAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, AssetAttributeOption option, CancellationToken ct = default)
```

Creates or updates an asset attribute option via HTTP PATCH then returns the refreshed entity.

- `assetFamilyCode` — The asset family code.
- `assetAttributeCode` — The attribute code.
- `option` — The option to create or update.

Returns: The updated `AssetAttributeOption` as returned by the API.

## `DownloadAssetMediaFileAsync`

```csharp
Task<byte[]> DownloadAssetMediaFileAsync(string mediaFileCode, CancellationToken ct = default)
```

Downloads the binary content of an asset media file.

- `mediaFileCode` — The media file code (as returned by the asset value data; may contain path segments separated by `/`).

Returns: Raw file bytes.

## `DownloadAssetMediaFileStreamAsync`

```csharp
Task<Stream> DownloadAssetMediaFileStreamAsync(string mediaFileCode, CancellationToken ct = default)
```

Downloads an asset media file as an unbuffered stream (for large files). Dispose the stream to release the HTTP response.

- `mediaFileCode` — The media file code (may contain path segments separated by `/`).

Returns: A stream over the file content.

## `UploadAssetMediaFileAsync`

```csharp
Task<string> UploadAssetMediaFileAsync(byte[] fileBytes, string fileName, string contentType, CancellationToken ct = default)
```

Uploads an asset media file and returns the created file code (resolved from the 201 response headers).

- `fileBytes` — Raw file bytes.
- `fileName` — Original file name (e.g. `image.jpg`).
- `contentType` — MIME type (e.g. `image/jpeg`).

Returns: The created media-file code.

## `StreamAssetsAsync`

```csharp
IAsyncEnumerable<Asset> StreamAssetsAsync(string assetFamilyCode, string? search = null, string? searchAfter = null, CancellationToken ct = default)
```

Streams all assets in a given family, following HAL pagination automatically.

- `assetFamilyCode` — The asset family code to list assets from.
- `search` — Optional JSON-encoded search filter.
- `searchAfter` — Cursor for keyset pagination.

## `GetAssetListFullAsync`

```csharp
Task<List<Asset>> GetAssetListFullAsync(string assetFamilyCode, string? search = null, string? searchAfter = null, CancellationToken ct = default)
```

Returns all assets in a given family as a materialised list.

- `assetFamilyCode` — The asset family code.
- `search` — Optional JSON-encoded search filter.
- `searchAfter` — Cursor for keyset pagination.

Returns: A list of all `Asset` objects in the family.

## `GetAssetListAsync`

```csharp
Task<AssetList> GetAssetListAsync(string assetFamilyCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns an asset page using an arbitrary set of pre-built query parameters.

- `assetFamilyCode` — The asset family code.
- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `AssetList` with HAL navigation links.

## `GetAssetListAsync`

```csharp
Task<AssetList> GetAssetListAsync(string assetFamilyCode, int limit = 100, string? search = null, string? searchAfter = null, CancellationToken ct = default)
```

Returns a single page of assets for a given family.

- `assetFamilyCode` — The asset family code.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `searchAfter` — Cursor for keyset pagination (use the value from the previous page's `next` link).

Returns: A paginated `AssetList` with HAL navigation links.

## `GetAssetAsync`

```csharp
Task<Asset> GetAssetAsync(string assetFamilyCode, string code, CancellationToken ct = default)
```

Returns a single asset by its code within a given family.

- `assetFamilyCode` — The asset family code.
- `code` — The asset code.

Returns: The matching `Asset`.

## `CreateOrUpdateAssetAsync`

```csharp
Task<Asset> CreateOrUpdateAssetAsync(string assetFamilyCode, Asset asset, CancellationToken ct = default)
```

Creates or updates an asset via HTTP PATCH then returns the refreshed entity.

- `assetFamilyCode` — The asset family code.
- `asset` — The asset to create or update. `Code` must be set.

Returns: The updated `Asset` as returned by the API.

## `DeleteAssetAsync`

```csharp
Task DeleteAssetAsync(string assetFamilyCode, string code, CancellationToken ct = default)
```

Deletes an asset by its code within a given family.

- `assetFamilyCode` — The asset family code.
- `code` — The asset code to delete.


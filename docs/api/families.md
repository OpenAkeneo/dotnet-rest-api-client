# Families — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Families domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.0 surface — do not edit by hand.

## `StreamFamiliesAsync`

```csharp
IAsyncEnumerable<Family> StreamFamiliesAsync(string? search = null, bool withCount = false, CancellationToken ct = default)
```

Streams all product families, following HAL pagination automatically.

- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total item count in API response.

## `GetFamilyListFullAsync`

```csharp
Task<List<Family>> GetFamilyListFullAsync(string? search = null, bool withCount = false, CancellationToken ct = default)
```

Returns all product families as a materialised list.

- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total item count in API response.

Returns: A list of all `Family` objects.

## `GetFamilyListAsync`

```csharp
Task<FamilyList> GetFamilyListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a family page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `FamilyList` with HAL navigation links.

## `GetFamilyListAsync`

```csharp
Task<FamilyList> GetFamilyListAsync(int page = 1, int limit = 100, string? search = null, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of product families.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total item count in API response.

Returns: A paginated `FamilyList` with HAL navigation links.

## `GetFamilyAsync`

```csharp
Task<Family> GetFamilyAsync(string familyCode, CancellationToken ct = default)
```

Returns a single family by its code.

- `familyCode` — The family code.

Returns: The matching `Family`.

## `CreateOrUpdateFamilyAsync`

```csharp
Task<Family> CreateOrUpdateFamilyAsync(Family family, CancellationToken ct = default)
```

Creates or updates a product family via HTTP PATCH then returns the refreshed entity.

- `family` — The family to create or update. `Code` must be set.

Returns: The updated `Family` as returned by the API.

## `CreateFamilyAsync`

```csharp
Task<Family> CreateFamilyAsync(Family family, CancellationToken ct = default)
```

Creates a new product family via HTTP POST and returns the created entity.

- `family` — The family to create.

Returns: The created `Family`.

## `StreamFamilyVariantsAsync`

```csharp
IAsyncEnumerable<FamilyVariant> StreamFamilyVariantsAsync(string familyCode, bool withCount = false, CancellationToken ct = default)
```

Streams all variants of a given product family, following HAL pagination automatically.

- `familyCode` — The family code whose variants to enumerate.
- `withCount` — Include total item count in API response.

## `GetFamilyVariantListFullAsync`

```csharp
Task<List<FamilyVariant>> GetFamilyVariantListFullAsync(string familyCode, bool withCount = false, CancellationToken ct = default)
```

Returns all variants of a given product family as a materialised list.

- `familyCode` — The family code.
- `withCount` — Include total item count in API response.

Returns: A list of all `FamilyVariant` objects.

## `GetFamilyVariantListAsync`

```csharp
Task<FamilyVariantList> GetFamilyVariantListAsync(string familyCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a family variant page using an arbitrary set of pre-built query parameters.

- `familyCode` — The family code.
- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `FamilyVariantList` with HAL navigation links.

## `GetFamilyVariantListAsync`

```csharp
Task<FamilyVariantList> GetFamilyVariantListAsync(string familyCode, int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of variants for a given family.

- `familyCode` — The family code.
- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `withCount` — Include total item count in API response.

Returns: A paginated `FamilyVariantList` with HAL navigation links.

## `GetFamilyVariantAsync`

```csharp
Task<FamilyVariant> GetFamilyVariantAsync(string familyCode, string code, CancellationToken ct = default)
```

Returns a single family variant by its code.

- `familyCode` — The family code.
- `code` — The variant code.

Returns: The matching `FamilyVariant`.

## `CreateOrUpdateFamilyVariantAsync`

```csharp
Task<FamilyVariant> CreateOrUpdateFamilyVariantAsync(string familyCode, FamilyVariant variant, CancellationToken ct = default)
```

Creates or updates a family variant via HTTP PATCH then returns the refreshed entity.

- `familyCode` — The family code.
- `variant` — The variant to create or update. `Code` must be set.

Returns: The updated `FamilyVariant` as returned by the API.

## `CreateFamilyVariantAsync`

```csharp
Task<FamilyVariant> CreateFamilyVariantAsync(string familyCode, FamilyVariant variant, CancellationToken ct = default)
```

Creates a new family variant via HTTP POST and returns the created entity.

- `familyCode` — The family code to create the variant under.
- `variant` — The variant to create.

Returns: The created `FamilyVariant`.


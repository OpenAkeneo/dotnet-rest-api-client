# Association Types — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Association Types domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.0 surface — do not edit by hand.

## `StreamAssociationTypesAsync`

```csharp
IAsyncEnumerable<AssociationType> StreamAssociationTypesAsync(bool withCount = false, CancellationToken ct = default)
```

Streams all association types, following HAL pagination automatically.

- `withCount` — When `true`, the response includes the total item count.

## `GetAssociationTypeListFullAsync`

```csharp
Task<List<AssociationType>> GetAssociationTypeListFullAsync(bool withCount = false, CancellationToken ct = default)
```

Returns all association types as a materialised list.

- `withCount` — Include total count in API response.

Returns: A list of all `AssociationType` objects.

## `GetAssociationTypeListAsync`

```csharp
Task<AssociationTypeList> GetAssociationTypeListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns an association type page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `AssociationTypeList` with HAL navigation links.

## `GetAssociationTypeListAsync`

```csharp
Task<AssociationTypeList> GetAssociationTypeListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of association types.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `withCount` — Include total count in API response.

Returns: A paginated `AssociationTypeList` with HAL navigation links.

## `GetAssociationTypeAsync`

```csharp
Task<AssociationType> GetAssociationTypeAsync(string code, CancellationToken ct = default)
```

Returns a single association type by its code.

- `code` — The association type code.

Returns: The matching `AssociationType`.

## `CreateOrUpdateAssociationTypeAsync`

```csharp
Task<AssociationType> CreateOrUpdateAssociationTypeAsync(AssociationType associationType, CancellationToken ct = default)
```

Creates or updates an association type via HTTP PATCH then returns the refreshed entity.

- `associationType` — The association type to create or update. `Code` must be set.

Returns: The updated `AssociationType` as returned by the API.

## `CreateAssociationTypeAsync`

```csharp
Task<AssociationType> CreateAssociationTypeAsync(AssociationType associationType, CancellationToken ct = default)
```

Creates a new association type via HTTP POST and returns the created entity.

- `associationType` — The association type to create.

Returns: The created `AssociationType`.


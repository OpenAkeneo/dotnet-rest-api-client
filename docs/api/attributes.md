# Attributes — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Attributes domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

## `StreamAttributesAsync`

```csharp
IAsyncEnumerable<AkeneoAttribute> StreamAttributesAsync(string? search = null, bool withCount = false, bool withTableSelectOptions = false, CancellationToken ct = default)
```

Streams all attributes from the Akeneo catalog one at a time, following HAL pagination automatically.

- `search` — Optional JSON-encoded Akeneo search filter.
- `withCount` — When `true`, the response includes the total item count.
- `withTableSelectOptions` — When `true`, table attribute select options are included.

## `GetAttributeListFullAsync`

```csharp
Task<List<AkeneoAttribute>> GetAttributeListFullAsync(string? search = null, bool withCount = false, bool withTableSelectOptions = false, CancellationToken ct = default)
```

Returns all attributes as a materialised list by exhausting `CancellationToken)`.

- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total count in API response.
- `withTableSelectOptions` — Include table attribute select options.

Returns: A list of all `AkeneoAttribute` objects.

## `GetAttributeListAsync`

```csharp
Task<AttributeList> GetAttributeListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns an attribute page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `AttributeList` with HAL navigation links.

## `GetAttributeListAsync`

```csharp
Task<AttributeList> GetAttributeListAsync(int page = 1, int limit = 100, string? search = null, bool withCount = false, bool withTableSelectOptions = false, CancellationToken ct = default)
```

Returns a single page of attributes.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total count in API response.
- `withTableSelectOptions` — Include table attribute select options.

Returns: A paginated `AttributeList` with HAL navigation links.

## `GetAttributeAsync`

```csharp
Task<AkeneoAttribute> GetAttributeAsync(string attributeCode, CancellationToken ct = default)
```

Returns a single attribute by its code.

- `attributeCode` — The Akeneo attribute code.

Returns: The matching `AkeneoAttribute`.

## `CreateOrUpdateAttributeAsync`

```csharp
Task<AkeneoAttribute> CreateOrUpdateAttributeAsync(AkeneoAttribute attribute, CancellationToken ct = default)
```

Creates or updates an attribute via HTTP PATCH then returns the refreshed entity.

- `attribute` — The attribute to create or update. `Code` must be set.

Returns: The updated `AkeneoAttribute` as returned by the API.

## `CreateAttributeAsync`

```csharp
Task<AkeneoAttribute> CreateAttributeAsync(AkeneoAttribute attribute, CancellationToken ct = default)
```

Creates a new attribute via HTTP POST and returns the created entity.

- `attribute` — The attribute to create.

Returns: The created `AkeneoAttribute`.

## `StreamAttributeOptionsAsync`

```csharp
IAsyncEnumerable<AttributeOption> StreamAttributeOptionsAsync(string attributeCode, bool withCount = false, CancellationToken ct = default)
```

Streams all options for a given attribute, following HAL pagination automatically.

- `attributeCode` — The attribute whose options to enumerate.
- `withCount` — When `true`, the response includes the total item count.

## `GetAttributeOptionListFullAsync`

```csharp
Task<List<AttributeOption>> GetAttributeOptionListFullAsync(string attributeCode, bool withCount = false, CancellationToken ct = default)
```

Returns all options for a given attribute as a materialised list.

- `attributeCode` — The attribute code.
- `withCount` — Include total count in API response.

Returns: A list of all `AttributeOption` objects.

## `GetAttributeOptionListAsync`

```csharp
Task<AttributeOptionList> GetAttributeOptionListAsync(string attributeCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns an attribute option page using an arbitrary set of pre-built query parameters.

- `attributeCode` — The attribute code.
- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `AttributeOptionList` with HAL navigation links.

## `GetAttributeOptionListAsync`

```csharp
Task<AttributeOptionList> GetAttributeOptionListAsync(string attributeCode, int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of options for a given attribute.

- `attributeCode` — The attribute code.
- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `withCount` — Include total count in API response.

Returns: A paginated `AttributeOptionList` with HAL navigation links.

## `GetAttributeOptionAsync`

```csharp
Task<AttributeOption> GetAttributeOptionAsync(string attributeCode, string attributeOptionCode, CancellationToken ct = default)
```

Returns a single attribute option by its code.

- `attributeCode` — The attribute code.
- `attributeOptionCode` — The option code.

Returns: The matching `AttributeOption`.

## `CreateOrUpdateAttributeOptionAsync`

```csharp
Task<AttributeOption> CreateOrUpdateAttributeOptionAsync(string attributeCode, AttributeOption attributeOption, CancellationToken ct = default)
```

Creates or updates an attribute option via HTTP PATCH then returns the refreshed entity.

- `attributeCode` — The attribute code.
- `attributeOption` — The option to create or update. `Code` must be set.

Returns: The updated `AttributeOption` as returned by the API.

## `CreateAttributeOptionAsync`

```csharp
Task<AttributeOption> CreateAttributeOptionAsync(string attributeCode, AttributeOption attributeOption, CancellationToken ct = default)
```

Creates a new attribute option via HTTP POST and returns the created entity.

- `attributeCode` — The attribute code.
- `attributeOption` — The option to create.

Returns: The created `AttributeOption`.

## `StreamAttributeGroupsAsync`

```csharp
IAsyncEnumerable<AttributeGroup> StreamAttributeGroupsAsync(string? search = null, bool withCount = false, CancellationToken ct = default)
```

Streams all attribute groups, following HAL pagination automatically.

- `search` — Optional JSON-encoded Akeneo search filter.
- `withCount` — When `true`, the response includes the total item count.

## `GetAttributeGroupListFullAsync`

```csharp
Task<List<AttributeGroup>> GetAttributeGroupListFullAsync(string? search = null, bool withCount = false, CancellationToken ct = default)
```

Returns all attribute groups as a materialised list.

- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total count in API response.

Returns: A list of all `AttributeGroup` objects.

## `GetAttributeGroupListAsync`

```csharp
Task<AttributeGroupList> GetAttributeGroupListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns an attribute group page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `AttributeGroupList` with HAL navigation links.

## `GetAttributeGroupListAsync`

```csharp
Task<AttributeGroupList> GetAttributeGroupListAsync(int page = 1, int limit = 100, string? search = null, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of attribute groups.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `withCount` — Include total count in API response.

Returns: A paginated `AttributeGroupList` with HAL navigation links.

## `GetAttributeGroupAsync`

```csharp
Task<AttributeGroup> GetAttributeGroupAsync(string code, CancellationToken ct = default)
```

Returns a single attribute group by its code.

- `code` — The attribute group code.

Returns: The matching `AttributeGroup`.

## `CreateOrUpdateAttributeGroupAsync`

```csharp
Task<AttributeGroup> CreateOrUpdateAttributeGroupAsync(AttributeGroup attributeGroup, CancellationToken ct = default)
```

Creates or updates an attribute group via HTTP PATCH then returns the refreshed entity.

- `attributeGroup` — The group to create or update. `Code` must be set.

Returns: The updated `AttributeGroup` as returned by the API.

## `CreateAttributeGroupAsync`

```csharp
Task<AttributeGroup> CreateAttributeGroupAsync(AttributeGroup attributeGroup, CancellationToken ct = default)
```

Creates a new attribute group via HTTP POST and returns the created entity.

- `attributeGroup` — The group to create.

Returns: The created `AttributeGroup`.


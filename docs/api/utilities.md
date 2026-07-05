# Utilities — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Utilities domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

**Domain notes:**
- UI-extensions and Data-Architect modelization endpoints are feature/plan-gated.
- Modelization request/response bodies are passed as raw JSON strings (frontier endpoints whose
  schemas are still loose).
- `GetExtensionListAsync` returns a bare array (not HAL-paginated).

## `GetSystemInformationAsync`

```csharp
Task<SystemInformation> GetSystemInformationAsync(CancellationToken ct = default)
```

Returns system information for the Akeneo instance (version, edition, etc.).

Returns: A `SystemInformation` object.

## `GetUserChannelsPermissionsAsync`

```csharp
Task<UserChannelsPermissions> GetUserChannelsPermissionsAsync(string userUuid, CancellationToken ct = default)
```

Returns the channel permissions for a given user.

- `userUuid` — The user UUID.

Returns: A `UserChannelsPermissions` object.

## `GetUserLocalesPermissionsAsync`

```csharp
Task<UserLocalesPermissions> GetUserLocalesPermissionsAsync(string userUuid, CancellationToken ct = default)
```

Returns the locale permissions for a given user.

- `userUuid` — The user UUID.

Returns: A `UserLocalesPermissions` object.

## `GetApiOverviewAsync`

```csharp
Task<ApiOverview> GetApiOverviewAsync(CancellationToken ct = default)
```

Returns the Akeneo REST API overview (root HAL response with top-level navigation links).

Returns: An `ApiOverview` object.

## `GetExtensionListAsync`

```csharp
Task<List<Extension>> GetExtensionListAsync(CancellationToken ct = default)
```

Returns all UI extensions associated with the current token. Unlike most Akeneo list endpoints, `GET /ui-extensions` is not paginated and returns a bare JSON array, so this method returns the full list directly.

Returns: A list of all `Extension` objects.

## `CreateExtensionAsync`

```csharp
Task<Extension> CreateExtensionAsync(Extension extension, CancellationToken ct = default)
```

Creates a new UI extension and returns the created entity.

- `extension` — The extension to create (name, type, position, configuration…).

Returns: The created `Extension` (including its server-assigned UUID).

## `UpdateExtensionAsync`

```csharp
Task<Extension> UpdateExtensionAsync(string uuid, Extension extension, CancellationToken ct = default)
```

Updates a UI extension and returns the updated entity.

- `uuid` — The extension UUID.
- `extension` — The properties to update.

Returns: The updated `Extension`.

## `DeleteExtensionAsync`

```csharp
Task DeleteExtensionAsync(string uuid, CancellationToken ct = default)
```

Deletes a UI extension.

- `uuid` — The extension UUID to delete.

## `UpdateExtensionWithFileAsync`

```csharp
Task<string> UpdateExtensionWithFileAsync(string uuid, byte[] fileBytes, string fileName, IReadOnlyDictionary<string, string>? fields = null, string contentType = "text/javascript", CancellationToken ct = default)
```

Updates a UI extension with a file upload (`sdk_script` extensions). String fields use the multipart form names from the Akeneo API reference, e.g. `name`, `version`, `configuration[default_label]`, `credentials[0][code]`.

- `uuid` — The extension UUID.
- `fileBytes` — The updated script file content.
- `fileName` — Original file name (e.g. `extension.js`).
- `fields` — Additional multipart string fields to update.
- `contentType` — MIME type of the file (defaults to `text/javascript`).

Returns: Raw JSON response body (the updated extension).

## `GetModelizationSuggestionListAsync`

```csharp
Task<ModelizationSuggestionList> GetModelizationSuggestionListAsync(int page = 1, int limit = 100, CancellationToken ct = default)
```

Returns a page of Data Architect modelization suggestions.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).

Returns: A `ModelizationSuggestionList` with HAL navigation links.

## `GetModelizationSuggestionAsync`

```csharp
Task<ModelizationSuggestion> GetModelizationSuggestionAsync(string uuid, CancellationToken ct = default)
```

Returns a single Data Architect modelization suggestion by its UUID.

- `uuid` — The suggestion UUID.

Returns: The matching `ModelizationSuggestion`.

## `SuggestModelizationAttributeAsync`

```csharp
Task<string> SuggestModelizationAttributeAsync(string requestJson, CancellationToken ct = default)
```

Submits an attribute modelization suggestion to the Data Architect agent. The request body is passed as raw JSON (fields per the Akeneo API reference: `source`, `description`, `code`, `type`, `additional_comments`).

- `requestJson` — Raw JSON request body.

Returns: Raw JSON response body (the created suggestion).

## `ApproveModelizationSuggestionAsync`

```csharp
Task<string> ApproveModelizationSuggestionAsync(string uuid, string? overridesJson = null, CancellationToken ct = default)
```

Approves a modelization suggestion, optionally overriding attribute properties.

- `uuid` — The suggestion UUID.
- `overridesJson` — Optional raw JSON object with attribute-property overrides.

Returns: Raw JSON response body.

## `DeclineModelizationSuggestionAsync`

```csharp
Task DeclineModelizationSuggestionAsync(string uuid, CancellationToken ct = default)
```

Declines a modelization suggestion.

- `uuid` — The suggestion UUID.


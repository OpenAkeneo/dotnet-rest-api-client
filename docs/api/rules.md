# Rules — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Rules domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

**Domain notes:**
- Rules Engine (Enterprise). `RuleDefinition.Conditions`/`Actions`/`Triggers` are polymorphic
  (`object?`) — shape per the Akeneo rule format; the client does not validate them.
- **Caution:** an `Enabled` rule executes against real catalog data. When experimenting, set
  `Enabled = false` explicitly.
- `CreateOrReplaceRuleDefinitionAsync` uses PUT: it **replaces** the whole rule, unlike the
  PATCH-merge semantics everywhere else.

## `StreamRuleDefinitionsAsync`

```csharp
IAsyncEnumerable<RuleDefinition> StreamRuleDefinitionsAsync(CancellationToken ct = default)
```

Streams all rule definitions, following HAL pagination automatically.

## `GetRuleDefinitionListFullAsync`

```csharp
Task<List<RuleDefinition>> GetRuleDefinitionListFullAsync(CancellationToken ct = default)
```

Returns all rule definitions as a materialised list.

Returns: A list of all `RuleDefinition` objects.

## `GetRuleDefinitionListAsync`

```csharp
Task<RuleDefinitionList> GetRuleDefinitionListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a rule definition page using an arbitrary set of pre-built query parameters (the endpoint also supports `codes`, `type`, `execution_type` and `enabled` filters).

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `RuleDefinitionList` with HAL navigation links.

## `GetRuleDefinitionListAsync`

```csharp
Task<RuleDefinitionList> GetRuleDefinitionListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of rule definitions.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `withCount` — Include total count in the API response.

Returns: A paginated `RuleDefinitionList` with HAL navigation links.

## `GetRuleDefinitionAsync`

```csharp
Task<RuleDefinition> GetRuleDefinitionAsync(string code, CancellationToken ct = default)
```

Returns a single rule definition by its code.

- `code` — The rule code.

Returns: The matching `RuleDefinition`.

## `CreateOrReplaceRuleDefinitionAsync`

```csharp
Task<RuleDefinition> CreateOrReplaceRuleDefinitionAsync(RuleDefinition rule, CancellationToken ct = default)
```

Creates or replaces a rule definition via HTTP PUT then returns the refreshed entity.

- `rule` — The rule to create or replace. `Code` must be set.

Returns: The stored `RuleDefinition` as returned by the API.


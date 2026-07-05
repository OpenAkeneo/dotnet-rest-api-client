# Jobs — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Jobs domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

## `LaunchExportJobAsync`

```csharp
Task<JobExecutionResult> LaunchExportJobAsync(string code, bool isDryRun = false, CancellationToken ct = default)
```

Launches an export job by its code.

- `code` — The export job code.
- `isDryRun` — When `true`, the job runs in dry-run mode without committing changes.

Returns: A `JobExecutionResult` containing the new execution ID.

## `LaunchImportJobAsync`

```csharp
Task<JobExecutionResult> LaunchImportJobAsync(string code, bool isDryRun = false, string? importMode = null, CancellationToken ct = default)
```

Launches an import job by its code.

- `code` — The import job code.
- `isDryRun` — When `true`, the job runs in dry-run mode without committing changes.
- `importMode` — Optional import mode override (e.g. `"add_or_update"`).

Returns: A `JobExecutionResult` containing the new execution ID.


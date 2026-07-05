# Workflows — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Workflows domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

**Domain notes:**
- Enterprise feature — expect 401/403/404 on unlicensed tenants; degrade gracefully.
- Reads: workflows, step assignees, tasks. Writes: `StartWorkflowExecutionsAsync` (build items
  with `WorkflowExecutionRequest.ForProduct` / `ForProductModel`; ≤100 per call, auto-chunked)
  and the task actions `CompleteWorkflowTaskAsync` / `ApproveWorkflowTaskAsync` /
  `RejectWorkflowTaskAsync(taskUuid, sendBackToStepUuid, rejectedAttributesJson?)`.
- The workflows list endpoint may hang rather than error when the feature is off — pass a
  CancellationToken with a timeout when probing.

## `StreamWorkflowsAsync`

```csharp
IAsyncEnumerable<Workflow> StreamWorkflowsAsync(CancellationToken ct = default)
```

Streams all workflows, following HAL pagination automatically. Note: Workflows are an optional Akeneo Enterprise feature. The endpoint may not respond if the feature is disabled; apply a caller-side timeout when consuming this stream.

## `GetWorkflowListFullAsync`

```csharp
Task<List<Workflow>> GetWorkflowListFullAsync(CancellationToken ct = default)
```

Returns all workflows as a materialised list.

Returns: A list of all `Workflow` objects.

## `GetWorkflowListAsync`

```csharp
Task<WorkflowList> GetWorkflowListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a workflow page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `WorkflowList` with HAL navigation links.

## `GetWorkflowListAsync`

```csharp
Task<WorkflowList> GetWorkflowListAsync(int page = 1, int limit = 100, CancellationToken ct = default)
```

Returns a single page of workflows.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).

Returns: A paginated `WorkflowList` with HAL navigation links.

## `GetWorkflowAsync`

```csharp
Task<Workflow> GetWorkflowAsync(string uuid, CancellationToken ct = default)
```

Returns a single workflow by its UUID.

- `uuid` — The workflow UUID.

Returns: The matching `Workflow`.

## `StreamWorkflowStepAssigneesAsync`

```csharp
IAsyncEnumerable<WorkflowStepAssignee> StreamWorkflowStepAssigneesAsync(string stepUuid, CancellationToken ct = default)
```

Streams all assignees for a given workflow step, following HAL pagination automatically.

- `stepUuid` — The workflow step UUID.

## `GetWorkflowStepAssigneeListFullAsync`

```csharp
Task<List<WorkflowStepAssignee>> GetWorkflowStepAssigneeListFullAsync(string stepUuid, CancellationToken ct = default)
```

Returns all assignees for a workflow step as a materialised list.

- `stepUuid` — The workflow step UUID.

Returns: A list of all `WorkflowStepAssignee` objects.

## `GetWorkflowStepAssigneeListAsync`

```csharp
Task<WorkflowStepAssigneeList> GetWorkflowStepAssigneeListAsync(string stepUuid, Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a workflow step assignee page using an arbitrary set of pre-built query parameters.

- `stepUuid` — The workflow step UUID.
- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `WorkflowStepAssigneeList` with HAL navigation links.

## `GetWorkflowStepAssigneeListAsync`

```csharp
Task<WorkflowStepAssigneeList> GetWorkflowStepAssigneeListAsync(string stepUuid, int page = 1, int limit = 100, CancellationToken ct = default)
```

Returns a single page of assignees for a workflow step.

- `stepUuid` — The workflow step UUID.
- `page` — 1-based page number.
- `limit` — Items per page (1–100).

Returns: A paginated `WorkflowStepAssigneeList` with HAL navigation links.

## `StreamWorkflowTasksAsync`

```csharp
IAsyncEnumerable<WorkflowTask> StreamWorkflowTasksAsync(string? search = null, bool withAttributes = false, CancellationToken ct = default)
```

Streams all workflow tasks, following HAL pagination automatically.

- `search` — Optional JSON-encoded search filter.
- `withAttributes` — When `true`, includes attribute values in the response.

## `GetWorkflowTaskListFullAsync`

```csharp
Task<List<WorkflowTask>> GetWorkflowTaskListFullAsync(string? search = null, bool withAttributes = false, CancellationToken ct = default)
```

Returns all workflow tasks as a materialised list.

- `search` — Optional JSON-encoded search filter.
- `withAttributes` — When `true`, includes attribute values in the response.

Returns: A list of all `WorkflowTask` objects.

## `GetWorkflowTaskListAsync`

```csharp
Task<WorkflowTaskList> GetWorkflowTaskListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a workflow task page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `WorkflowTaskList` with HAL navigation links.

## `GetWorkflowTaskListAsync`

```csharp
Task<WorkflowTaskList> GetWorkflowTaskListAsync(int page = 1, int limit = 100, string? search = null, bool withAttributes = false, CancellationToken ct = default)
```

Returns a single page of workflow tasks.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `search` — Optional JSON-encoded search filter.
- `withAttributes` — When `true`, includes attribute values in the response.

Returns: A paginated `WorkflowTaskList` with HAL navigation links.

## `GetWorkflowTaskAsync`

```csharp
Task<WorkflowTask> GetWorkflowTaskAsync(string uuid, CancellationToken ct = default)
```

Returns a single workflow task by its UUID.

- `uuid` — The workflow task UUID.

Returns: The matching `WorkflowTask`.

## `StartWorkflowExecutionsAsync`

```csharp
Task<List<string>> StartWorkflowExecutionsAsync(IEnumerable<WorkflowExecutionRequest> requests, CancellationToken ct = default)
```

Starts workflow executions for products and/or product models (max 100 per call; larger inputs are chunked transparently). Build items with `String)` / `String)`.

- `requests` — The executions to start.

Returns: The raw JSON response bodies (one per chunk; 201 on full success, 207 on partial).

## `CompleteWorkflowTaskAsync`

```csharp
Task CompleteWorkflowTaskAsync(string taskUuid, CancellationToken ct = default)
```

Marks an enrichment task as completed.

- `taskUuid` — The workflow task UUID.

## `ApproveWorkflowTaskAsync`

```csharp
Task ApproveWorkflowTaskAsync(string taskUuid, CancellationToken ct = default)
```

Approves a review task.

- `taskUuid` — The workflow task UUID.

## `RejectWorkflowTaskAsync`

```csharp
Task RejectWorkflowTaskAsync(string taskUuid, string sendBackToStepUuid, string? rejectedAttributesJson = null, CancellationToken ct = default)
```

Rejects a review task, sending it back to an earlier workflow step.

- `taskUuid` — The workflow task UUID.
- `sendBackToStepUuid` — UUID of the step the task is returned to.
- `rejectedAttributesJson` — Optional JSON object detailing the rejected attributes (per-attribute comment/locale/scope — see the Akeneo API reference for the shape).


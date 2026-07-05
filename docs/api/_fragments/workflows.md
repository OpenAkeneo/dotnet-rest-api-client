**Domain notes:**
- Enterprise feature — expect 401/403/404 on unlicensed tenants; degrade gracefully.
- Reads: workflows, step assignees, tasks. Writes: `StartWorkflowExecutionsAsync` (build items
  with `WorkflowExecutionRequest.ForProduct` / `ForProductModel`; ≤100 per call, auto-chunked)
  and the task actions `CompleteWorkflowTaskAsync` / `ApproveWorkflowTaskAsync` /
  `RejectWorkflowTaskAsync(taskUuid, sendBackToStepUuid, rejectedAttributesJson?)`.
- The workflows list endpoint may hang rather than error when the feature is off — pass a
  CancellationToken with a timeout when probing.
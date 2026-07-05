**Domain notes:**
- One HTTP call per 100 items — larger inputs are chunked transparently and result `Line`
  numbers are renumbered to absolute positions across chunks.
- **A bulk call does not throw for per-item failures.** Inspect each `BulkItemResult`:
  `Succeeded` (`StatusCode` 201 created / 204 updated), `Key` (code/identifier/uuid),
  `Message` + `Errors` on 422 rejections.
- Referenced codes must exist before the batch runs (e.g. an attribute's `group`, a family's
  `attributes`) — a missing reference rejects that line with 422, the rest still apply.
- Products/models/families/attributes/options/groups/association-types/channels/categories use
  Akeneo's NDJSON batch format; reference-entity records and assets use JSON arrays. The client
  handles both — this only matters if you inspect traffic.
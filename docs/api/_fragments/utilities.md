**Domain notes:**
- UI-extensions and Data-Architect modelization endpoints are feature/plan-gated.
- Modelization request/response bodies are passed as raw JSON strings (frontier endpoints whose
  schemas are still loose).
- `GetExtensionListAsync` returns a bare array (not HAL-paginated).
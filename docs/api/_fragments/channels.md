**Domain notes:**
- This page covers channels, locales, currencies, and measurement families.
- Locales and currencies are **read-only** in the API. Channels are not deletable.
- Creating a channel requires existing `CategoryTree`, `Currencies`, and `Locales` codes —
  copy them from an existing channel when unsure.
- `CreateOrUpdateMeasurementFamiliesAsync` is inherently batch: it takes a list and returns the
  raw per-item status JSON (`[{"code":...,"status_code":201|204|4xx,...}]`) — parse it; it does
  not throw per item.
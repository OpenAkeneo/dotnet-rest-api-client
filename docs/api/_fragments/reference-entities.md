**Domain notes:**
- Hierarchy: reference **entity** → **attributes** → attribute **options** → **records**.
  Nothing in this domain is deletable via the API — plan codes accordingly.
- `ReferenceEntityAttribute.Type` literals (instance-confirmed): `text`, `text_area`, `number`,
  `yes_no`, `single_option`, `multiple_options`, `image`, `reference_entity_single_link`,
  `reference_entity_multiple_links`. `ValidationRule` literals (text types): `none`, `email`,
  `url`, `regexp`.
- Record value `Data` shapes: `text`/`text_area`/`single_option` → `string`; `number` →
  `long`/`double`; `yes_no` → `bool`; `multiple_options`/`reference_entity_multiple_links` →
  list of `string`; `image` → media-file code; `reference_entity_single_link` → record code.
- Server-managed on `ReferenceEntityRecord`: `ReferenceEntityCode`, `Created`, `Updated` —
  omit from writes.
- All listings paginate by `search_after` cursor only.
- The cross-entity endpoint (`GetAllReferenceEntityRecordListAsync` /
  `StreamAllReferenceEntityRecordsAsync`) is **search-index backed and eventually consistent**:
  records created moments ago may take seconds to appear. The per-entity records list is
  strongly consistent — prefer it for read-after-write.
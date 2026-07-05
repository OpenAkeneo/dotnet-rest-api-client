**Domain notes:**
- Categories are not deletable via the API. New categories need an existing `Parent` code.
- `UploadCategoryMediaFileAsync` requires `categoryJson`
  (`{"code":"...","attribute_code":"...","channel":...,"locale":...}`) and the enriched-categories
  feature (template with attributes) on the tenant.
- `withPosition`/`withEnrichedAttributes` enrichments are opt-in query flags on reads.
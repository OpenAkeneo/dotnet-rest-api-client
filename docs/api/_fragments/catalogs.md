**Domain notes:**
- "Catalogs for Apps" — requires an app token with the feature; classic connection tokens get
  403/404 on every endpoint here.
- Catalog product listings paginate by `search_after` only.
- `GetCatalogMapped*ListAsync` return **raw JSON strings** (the shape depends on the catalog's
  mapping schema, so no typed model exists).
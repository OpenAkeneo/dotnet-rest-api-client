# Bulk — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Bulk domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.2 surface — do not edit by hand.

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

## `BulkCreateOrUpdateProductUuidsAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateProductUuidsAsync(IEnumerable<ProductUuid> products, CancellationToken ct = default)
```

Creates or updates several UUID-keyed products in one call per 100 items.

- `products` — The products to upsert.

Returns: One `BulkItemResult` per submitted product, in order.

## `BulkCreateOrUpdateProductIdentifiersAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateProductIdentifiersAsync(IEnumerable<ProductIdentifier> products, CancellationToken ct = default)
```

Creates or updates several identifier-keyed products in one call per 100 items.

- `products` — The products to upsert.

Returns: One `BulkItemResult` per submitted product, in order.

## `BulkCreateOrUpdateProductModelsAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateProductModelsAsync(IEnumerable<ProductModel> productModels, CancellationToken ct = default)
```

Creates or updates several product models in one call per 100 items.

- `productModels` — The product models to upsert.

Returns: One `BulkItemResult` per submitted model, in order.

## `BulkCreateOrUpdateFamiliesAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateFamiliesAsync(IEnumerable<Family> families, CancellationToken ct = default)
```

Creates or updates several families in one call per 100 items.

- `families` — The families to upsert.

Returns: One `BulkItemResult` per submitted family, in order.

## `BulkCreateOrUpdateFamilyVariantsAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateFamilyVariantsAsync(string familyCode, IEnumerable<FamilyVariant> variants, CancellationToken ct = default)
```

Creates or updates several variants of one family in one call per 100 items.

- `familyCode` — The parent family code.
- `variants` — The family variants to upsert.

Returns: One `BulkItemResult` per submitted variant, in order.

## `BulkCreateOrUpdateAttributesAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateAttributesAsync(IEnumerable<AkeneoAttribute> attributes, CancellationToken ct = default)
```

Creates or updates several attributes in one call per 100 items.

- `attributes` — The attributes to upsert.

Returns: One `BulkItemResult` per submitted attribute, in order.

## `BulkCreateOrUpdateAttributeOptionsAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateAttributeOptionsAsync(string attributeCode, IEnumerable<AttributeOption> options, CancellationToken ct = default)
```

Creates or updates several options of one attribute in one call per 100 items.

- `attributeCode` — The parent attribute code.
- `options` — The options to upsert.

Returns: One `BulkItemResult` per submitted option, in order.

## `BulkCreateOrUpdateAttributeGroupsAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateAttributeGroupsAsync(IEnumerable<AttributeGroup> attributeGroups, CancellationToken ct = default)
```

Creates or updates several attribute groups in one call per 100 items.

- `attributeGroups` — The attribute groups to upsert.

Returns: One `BulkItemResult` per submitted group, in order.

## `BulkCreateOrUpdateAssociationTypesAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateAssociationTypesAsync(IEnumerable<AssociationType> associationTypes, CancellationToken ct = default)
```

Creates or updates several association types in one call per 100 items.

- `associationTypes` — The association types to upsert.

Returns: One `BulkItemResult` per submitted association type, in order.

## `BulkCreateOrUpdateChannelsAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateChannelsAsync(IEnumerable<Channel> channels, CancellationToken ct = default)
```

Creates or updates several channels in one call per 100 items.

- `channels` — The channels to upsert.

Returns: One `BulkItemResult` per submitted channel, in order.

## `BulkCreateOrUpdateCategoriesAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateCategoriesAsync(IEnumerable<Category> categories, CancellationToken ct = default)
```

Creates or updates several categories in one call per 100 items.

- `categories` — The categories to upsert.

Returns: One `BulkItemResult` per submitted category, in order.

## `BulkCreateOrUpdateReferenceEntityRecordsAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateReferenceEntityRecordsAsync(string referenceEntityCode, IEnumerable<ReferenceEntityRecord> records, CancellationToken ct = default)
```

Creates or updates several records of one reference entity in one call per 100 items.

- `referenceEntityCode` — The reference entity code.
- `records` — The records to upsert.

Returns: One `BulkItemResult` per submitted record, in order.

## `BulkCreateOrUpdateAssetsAsync`

```csharp
Task<List<BulkItemResult>> BulkCreateOrUpdateAssetsAsync(string assetFamilyCode, IEnumerable<Asset> assets, CancellationToken ct = default)
```

Creates or updates several assets of one asset family in one call per 100 items.

- `assetFamilyCode` — The asset family code.
- `assets` — The assets to upsert.

Returns: One `BulkItemResult` per submitted asset, in order.


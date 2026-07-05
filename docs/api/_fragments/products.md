**Domain notes:**
- Two product keys exist: **UUID** (`ProductUuid`, modern) and **identifier/SKU**
  (`ProductIdentifier`, legacy). Same shape otherwise; pick one consistently.
- Streamers use `search_after` internally (no 10k ceiling). Manual `page`/`limit` paging is
  capped at 10 000 items by Akeneo.
- `Enabled` is `bool?`: `null` = server default (**enabled**). See llms.txt pattern 5.
- `CreateProductUuidAsync` with `Uuid = null` lets the server generate the UUID (resolved from
  the 201 Location header). `CreateOrUpdate*` requires the key to be set.
- `UploadProductMediaFileAsync` requires `productJson` or `productModelJson` (exactly one) —
  the API rejects uploads without a link target. Returns the media-file code.
- Drafts/proposals (`Get*DraftAsync`, `Submit*ProposalAsync`) need the Enterprise workflow
  feature; expect 4xx on unlicensed tenants.
- `AkeneoAttribute.Type` literals (instance-confirmed): `pim_catalog_text`, `pim_catalog_textarea`,
  `pim_catalog_identifier`, `pim_catalog_number`, `pim_catalog_boolean`, `pim_catalog_date`,
  `pim_catalog_simpleselect`, `pim_catalog_multiselect`, `pim_catalog_price_collection`,
  `pim_catalog_metric`, `pim_catalog_file`, `pim_catalog_image`, `pim_catalog_asset_collection`,
  `pim_catalog_table`, `pim_reference_data_simpleselect`, `pim_reference_data_multiselect`.
- Server-managed (returned on reads, omit from writes): `Created`, `Updated`, `QualityScores`,
  `Completenesses` on products; `AttributeType` and `LinkedData` on `ProductValue`.
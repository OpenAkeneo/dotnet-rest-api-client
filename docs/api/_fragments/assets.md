**Domain notes:**
- Hierarchy: asset **family** → asset **attributes** → attribute **options** → **assets**.
  Families and attributes are not deletable via the API; assets are.
- `AssetAttribute.Type` literals (instance-confirmed): `text`, `media_file`, `number`,
  `boolean`, `single_option`, `multiple_options`. `MediaType` literals (for `media_file`):
  `image`, `pdf`, `youtube`, `vimeo`, `other`.
- `AssetValue.Data` shapes by attribute type: `text`/`single_option` → `string`;
  `media_file` → `string` media-file code (plus `Links` download hrefs and `LinkedData` file
  metadata — both server-managed, never write them); `number` → `long`/`double`;
  `boolean` → `bool`; `multiple_options` → list of `string`.
- Server-managed on `Asset`: `Created`, `Updated` — returned on reads, omit from writes.
- Asset lists paginate by `search_after` cursor only (no page numbers).
- `UploadAssetMediaFileAsync` needs no link target; attach the returned code to an asset value
  afterwards via `CreateOrUpdateAssetAsync`.
- The cross-family media-file code from an upload round-trips byte-for-byte through
  `DownloadAssetMediaFileAsync`/`...StreamAsync`.
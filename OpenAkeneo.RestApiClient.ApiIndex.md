# OpenAkeneo .NET Client — API Index

Lookup index for `OpenAkeneo.RestApiClient.ApiReference.md`.
Each entry shows the .NET method or section and its line range in the Reference file.
Use this to find what you need, then read those lines from the Reference for full details.

**Companion files:**
- **Reference**: `OpenAkeneo.RestApiClient.ApiReference.md` — full method signatures, parameters, return types, and schemas.
- **README**: `README.md` — installation, DI configuration, usage examples, and attribute value patterns.

---

## Common Patterns & Conventions

All methods live on `AkeneoContext` (high-level client). Key conventions:

- **Async-only**: Every method is `async` and returns `Task<T>` or `IAsyncEnumerable<T>`.
- **CancellationToken**: All methods accept an optional `CancellationToken ct` parameter (omitted in listings below).
- **Pagination triple**: Most resources have `Stream…Async` (lazy all-pages), `Get…ListFullAsync` (materialised list), `Get…ListAsync` (single page).
- **Upsert**: `CreateOrUpdate…Async` uses PATCH — creates on first call, updates on subsequent calls.
- **Error handling**: All methods throw `AkeneoApiException` with `StatusCode`, `ApiMessage`, `RequestUrl`, `ResponseBody` properties.
- **Attribute values**: Product/model `Values` are `Dictionary<string, List<ProductValue>>` where each value has `Locale`, `Scope`, `Data`.
- **Dictionary overloads**: Most list methods have an overload accepting `Dictionary<string, string>` for custom query parameters.

See Reference Lines 1–70 for the full Common Patterns section including attribute value type mappings.

---

### Common Patterns
Lines 12–69

- Pagination: L14–27
- Error Handling: L29–43
- Create-or-Update (Upsert): L45–46
- Dictionary Overloads: L48–49
- Attribute Value Types: L51–69

### Product UUID
Lines 71–150

- `StreamProductUuidsAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`: L75–82
- `GetProductUuidListFullAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`: L84–91
- `GetProductUuidListAsync(int page, int limit, string? search, string? scope, string? locales, bool withAssetShareLinks)`: L93–102
- `GetProductUuidListAsync(Dictionary<string, string> queryParameters)`: L104–108
- `GetProductUuidAsync(string uuid)`: L110–114
- `GetProductUuidDraftAsync(string uuid)`: L116–120
- `CreateOrUpdateProductUuidAsync(ProductUuid product)`: L122–126
- `CreateProductUuidAsync(ProductUuid product)`: L128–132
- `DeleteProductUuidAsync(string uuid)`: L134–138
- `SubmitProductUuidProposalAsync(string uuid)`: L140–144
- `SearchProductUuidsAsync(string searchBody)`: L146–150

### Product Identifier
Lines 152–225

- `StreamProductIdentifiersAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`: L156–163
- `GetProductIdentifierListFullAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`: L165–172
- `GetProductIdentifierListAsync(int page, int limit, string? search, string? scope, string? locales, bool withAssetShareLinks)`: L174–183
- `GetProductIdentifierListAsync(Dictionary<string, string> queryParameters)`: L185–189
- `GetProductIdentifierAsync(string identifier)`: L191–195
- `GetProductIdentifierDraftAsync(string identifier)`: L197–201
- `CreateOrUpdateProductIdentifierAsync(ProductIdentifier product)`: L203–207
- `CreateProductIdentifierAsync(ProductIdentifier product)`: L209–213
- `DeleteProductIdentifierAsync(string identifier)`: L215–219
- `SubmitProductIdentifierProposalAsync(string identifier)`: L221–225

### Product Model
Lines 227–294

- `StreamProductModelsAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`: L231–238
- `GetProductModelListFullAsync(string? search, string? scope, string? locales, bool withAssetShareLinks)`: L240–247
- `GetProductModelListAsync(int page, int limit, string? search, string? scope, string? locales, bool withAssetShareLinks)`: L249–258
- `GetProductModelAsync(string code)`: L260–264
- `GetProductModelDraftAsync(string code)`: L266–270
- `CreateOrUpdateProductModelAsync(ProductModel productModel)`: L272–276
- `CreateProductModelAsync(ProductModel productModel)`: L278–282
- `DeleteProductModelAsync(string code)`: L284–288
- `SubmitProductModelProposalAsync(string code)`: L290–294

### Product Media File
Lines 296–335

- `StreamProductMediaFilesAsync()`: L300–302
- `GetProductMediaFileListFullAsync()`: L304–306
- `GetProductMediaFileListAsync(int page, int limit, bool withCount)`: L308–314
- `GetProductMediaFileAsync(string code)`: L316–320
- `DownloadProductMediaFileAsync(string code)`: L322–326
- `UploadProductMediaFileAsync(byte[] fileBytes, string fileName, string contentType, string? productJson)`: L328–335

### Attribute
Lines 337–383

- `StreamAttributesAsync(string? search, bool withCount, bool withTableSelectOptions)`: L341–347
- `GetAttributeListFullAsync(string? search, bool withCount, bool withTableSelectOptions)`: L349–355
- `GetAttributeListAsync(int page, int limit, string? search, bool withCount, bool withTableSelectOptions)`: L357–365
- `GetAttributeAsync(string attributeCode)`: L367–371
- `CreateOrUpdateAttributeAsync(AkeneoAttribute attribute)`: L373–377
- `CreateAttributeAsync(AkeneoAttribute attribute)`: L379–383

### Attribute Option
Lines 385–438

- `StreamAttributeOptionsAsync(string attributeCode, bool withCount)`: L389–394
- `GetAttributeOptionListFullAsync(string attributeCode, bool withCount)`: L396–401
- `GetAttributeOptionListAsync(string attributeCode, int page, int limit, bool withCount)`: L403–410
- `GetAttributeOptionListAsync(string attributeCode, Dictionary<string, string> queryParameters)`: L412–417
- `GetAttributeOptionAsync(string attributeCode, string attributeOptionCode)`: L419–424
- `CreateOrUpdateAttributeOptionAsync(string attributeCode, AttributeOption attributeOption)`: L426–431
- `CreateAttributeOptionAsync(string attributeCode, AttributeOption attributeOption)`: L433–438

### Attribute Group
Lines 440–483

- `StreamAttributeGroupsAsync(string? search, bool withCount)`: L444–449
- `GetAttributeGroupListFullAsync(string? search, bool withCount)`: L451–456
- `GetAttributeGroupListAsync(int page, int limit, string? search, bool withCount)`: L458–465
- `GetAttributeGroupAsync(string attributeCode)`: L467–471
- `CreateOrUpdateAttributeGroupAsync(AttributeGroup attributeGroup)`: L473–477
- `CreateAttributeGroupAsync(AttributeGroup attributeGroup)`: L479–483

### Family
Lines 485–521

- `GetFamilyListFullAsync(string? search, bool withCount)`: L489–494
- `GetFamilyListAsync(int page, int limit, string? search, bool withCount)`: L496–503
- `GetFamilyAsync(string familyCode)`: L505–509
- `CreateOrUpdateFamilyAsync(Family family)`: L511–515
- `CreateFamilyAsync(Family family)`: L517–521

### Family Variant
Lines 523–576

- `StreamFamilyVariantsAsync(string familyCode, bool withCount)`: L527–532
- `GetFamilyVariantListFullAsync(string familyCode, bool withCount)`: L534–539
- `GetFamilyVariantListAsync(string familyCode, int page, int limit, bool withCount)`: L541–548
- `GetFamilyVariantListAsync(string familyCode, Dictionary<string, string> queryParameters)`: L550–555
- `GetFamilyVariantAsync(string familyCode, string code)`: L557–562
- `CreateOrUpdateFamilyVariantAsync(string familyCode, FamilyVariant variant)`: L564–569
- `CreateFamilyVariantAsync(string familyCode, FamilyVariant variant)`: L571–576

### Category
Lines 578–650

- `StreamCategoriesAsync(string? search, bool withCount, bool withPosition, bool withEnrichedAttributes)`: L582–589
- `GetCategoryListFullAsync(string? search, bool withCount, bool withPosition, bool withEnrichedAttributes)`: L591–598
- `GetCategoryListAsync(int page, int limit, string? search, bool withCount, bool withPosition, bool withEnrichedAttributes)`: L600–609
- `GetCategoryAsync(string code, bool withPosition, bool withEnrichedAttributes)`: L611–617
- `GetCategoryAsync(string code, Dictionary<string, string> queryParameters)`: L619–624
- `DownloadCategoryMediaFileAsync(string filePath)`: L626–630
- `CreateOrUpdateCategoryAsync(Category category)`: L632–636
- `CreateCategoryAsync(Category category)`: L638–642
- `UploadCategoryMediaFileAsync(byte[] fileBytes, string fileName, string contentType)`: L644–650

### Channel
Lines 652–692

- `StreamChannelsAsync(bool withCount)`: L656–660
- `GetChannelListFullAsync(bool withCount)`: L662–666
- `GetChannelListAsync(int page, int limit, bool withCount)`: L668–674
- `GetChannelAsync(string channelCode)`: L676–680
- `CreateOrUpdateChannelAsync(Channel channel)`: L682–686
- `CreateChannelAsync(Channel channel)`: L688–692

### Locale
Lines 694–722

- `StreamLocalesAsync(bool withCount)`: L698–702
- `GetLocaleListFullAsync(bool withCount)`: L704–708
- `GetLocaleListAsync(int page, int limit, bool withCount)`: L710–716
- `GetLocaleAsync(string localeCode)`: L718–722

### Currency
Lines 724–752

- `StreamCurrenciesAsync(bool withCount)`: L728–732
- `GetCurrencyListFullAsync(bool withCount)`: L734–738
- `GetCurrencyListAsync(int page, int limit, bool withCount)`: L740–746
- `GetCurrencyAsync(string currencyCode)`: L748–752

### Measurement Family
Lines 754–766

- `GetMeasurementFamilyListAsync()`: L758–760
- `CreateOrUpdateMeasurementFamiliesAsync(List<MeasurementFamily> measurementFamilies)`: L762–766

### Association Type
Lines 768–808

- `StreamAssociationTypesAsync(bool withCount)`: L772–776
- `GetAssociationTypeListFullAsync(bool withCount)`: L778–782
- `GetAssociationTypeListAsync(int page, int limit, bool withCount)`: L784–790
- `GetAssociationTypeAsync(string attributeCode)`: L792–796
- `CreateOrUpdateAssociationTypeAsync(AssociationType associationType)`: L798–802
- `CreateAssociationTypeAsync(AssociationType associationType)`: L804–808

### Reference Entity
Lines 810–834

- `GetReferenceEntityListFullAsync()`: L814–816
- `GetReferenceEntityListAsync(string? searchAfter)`: L818–822
- `GetReferenceEntityAsync(string referenceEntityCode)`: L824–828
- `CreateOrUpdateReferenceEntityAsync(ReferenceEntity referenceEntity)`: L830–834

### Reference Entity Attribute
Lines 836–858

- `GetReferenceEntityAttributeListAsync(string referenceEntityCode)`: L840–844
- `GetReferenceEntityAttributeAsync(string referenceEntityCode, string attributeCode)`: L846–851
- `CreateOrUpdateReferenceEntityAttributeAsync(string referenceEntityCode, ReferenceEntityAttribute attribute)`: L853–858

### Reference Entity Attribute Option
Lines 860–885

- `GetReferenceEntityAttributeOptionListAsync(string referenceEntityCode, string attributeCode)`: L864–869
- `GetReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, string optionCode)`: L871–877
- `CreateOrUpdateReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, ReferenceEntityAttributeOption option)`: L879–885

### Reference Entity Record
Lines 887–940

- `StreamReferenceEntityRecordsAsync(string referenceEntityCode, string? search, string? channel, string? locales, string? searchAfter)`: L891–899
- `GetReferenceEntityRecordListFullAsync(string referenceEntityCode, string? search, string? channel, string? locales, string? searchAfter)`: L901–909
- `GetReferenceEntityRecordListAsync(string referenceEntityCode, string? search, string? channel, string? locales, string? searchAfter)`: L911–919
- `GetReferenceEntityRecordListAsync(string referenceEntityCode, Dictionary<string, string> queryParameters)`: L921–926
- `GetReferenceEntityRecordAsync(string referenceEntityCode, string recordCode)`: L928–933
- `CreateOrUpdateReferenceEntityRecordAsync(string referenceEntityCode, ReferenceEntityRecord record)`: L935–940

### Reference Entity Media File
Lines 942–957

- `DownloadReferenceEntityMediaFileAsync(string mediaFileCode)`: L945–949
- `UploadReferenceEntityMediaFileAsync(byte[] fileBytes, string fileName, string contentType)`: L951–957

### Asset Family
Lines 959–983

- `GetAssetFamilyListFullAsync()`: L963–965
- `GetAssetFamilyListAsync(string? searchAfter)`: L967–971
- `GetAssetFamilyAsync(string assetFamilyCode)`: L973–977
- `CreateOrUpdateAssetFamilyAsync(AssetFamily assetFamily)`: L979–983

### Asset Attribute
Lines 985–1007

- `GetAssetAttributeListAsync(string assetFamilyCode)`: L989–993
- `GetAssetAttributeAsync(string assetFamilyCode, string assetAttributeCode)`: L995–1000
- `CreateOrUpdateAssetAttributeAsync(string assetFamilyCode, AssetAttribute assetAttribute)`: L1002–1007

### Asset Attribute Option
Lines 1009–1034

- `GetAssetAttributeOptionListAsync(string assetFamilyCode, string assetAttributeCode)`: L1013–1018
- `GetAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, string optionCode)`: L1020–1026
- `CreateOrUpdateAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, AssetAttributeOption option)`: L1028–1034

### Asset Media File
Lines 1036–1051

- `DownloadAssetMediaFileAsync(string mediaFileCode)`: L1039–1043
- `UploadAssetMediaFileAsync(byte[] fileBytes, string fileName, string contentType)`: L1045–1051

### Asset
Lines 1053–1101

- `StreamAssetsAsync(string assetFamilyCode, string? search, string? searchAfter)`: L1057–1063
- `GetAssetListFullAsync(string assetFamilyCode, string? search, string? searchAfter)`: L1065–1071
- `GetAssetListAsync(string assetFamilyCode, int limit, string? search, string? searchAfter)`: L1073–1080
- `GetAssetListAsync(string assetFamilyCode, Dictionary<string, string> queryParameters)`: L1082–1087
- `GetAssetAsync(string assetFamilyCode, string code)`: L1089–1094
- `CreateOrUpdateAssetAsync(string assetFamilyCode, Asset asset)`: L1096–1101

### Catalog
Lines 1103–1242

- `GetCatalogListAsync(int page, int limit)`: L1107–1112
- `StreamCatalogsAsync()`: L1114–1116
- `GetCatalogListFullAsync()`: L1118–1120
- `GetCatalogAsync(string id)`: L1122–1126
- `GetCatalogProductUuidListAsync(string catalogId, int limit, string? searchAfter)`: L1128–1134
- `GetCatalogProductListAsync(string catalogId, int limit, string? searchAfter)`: L1136–1142
- `StreamCatalogProductUuidsAsync(string catalogId)`: L1144–1148
- `GetCatalogProductUuidListFullAsync(string catalogId)`: L1150–1154
- `StreamCatalogProductsAsync(string catalogId)`: L1156–1160
- `GetCatalogProductListFullAsync(string catalogId)`: L1162–1166
- `GetCatalogProductAsync(string catalogId, string uuid)`: L1168–1173
- `GetCatalogMappedProductListAsync(string catalogId, int limit, string? searchAfter)`: L1175–1181
- `GetCatalogMappedModelListAsync(string catalogId, int limit, string? searchAfter)`: L1183–1189
- `GetCatalogMappedVariantListAsync(string catalogId, string modelCode, int limit, string? searchAfter)`: L1191–1198
- `GetCatalogMappingSchemaAsync(string catalogId)`: L1200–1204
- `CreateCatalogAsync(Catalog catalog)`: L1206–1210
- `UpdateCatalogAsync(string catalogId, Catalog catalog)`: L1212–1217
- `DeleteCatalogAsync(string catalogId)`: L1219–1223
- `DuplicateCatalogAsync(string catalogId)`: L1225–1229
- `SetCatalogMappingSchemaAsync(string catalogId, string schemaJson)`: L1231–1236
- `DeleteCatalogMappingSchemaAsync(string catalogId)`: L1238–1242

### Job
Lines 1244–1261

- `LaunchExportJobAsync(string code, bool isDryRun)`: L1248–1253
- `LaunchImportJobAsync(string code, bool isDryRun, string? importMode)`: L1255–1261

### Workflow
Lines 1263–1286

- `StreamWorkflowsAsync()`: L1267–1269
- `GetWorkflowListFullAsync()`: L1271–1273
- `GetWorkflowListAsync(int page, int limit)`: L1275–1280
- `GetWorkflowAsync(string uuid)`: L1282–1286

### Workflow Step Assignee
Lines 1288–1317

- `StreamWorkflowStepAssigneesAsync(string stepUuid)`: L1292–1296
- `GetWorkflowStepAssigneeListFullAsync(string stepUuid)`: L1298–1302
- `GetWorkflowStepAssigneeListAsync(string stepUuid, int page, int limit)`: L1304–1310
- `GetWorkflowStepAssigneeListAsync(string stepUuid, Dictionary<string, string> queryParameters)`: L1312–1317

### Workflow Task
Lines 1319–1350

- `StreamWorkflowTasksAsync(string? search, bool withAttributes)`: L1323–1328
- `GetWorkflowTaskListFullAsync(string? search, bool withAttributes)`: L1330–1335
- `GetWorkflowTaskListAsync(int page, int limit, string? search, bool withAttributes)`: L1337–1344
- `GetWorkflowTaskAsync(string uuid)`: L1346–1350

### System & Permissions
Lines 1352–1392

- `GetSystemInformationAsync()`: L1355–1357
- `GetUserChannelsPermissionsAsync(string userUuid)`: L1359–1363
- `GetUserLocalesPermissionsAsync(string userUuid)`: L1365–1369
- `GetApiOverviewAsync()`: L1371–1373
- `GetExtensionListAsync()`: L1375–1377
- `GetModelizationSuggestionListAsync(int page, int limit)`: L1379–1384
- `GetModelizationSuggestionAsync(string uuid)`: L1386–1392

### Key Schemas
Lines 1394–1525

- ProductUuid: L1399–1415
- ProductIdentifier: L1417–1425
- ProductModel: L1427–1435
- AkeneoAttribute: L1437–1446
- AttributeOption: L1448–1454
- Family: L1456–1464
- Category: L1466–1472
- Channel: L1474–1481
- ReferenceEntity: L1483–1488
- ReferenceEntityRecord: L1490–1494
- AssetFamily: L1496–1501
- Asset: L1503–1508
- Catalog: L1510–1515
- ProductMediaFile: L1517–1525

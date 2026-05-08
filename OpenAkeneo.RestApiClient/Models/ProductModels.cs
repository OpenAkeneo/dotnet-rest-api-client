using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Product base (shared)

    /// <summary>Shared fields common to both UUID-keyed and identifier-keyed Akeneo products.</summary>
    public abstract class ProductBase : HalItemInheritance
    {

        /// <summary>Whether the product is enabled (visible in the storefront).</summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>Code of the family this product belongs to.</summary>
        [JsonPropertyName("family")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Family { get; set; }

        /// <summary>Category codes the product is classified under.</summary>
        [JsonPropertyName("categories")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Categories { get; set; }

        /// <summary>Group codes the product belongs to.</summary>
        [JsonPropertyName("groups")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Groups { get; set; }

        /// <summary>Code of the parent product model, if this product is a variant.</summary>
        [JsonPropertyName("parent")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Parent { get; set; }

        /// <summary>Attribute values keyed by attribute code; each entry is a list of locale/scope-specific values.</summary>
        [JsonPropertyName("values")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, List<ProductValue>>? Values { get; set; }

        // Server-managed — omit from write payloads
        /// <summary>ISO 8601 timestamp of when the product was created. Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("created")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Created { get; set; }

        /// <summary>ISO 8601 timestamp of the last update. Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("updated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Updated { get; set; }

        /// <summary>Product associations grouped by association type code.</summary>
        [JsonPropertyName("associations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Associations { get; set; }

        /// <summary>Quantified product associations (with a quantity value) grouped by association type code.</summary>
        [JsonPropertyName("quantified_associations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? QuantifiedAssociations { get; set; }

        // Server-managed read-only — never send in write payloads
        /// <summary>Quality scores per scope and locale. Server-managed read-only; never send in write payloads.</summary>
        [JsonPropertyName("quality_scores")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ProductQualityScore>? QualityScores { get; set; }

        /// <summary>Completeness percentages per scope and locale. Server-managed read-only; never send in write payloads.</summary>
        [JsonPropertyName("completenesses")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ProductCompleteness>? Completenesses { get; set; }

    }

    #endregion


    #region Product UUID

    /// <summary>Paginated list of UUID-keyed products.</summary>
    public class ProductUuidList : HalBaseInheritance
    {
        /// <summary>Products on the current page.</summary>
        public List<ProductUuid> Products { get; set; } = new();
    }

    /// <summary>An Akeneo product identified by its UUID.</summary>
    public class ProductUuid : ProductBase
    {

        /// <summary>Unique identifier (UUID v4) of the product.</summary>
        [JsonPropertyName("uuid")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Uuid { get; set; }

    }

    #endregion


    #region Product identifier

    /// <summary>Paginated list of identifier-keyed products.</summary>
    public class ProductIdentifierList : HalBaseInheritance
    {
        /// <summary>Products on the current page.</summary>
        public List<ProductIdentifier> Products { get; set; } = new();
    }

    /// <summary>An Akeneo product identified by its SKU/identifier string.</summary>
    public class ProductIdentifier : ProductBase
    {

        /// <summary>Unique SKU / product identifier string.</summary>
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } = default!;

    }

    #endregion


    #region Product model

    /// <summary>Paginated list of product models.</summary>
    public class ProductModelList : HalBaseInheritance
    {
        /// <summary>Product models on the current page.</summary>
        public List<ProductModel> ProductModels { get; set; } = new();
    }

    /// <summary>An Akeneo product model, which groups variant products sharing common attribute values.</summary>
    public class ProductModel : HalItemInheritance
    {

        /// <summary>Unique code identifying the product model.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Code of the family this product model belongs to.</summary>
        [JsonPropertyName("family")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Family { get; set; }

        /// <summary>Code of the family variant that defines the variation axes.</summary>
        [JsonPropertyName("family_variant")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FamilyVariant { get; set; }

        /// <summary>Category codes the product model is classified under.</summary>
        [JsonPropertyName("categories")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Categories { get; set; }

        /// <summary>Code of the parent product model, for sub-product models.</summary>
        [JsonPropertyName("parent")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Parent { get; set; }

        /// <summary>Attribute values keyed by attribute code; each entry is a list of locale/scope-specific values.</summary>
        [JsonPropertyName("values")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, List<ProductValue>>? Values { get; set; }

        // Server-managed — omit from write payloads
        /// <summary>ISO 8601 timestamp of when the product model was created. Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("created")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Created { get; set; }

        /// <summary>ISO 8601 timestamp of the last update. Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("updated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Updated { get; set; }

        /// <summary>Product associations grouped by association type code.</summary>
        [JsonPropertyName("associations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Associations { get; set; }

        /// <summary>Quantified product associations grouped by association type code.</summary>
        [JsonPropertyName("quantified_associations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? QuantifiedAssociations { get; set; }

        // Server-managed read-only — never send in write payloads
        /// <summary>Quality scores per scope and locale. Server-managed read-only; never send in write payloads.</summary>
        [JsonPropertyName("quality_scores")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ProductQualityScore>? QualityScores { get; set; }

    }

    #endregion


    #region Product media file

    /// <summary>Paginated list of product media files.</summary>
    public class ProductMediaFileList : HalBaseInheritance
    {
        /// <summary>Media files on the current page.</summary>
        public List<ProductMediaFile> MediaFiles { get; set; } = new();
    }

    /// <summary>A media file (image, PDF, etc.) attached to a product attribute.</summary>
    public class ProductMediaFile : HalItemInheritance
    {

        /// <summary>Unique code / storage key of the media file.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Original filename as uploaded by the user.</summary>
        [JsonPropertyName("original_filename")]
        public string OriginalFilename { get; set; } = default!;

        /// <summary>MIME type of the file (e.g. <c>image/jpeg</c>).</summary>
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = default!;

        /// <summary>File size in bytes.</summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>File extension without the leading dot (e.g. <c>jpg</c>).</summary>
        [JsonPropertyName("extension")]
        public string Extension { get; set; } = default!;

    }

    #endregion


    #region Asset collection share links (linked_data on asset_collection product values)

    /// <summary>
    /// Typed linked_data for a <c>pim_catalog_asset_collection</c> product value.
    /// Keys are asset codes; values contain the CDN share links for that asset.
    /// Deserialize from <see cref="ProductValue.GetLinkedData{T}"/> using <c>Dictionary&lt;string, AssetCollectionLinkedDataEntry&gt;</c>.
    /// </summary>
    public class AssetCollectionLinkedDataEntry
    {
        /// <summary>Share links for each attribute/scope/locale combination of this asset.</summary>
        [JsonPropertyName("share_links")]
        public List<AssetCollectionShareLink>? ShareLinks { get; set; }
    }

    /// <summary>A single share link for one attribute/scope/locale combination of an asset.</summary>
    public class AssetCollectionShareLink
    {

        /// <summary>Asset attribute code this link belongs to (e.g. the media_file attribute on the asset family).</summary>
        [JsonPropertyName("attribute")]
        public string? Attribute { get; set; }

        /// <summary>Channel scope this share link applies to, or null for non-scopable.</summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        /// <summary>Locale this share link applies to, or null for non-localisable.</summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        /// <summary>Contains <c>self.href</c> — the public CDN URL for the asset file.</summary>
        [JsonPropertyName("_links")]
        public AssetCollectionShareLinkHrefs? Links { get; set; }

    }

    /// <summary>HAL links on an asset collection share link entry.</summary>
    public class AssetCollectionShareLinkHrefs
    {
        /// <summary>The public CDN URL for this asset file.</summary>
        [JsonPropertyName("self")]
        public AssetCollectionHref? Self { get; set; }
    }

    /// <summary>A HAL href wrapper.</summary>
    public class AssetCollectionHref
    {
        /// <summary>The URL.</summary>
        [JsonPropertyName("href")]
        public string? Href { get; set; }
    }

    #endregion


    #region Shared product sub-models

    /// <summary>A single scoped/localised attribute value on a product or product model.</summary>
    public class ProductValue
    {

        /// <summary>Locale code this value applies to, or <c>null</c> for non-localisable attributes.</summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        /// <summary>Channel scope this value applies to, or <c>null</c> for non-scopable attributes.</summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        /// <summary>The raw attribute value; shape depends on the attribute type.</summary>
        [JsonPropertyName("data")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Data { get; set; }

        // Read-only enrichment fields — omit from write payloads
        /// <summary>Attribute type identifier returned by the API for enrichment; omit from write payloads.</summary>
        [JsonPropertyName("attribute_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AttributeType { get; set; }

        /// <summary>Additional linked data (e.g. option labels) returned by the API; omit from write payloads.</summary>
        [JsonPropertyName("linked_data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? LinkedData { get; set; }

        /// <summary>Deserializes <see cref="Data"/> into the specified type.</summary>
        public T? GetData<T>()
        {
            if (Data == null) return default;
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(Data));
        }

        /// <summary>Deserializes <see cref="LinkedData"/> into the specified type.</summary>
        public T? GetLinkedData<T>()
        {
            if (LinkedData == null) return default;
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(LinkedData));
        }

        /// <summary>
        /// Returns <see cref="Data"/> as a plain string, covering the types the
        /// <see cref="Converters.PolymorphicDataConverter"/> can produce:
        /// <list type="bullet">
        ///   <item><c>string</c> — returned as-is.</item>
        ///   <item><c>long</c>, <c>double</c>, <c>decimal</c> — converted with <c>ToString()</c>.</item>
        ///   <item><c>List&lt;object?&gt;</c> — first non-null element is stringified recursively
        ///         (covers reference-entity and simple-select collections).</item>
        /// </list>
        /// Returns <c>null</c> for <c>null</c>, <c>bool</c>, or object data.
        /// </summary>
        public string? GetStringData() => StringifyValue(Data);

        private static string? StringifyValue(object? value) => value switch
        {
            null => null,
            string s => s,
            long l => l.ToString(),
            double d => d.ToString(),
            decimal m => m.ToString(),
            List<object?> list => StringifyValue(list.FirstOrDefault(x => x != null)),
            _ => null
        };

    }

    /// <summary>Quality score for a product in a specific channel and locale.</summary>
    public class ProductQualityScore
    {

        /// <summary>Channel scope this score applies to.</summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        /// <summary>Locale this score applies to.</summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        /// <summary>Letter grade quality score (e.g. <c>A</c>, <c>B</c>, <c>C</c>).</summary>
        [JsonPropertyName("data")]
        public string? Data { get; set; }

    }

    /// <summary>Completeness percentage for a product in a specific channel and locale.</summary>
    public class ProductCompleteness
    {

        /// <summary>Channel scope this completeness applies to.</summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        /// <summary>Locale this completeness applies to.</summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        /// <summary>Completeness percentage (0–100).</summary>
        [JsonPropertyName("data")]
        public int Data { get; set; }

    }

    #endregion

}

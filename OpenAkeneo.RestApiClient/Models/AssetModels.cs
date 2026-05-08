using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Asset Family

    /// <summary>Paginated list of asset families.</summary>
    public class AssetFamilyList : HalBaseInheritance
    {
        /// <summary>Asset families on the current page.</summary>
        public List<AssetFamily> AssetFamilies { get; set; } = new();
    }

    /// <summary>An Akeneo asset family, which defines the structure and rules for a group of digital assets.</summary>
    public class AssetFamily : HalItemInheritance
    {

        /// <summary>Unique code identifying the asset family.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string> Labels { get; set; } = new();

        /// <summary>Code of the attribute used as the main media (preview) for assets in this family.</summary>
        [JsonPropertyName("attribute_as_main_media")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AttributeAsMainMedia { get; set; }

        /// <summary>Naming convention rules used to auto-populate attribute values from the asset filename.</summary>
        [JsonPropertyName("naming_convention")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AssetFamilyNamingConvention? NamingConvention { get; set; }

        /// <summary>Rules that automatically link assets to products based on attribute values.</summary>
        [JsonPropertyName("product_link_rules")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? ProductLinkRules { get; set; }

        /// <summary>Image transformation definitions applied when assets are uploaded.</summary>
        [JsonPropertyName("transformations")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Transformations { get; set; }

        /// <summary>Channel codes available globally for assets in this family.</summary>
        [JsonPropertyName("global_channels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? GlobalChannels { get; set; }

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    }

    /// <summary>Naming convention configuration for an asset family.</summary>
    public class AssetFamilyNamingConvention
    {

        /// <summary>Source attribute or file property to extract the value from.</summary>
        [JsonPropertyName("source")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Source { get; set; }

        /// <summary>Regular expression pattern used to parse the source value.</summary>
        [JsonPropertyName("pattern")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Pattern { get; set; }

        /// <summary>When <c>true</c>, asset creation is aborted if the naming convention cannot be applied.</summary>
        [JsonPropertyName("abort_asset_creation_on_error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool AbortAssetCreationOnError { get; set; }

    }

    #endregion

    #region Asset Attribute

    /// <summary>List of attributes defined on an asset family.</summary>
    public class AssetAttributeList : List<AssetAttribute> { }

    /// <summary>An attribute defined on an asset family.</summary>
    public class AssetAttribute : HalItemInheritance
    {

        /// <summary>Unique code identifying this attribute within the asset family.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string> Labels { get; set; } = new();

        /// <summary>Attribute type (e.g. <c>text</c>, <c>media_file</c>, <c>single_option</c>).</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        /// <summary>Whether this attribute stores a separate value per locale.</summary>
        [JsonPropertyName("value_per_locale")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool ValuePerLocale { get; set; }

        /// <summary>Whether a value for this attribute is required for the asset to be considered complete.</summary>
        [JsonPropertyName("is_required_for_completeness")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsRequiredForCompleteness { get; set; }

        /// <summary>Whether the attribute value is read-only and cannot be modified via the API.</summary>
        [JsonPropertyName("is_read_only")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsReadOnly { get; set; }

        /// <summary>Maximum number of characters allowed (text attributes only).</summary>
        [JsonPropertyName("max_characters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxCharacters { get; set; }

        /// <summary>Whether the text attribute renders as a multi-line textarea.</summary>
        [JsonPropertyName("is_textarea")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsTextarea { get; set; }

        /// <summary>Whether the textarea renders a rich-text (WYSIWYG) editor.</summary>
        [JsonPropertyName("is_rich_text_editor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsRichTextEditor { get; set; }

        /// <summary>Validation rule applied to text values (e.g. <c>email</c>, <c>url</c>, <c>regexp</c>).</summary>
        [JsonPropertyName("validation_rule")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValidationRule { get; set; }

        /// <summary>Regular expression used when <see cref="ValidationRule"/> is <c>regexp</c>.</summary>
        [JsonPropertyName("validation_regexp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValidationRegexp { get; set; }

        /// <summary>Permitted file extensions for media file attributes.</summary>
        [JsonPropertyName("allowed_extensions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? AllowedExtensions { get; set; }

        /// <summary>Maximum file size in MB for media file attributes.</summary>
        [JsonPropertyName("max_file_size")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MaxFileSize { get; set; }

        /// <summary>Whether decimal values are allowed (number attributes only).</summary>
        [JsonPropertyName("decimals_allowed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool DecimalsAllowed { get; set; }

        /// <summary>Minimum numeric value (number attributes only).</summary>
        [JsonPropertyName("min_value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MinValue { get; set; }

        /// <summary>Maximum numeric value (number attributes only).</summary>
        [JsonPropertyName("max_value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MaxValue { get; set; }

        /// <summary>Expected media type for media file attributes (e.g. <c>image</c>, <c>pdf</c>).</summary>
        [JsonPropertyName("media_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MediaType { get; set; }

        /// <summary>Prefix prepended to the value (text attributes only).</summary>
        [JsonPropertyName("prefix")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Prefix { get; set; }

        /// <summary>Suffix appended to the value (text attributes only).</summary>
        [JsonPropertyName("suffix")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Suffix { get; set; }

        /// <summary>Code of the reference entity linked by this attribute.</summary>
        [JsonPropertyName("reference_entity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReferenceEntity { get; set; }

        /// <summary>Code of the asset family linked by this attribute.</summary>
        [JsonPropertyName("asset_family_code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AssetFamilyCode { get; set; }

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    }

    #endregion

    #region Asset Attribute Option

    /// <summary>List of selectable options for a single/multiple-option asset attribute.</summary>
    public class AssetAttributeOptionList : List<AssetAttributeOption> { }

    /// <summary>A selectable option for an asset attribute of type <c>single_option</c> or <c>multiple_options</c>.</summary>
    public class AssetAttributeOption : HalItemInheritance
    {

        /// <summary>Unique code identifying this option.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string> Labels { get; set; } = new();

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    }

    #endregion

    #region Asset

    /// <summary>Paginated list of assets within an asset family.</summary>
    public class AssetList : HalBaseInheritance
    {
        /// <summary>Assets on the current page.</summary>
        public List<Asset> Assets { get; set; } = new();
    }

    /// <summary>A single digital asset belonging to an asset family.</summary>
    public class Asset : HalItemInheritance
    {

        /// <summary>Unique code identifying this asset within its family.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Attribute values keyed by attribute code; each entry is a list of locale/channel-specific values.</summary>
        [JsonPropertyName("values")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, List<AssetValue>>? Values { get; set; }

        // Server-managed — omit from write payloads
        /// <summary>ISO 8601 timestamp of when the asset was created. Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("created")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Created { get; set; }

        /// <summary>ISO 8601 timestamp of the last update. Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("updated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Updated { get; set; }

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    }

    /// <summary>A single locale/channel-scoped attribute value on an asset.</summary>
    public class AssetValue
    {

        /// <summary>Locale code this value applies to, or <c>null</c> for non-localisable attributes.</summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        /// <summary>Channel code this value applies to, or <c>null</c> for non-scopable attributes.</summary>
        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        /// <summary>The raw attribute value; shape depends on the attribute type.</summary>
        [JsonPropertyName("data")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Data { get; set; }

        /// <summary>HAL links for media_file values: <c>download</c> (authenticated) and optional <c>share_link</c> (public CDN, requires asset sharing enabled).</summary>
        [JsonPropertyName("_links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AssetValueLinks? Links { get; set; }

        /// <summary>File metadata returned for media_file values. Read-only; omit from write payloads.</summary>
        [JsonPropertyName("linked_data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AssetMediaFileLinkedData? LinkedData { get; set; }

        /// <summary>Deserializes <see cref="Data"/> into the specified type.</summary>
        public T? GetData<T>()
        {
            if (Data == null) return default;
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(Data));
        }

    }

    /// <summary>HAL links on an asset media_file value.</summary>
    public class AssetValueLinks
    {

        /// <summary>Authenticated download URL via the Akeneo REST API.</summary>
        [JsonPropertyName("download")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AssetHref? Download { get; set; }

        /// <summary>Public CDN URL. Present only when asset sharing is enabled on the instance.</summary>
        [JsonPropertyName("share_link")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AssetHref? ShareLink { get; set; }

    }

    /// <summary>A HAL href wrapper.</summary>
    public class AssetHref
    {
        /// <summary>The URL.</summary>
        [JsonPropertyName("href")]
        public string? Href { get; set; }
    }

    /// <summary>File metadata returned inside a media_file asset value. Read-only.</summary>
    public class AssetMediaFileLinkedData
    {

        /// <summary>File size in bytes.</summary>
        [JsonPropertyName("size")]
        public long? Size { get; set; }

        /// <summary>MIME type of the file (e.g. image/png).</summary>
        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }

        /// <summary>File extension (e.g. png).</summary>
        [JsonPropertyName("extension")]
        public string? Extension { get; set; }

        /// <summary>Original filename as uploaded.</summary>
        [JsonPropertyName("original_filename")]
        public string? OriginalFilename { get; set; }

        /// <summary>ISO 8601 timestamp of the last update.</summary>
        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }

    }

    #endregion

}

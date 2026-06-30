using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Reference entity

    /// <summary>Paginated list of reference entities.</summary>
    public class ReferenceEntityList : HalBaseInheritance
    {
        /// <summary>Reference entities on the current page.</summary>
        public List<ReferenceEntity> ReferenceEntities { get; set; } = new();
    }

    /// <summary>An Akeneo reference entity definition (e.g. Brand, Designer).</summary>
    public class ReferenceEntity : HalItemInheritance
    {

        /// <summary>Unique code identifying the reference entity.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>Media file code used as the reference entity's main image.</summary>
        [JsonPropertyName("image")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Image { get; set; }

    }

    #endregion

    #region Reference entity attribute

    /// <summary>List of attributes defined on a reference entity.</summary>
    public class ReferenceEntityAttributeList : List<ReferenceEntityAttribute> { }

    /// <summary>An attribute defined on a reference entity.</summary>
    public class ReferenceEntityAttribute : HalItemInheritance
    {

        /// <summary>Unique code identifying this attribute within the reference entity.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>Attribute type (e.g. <c>text</c>, <c>image</c>, <c>single_option</c>).</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        /// <summary>Whether this attribute stores a separate value per locale.</summary>
        [JsonPropertyName("value_per_locale")]
        public bool ValuePerLocale { get; set; }

        /// <summary>Whether this attribute stores a separate value per channel.</summary>
        [JsonPropertyName("value_per_channel")]
        public bool ValuePerChannel { get; set; }

        /// <summary>Whether a value for this attribute is required for the record to be considered complete.</summary>
        [JsonPropertyName("is_required_for_completeness")]
        public bool IsRequiredForCompleteness { get; set; }

        /// <summary>Maximum number of characters allowed (text attributes only).</summary>
        [JsonPropertyName("max_characters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxCharacters { get; set; }

        /// <summary>Whether the text attribute renders as a multi-line textarea.</summary>
        [JsonPropertyName("is_textarea")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(NullableBoolFromStringConverter))]
        public bool? IsTextarea { get; set; }

        /// <summary>Whether the textarea renders a rich-text (WYSIWYG) editor.</summary>
        [JsonPropertyName("is_rich_text_editor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(NullableBoolFromStringConverter))]
        public bool? IsRichTextEditor { get; set; }

        /// <summary>Validation rule applied to text values (e.g. <c>email</c>, <c>url</c>, <c>regexp</c>).</summary>
        [JsonPropertyName("validation_rule")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValidationRule { get; set; }

        /// <summary>Regular expression used when <see cref="ValidationRule"/> is <c>regexp</c>.</summary>
        [JsonPropertyName("validation_regexp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValidationRegexp { get; set; }

        /// <summary>Whether decimal values are allowed (number attributes only).</summary>
        [JsonPropertyName("decimals_allowed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(NullableBoolFromStringConverter))]
        public bool? DecimalsAllowed { get; set; }

        /// <summary>Minimum numeric value (number/metric attributes only).</summary>
        [JsonPropertyName("min_value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MinValue { get; set; }

        /// <summary>Maximum numeric value (number/metric attributes only).</summary>
        [JsonPropertyName("max_value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MaxValue { get; set; }

        /// <summary>Permitted file extensions for file/image attributes.</summary>
        [JsonPropertyName("allowed_extensions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? AllowedExtensions { get; set; }

        /// <summary>Maximum file size in MB for file/image attributes.</summary>
        [JsonPropertyName("max_file_size")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MaxFileSize { get; set; }

        /// <summary>Code of the reference entity this attribute links to. Only present for <c>reference_entity_single_link</c> and <c>reference_entity_multiple_links</c> attribute types.</summary>
        [JsonPropertyName("reference_entity_code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReferenceEntityCode { get; set; }

        /// <summary>Code of the asset family this attribute links to. Only present for <c>asset_collection</c> attribute types.</summary>
        [JsonPropertyName("asset_family_identifier")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AssetFamilyIdentifier { get; set; }

    }

    #endregion

    #region Reference entity attribute option

    /// <summary>List of options for a single/multiple-option reference entity attribute.</summary>
    public class ReferenceEntityAttributeOptionList : List<ReferenceEntityAttributeOption> { }

    /// <summary>A selectable option for a reference entity attribute of type <c>single_option</c> or <c>multiple_options</c>.</summary>
    public class ReferenceEntityAttributeOption : HalItemInheritance
    {

        /// <summary>Unique code identifying this option.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

    }

    #endregion

    #region Reference entity record

    /// <summary>Paginated list of reference entity records.</summary>
    public class ReferenceEntityRecordList : HalBaseInheritance
    {
        /// <summary>Records on the current page.</summary>
        public List<ReferenceEntityRecord> ReferenceEntityRecords { get; set; } = new();
    }

    /// <summary>A single record (instance) of a reference entity.</summary>
    public class ReferenceEntityRecord : HalItemInheritance
    {

        /// <summary>Unique code identifying this record within its reference entity.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        // Server-managed — omit from write payloads
        /// <summary>Code of the parent reference entity. Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("reference_entity_code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReferenceEntityCode { get; set; }

        /// <summary>Timestamp of when this record was created (Unix epoch seconds). Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("created")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(EpochSecondsConverter))]
        public DateTimeOffset? Created { get; set; }

        /// <summary>Timestamp of the last update (Unix epoch seconds). Server-managed; omit from write payloads.</summary>
        [JsonPropertyName("updated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(EpochSecondsConverter))]
        public DateTimeOffset? Updated { get; set; }

        /// <summary>Attribute values keyed by attribute code; each entry is a list of locale/channel-specific values.</summary>
        [JsonPropertyName("values")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, List<ReferenceEntityRecordValue>>? Values { get; set; }

    }

    /// <summary>A single locale/channel-scoped attribute value on a reference entity record.</summary>
    public class ReferenceEntityRecordValue
    {

        /// <summary>Locale code this value applies to, or <c>null</c> for non-localisable attributes.</summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        /// <summary>Channel code this value applies to, or <c>null</c> for non-scopable attributes.</summary>
        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        /// <summary>
        /// The raw attribute value. Runtime type depends on the reference entity attribute type:
        /// <list type="bullet">
        ///   <item><description><c>text</c>, <c>text_area</c>, <c>single_option</c>, <c>image</c> — <see cref="string"/></description></item>
        ///   <item><description><c>multiple_options</c> — <see cref="System.Collections.Generic.List{T}"/> of <see cref="string"/> (option codes)</description></item>
        ///   <item><description><c>number</c> — <see cref="long"/>, <see cref="double"/>, or <see cref="decimal"/> depending on the value</description></item>
        ///   <item><description><c>yes_no</c> — <see cref="bool"/></description></item>
        ///   <item><description><c>reference_entity_single_link</c> — <see cref="string"/> (record code)</description></item>
        ///   <item><description><c>reference_entity_multiple_links</c> — <see cref="System.Collections.Generic.List{T}"/> of <see cref="string"/> (record codes)</description></item>
        /// </list>
        /// Use <see cref="GetData{T}"/> to deserialize into a typed model.
        /// </summary>
        [JsonPropertyName("data")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Data { get; set; }

        /// <summary>Deserializes <see cref="Data"/> into the specified type.</summary>
        public T? GetData<T>()
        {
            if (Data == null) return default;
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(Data));
        }

    }

    #endregion

}

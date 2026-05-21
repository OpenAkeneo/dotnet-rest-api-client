using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Attribute

    /// <summary>Paginated list of product attributes.</summary>
    public class AttributeList : HalBaseInheritance
    {
        /// <summary>Attributes on the current page.</summary>
        public List<AkeneoAttribute> Attributes { get; set; } = new();
    }

    /// <summary>An Akeneo product attribute definition.</summary>
    // NOTE: required string fields like Code use `= default!` because Akeneo guarantees they are
    // always present. If Akeneo ever violates the contract and omits one, the null will surface as
    // a NullReferenceException at point-of-use rather than at deserialization. If that becomes a
    // real problem, add a post-deserialization guard that throws with context (field name + URL).
    public class AkeneoAttribute : HalItemInheritance
    {

        /// <summary>Unique code identifying the attribute.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Attribute type (e.g. <c>pim_catalog_text</c>, <c>pim_catalog_simpleselect</c>).</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>Code of the attribute group this attribute belongs to.</summary>
        [JsonPropertyName("group")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Group { get; set; }

        /// <summary>Localised labels of the attribute's group, keyed by locale code.</summary>
        [JsonPropertyName("group_labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? GroupLabels { get; set; }

        /// <summary>Display order of this attribute within its group.</summary>
        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>Whether this attribute stores a separate value per locale.</summary>
        [JsonPropertyName("localizable")]
        public bool Localizable { get; set; }

        /// <summary>Whether this attribute stores a separate value per channel.</summary>
        [JsonPropertyName("scopable")]
        public bool Scopable { get; set; }

        /// <summary>Locale codes for which this attribute is available; <c>null</c> means all locales.</summary>
        [JsonPropertyName("available_locales")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? AvailableLocales { get; set; }

        /// <summary>Whether the attribute value must be unique across all products.</summary>
        [JsonPropertyName("unique")]
        public bool Unique { get; set; }

        /// <summary>Whether this attribute can be used as a filter in the product grid.</summary>
        [JsonPropertyName("useable_as_grid_filter")]
        public bool UseableAsGridFilter { get; set; }

        // Type-specific properties — omitted from write payloads when null to avoid
        // Akeneo rejecting properties that don't apply to the attribute's type.

        /// <summary>Maximum number of characters allowed (text/textarea attributes only).</summary>
        [JsonPropertyName("max_characters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxCharacters { get; set; }

        /// <summary>Validation rule applied to text values (e.g. <c>email</c>, <c>url</c>, <c>regexp</c>).</summary>
        [JsonPropertyName("validation_rule")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValidationRule { get; set; }

        /// <summary>Regular expression used when <see cref="ValidationRule"/> is <c>regexp</c>.</summary>
        [JsonPropertyName("validation_regexp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ValidationRegexp { get; set; }

        /// <summary>Whether the WYSIWYG editor is enabled for textarea attributes.</summary>
        [JsonPropertyName("wysiwyg_enabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? WysiwygEnabled { get; set; }

        /// <summary>Minimum numeric value (number/metric attributes only).</summary>
        [JsonPropertyName("number_min")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NumberMin { get; set; }

        /// <summary>Maximum numeric value (number/metric attributes only).</summary>
        [JsonPropertyName("number_max")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NumberMax { get; set; }

        /// <summary>Whether decimal values are allowed (number/metric attributes only).</summary>
        [JsonPropertyName("decimals_allowed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? DecimalsAllowed { get; set; }

        /// <summary>Whether negative values are allowed (number/metric attributes only).</summary>
        [JsonPropertyName("negative_allowed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? NegativeAllowed { get; set; }

        /// <summary>Measurement family code (metric attributes only).</summary>
        [JsonPropertyName("metric_family")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MetricFamily { get; set; }

        /// <summary>Default unit code within the metric family (metric attributes only).</summary>
        [JsonPropertyName("default_metric_unit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DefaultMetricUnit { get; set; }

        /// <summary>Earliest allowed date (date attributes only).</summary>
        [JsonPropertyName("date_min")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DateMin { get; set; }

        /// <summary>Latest allowed date (date attributes only).</summary>
        [JsonPropertyName("date_max")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DateMax { get; set; }

        /// <summary>Permitted file extensions (file/image attributes only).</summary>
        [JsonPropertyName("allowed_extensions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? AllowedExtensions { get; set; }

        /// <summary>Maximum file size in MB (file/image attributes only).</summary>
        [JsonPropertyName("max_file_size")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MaxFileSize { get; set; }

        /// <summary>Reference data entity name linked by this attribute.</summary>
        [JsonPropertyName("reference_data_name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReferenceDataName { get; set; }

        /// <summary>Default boolean value (yes/no attributes only).</summary>
        [JsonPropertyName("default_value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? DefaultValue { get; set; }

        /// <summary>Column definitions for table attributes.</summary>
        [JsonPropertyName("table_configuration")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<AttributeTableConfiguration>? TableConfiguration { get; set; }

        /// <summary>Whether this attribute is the main product identifier.</summary>
        [JsonPropertyName("is_main_identifier")]
        public bool IsMainIdentifier { get; set; }

        /// <summary>Whether a value for this attribute is mandatory.</summary>
        [JsonPropertyName("is_mandatory")]
        public bool IsMandatory { get; set; }

        /// <summary>Strategy for controlling decimal precision display (e.g. <c>fixed</c>, <c>variable</c>).</summary>
        [JsonPropertyName("decimal_places_strategy")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DecimalPlacesStrategy { get; set; }

        /// <summary>Number of decimal places to display when <see cref="DecimalPlacesStrategy"/> is <c>fixed</c>.</summary>
        [JsonPropertyName("decimal_places")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? DecimalPlaces { get; set; }

        /// <summary>Whether new options can be created on the fly during import.</summary>
        [JsonPropertyName("enable_option_creation_during_import")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? EnableOptionCreationDuringImport { get; set; }

        /// <summary>Maximum number of items selectable in a multi-select or collection attribute.</summary>
        [JsonPropertyName("max_items_count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MaxItemsCount { get; set; }

        // Properties in API responses but not in API documentation:

        /// <summary>Minimum number of characters required before the search suggestion dropdown activates.</summary>
        [JsonPropertyName("minimum_input_length")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinimumInputLength { get; set; }

        /// <summary>Localised usage guidelines for this attribute, keyed by locale code.</summary>
        [JsonPropertyName("guidelines")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Guidelines { get; set; }

        /// <summary>Whether options are sorted automatically in alphabetical order.</summary>
        [JsonPropertyName("auto_option_sorting")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AutoOptionSorting { get; set; }

        /// <summary>Whether HTML tags are stripped from the stored text value.</summary>
        [JsonPropertyName("remove_html_tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? RemoveHtmlTags { get; set; }

        /// <summary>Whether the attribute value is read-only and cannot be modified via the API.</summary>
        [JsonPropertyName("is_read_only")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsReadOnly { get; set; }

        /// <summary>Whether the time portion is shown for date-time attributes.</summary>
        [JsonPropertyName("display_time")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? DisplayTime { get; set; }

        /// <summary>Unicode characters that are considered invalid for this attribute's values.</summary>
        [JsonPropertyName("invalid_characters_in_unicode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? InvalidCharactersInUnicode { get; set; }

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    }

    /// <summary>Column definition for a table attribute.</summary>
    public class AttributeTableConfiguration
    {

        /// <summary>Unique code identifying this column within the table attribute.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Data type of this column (e.g. <c>text</c>, <c>number</c>, <c>select</c>).</summary>
        [JsonPropertyName("data_type")]
        public string DataType { get; set; } = default!;

        // TODO: Improve
        /// <summary>Validation rules applied to values in this column.</summary>
        [JsonPropertyName("validations")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Validations { get; set; }

        /// <summary>Localised display labels for this column, keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>Whether a value in this column is required for completeness calculation.</summary>
        [JsonPropertyName("is_required_for_completeness")]
        public bool IsRequiredForCompleteness { get; set; }

    }

    #endregion

    #region Attribute option

    /// <summary>Paginated list of options for a select attribute.</summary>
    public class AttributeOptionList : HalBaseInheritance
    {
        /// <summary>Attribute options on the current page.</summary>
        public List<AttributeOption> AttributeOptions { get; set; } = new();
    }

    /// <summary>A selectable option for a <c>simpleselect</c> or <c>multiselect</c> attribute.</summary>
    public class AttributeOption : HalItemInheritance
    {

        /// <summary>Unique code identifying this option within its attribute.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Code of the attribute this option belongs to.</summary>
        [JsonPropertyName("attribute")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Attribute { get; set; }

        /// <summary>Display order of this option within the attribute's option list.</summary>
        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    }

    #endregion

    #region Attribute group

    /// <summary>Paginated list of attribute groups.</summary>
    public class AttributeGroupList : HalBaseInheritance
    {
        /// <summary>Attribute groups on the current page.</summary>
        public List<AttributeGroup> AttributeGroups { get; set; } = new();
    }

    /// <summary>An attribute group, used to organise attributes in the Akeneo UI.</summary>
    public class AttributeGroup : HalItemInheritance
    {

        /// <summary>Unique code identifying the attribute group.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Display order of this group relative to other attribute groups.</summary>
        [JsonPropertyName("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>Codes of attributes belonging to this group.</summary>
        [JsonPropertyName("attributes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Attributes { get; set; }

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    }

    #endregion

}

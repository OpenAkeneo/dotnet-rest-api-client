using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Family

    /// <summary>Paginated list of product families.</summary>
    public class FamilyList : HalBaseInheritance
    {
        /// <summary>Families on the current page.</summary>
        public List<Family> Families { get; set; } = new();
    }

    /// <summary>An Akeneo product family, which defines the set of attributes shared by a group of products.</summary>
    public class Family : HalItemInheritance
    {

        /// <summary>Unique code identifying the family.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Code of the attribute used as the product label in lists and search results.</summary>
        [JsonPropertyName("attribute_as_label")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AttributeAsLabel { get; set; }

        /// <summary>Code of the attribute used as the product's main image.</summary>
        [JsonPropertyName("attribute_as_image")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AttributeAsImage { get; set; }

        /// <summary>Codes of all attributes belonging to this family.</summary>
        [JsonPropertyName("attributes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Attributes { get; set; }

        /// <summary>Required attributes per channel; keyed by channel code, each value is a list of attribute codes.</summary>
        [JsonPropertyName("attribute_requirements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, List<string>>? AttributeRequirements { get; set; }

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>Code of the parent family (for family inheritance).</summary>
        [JsonPropertyName("parent")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Parent { get; set; }

    }

    #endregion

}

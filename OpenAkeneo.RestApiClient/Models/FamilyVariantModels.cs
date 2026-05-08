using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{


    #region Family variant

    /// <summary>Paginated list of family variants.</summary>
    public class FamilyVariantList : HalBaseInheritance
    {
        /// <summary>Family variants on the current page.</summary>
        public List<FamilyVariant> FamilyVariants { get; set; } = new();
    }

    /// <summary>A family variant, which extends a family by defining variation axes and per-level attribute sets.</summary>
    public class FamilyVariant : HalItemInheritance
    {

        /// <summary>Unique code identifying the family variant.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Attribute codes shared by all variant products (not used as variation axes).</summary>
        [JsonPropertyName("common_attributes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? CommonAttributes { get; set; }

        /// <summary>Attribute sets for each variation level, defining axes and level-specific attributes.</summary>
        [JsonPropertyName("variant_attribute_sets")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<FamilyVariantAttributeSet>? VariantAttributeSets { get; set; }

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

    }

    /// <summary>The attribute set for one variation level within a family variant.</summary>
    public class FamilyVariantAttributeSet : HalItemInheritance
    {

        /// <summary>Variation level number (1 for the first level, 2 for the second).</summary>
        [JsonPropertyName("level")]
        public int Level { get; set; }

        /// <summary>Attribute codes used as variation axes at this level.</summary>
        [JsonPropertyName("axes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Axes { get; set; }

        /// <summary>Attribute codes assigned to products or product models at this variation level.</summary>
        [JsonPropertyName("attributes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Attributes { get; set; }

    }

    #endregion


}

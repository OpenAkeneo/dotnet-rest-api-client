using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{


    #region Association type

    /// <summary>Paginated list of product association types.</summary>
    public class AssociationTypeList : HalBaseInheritance
    {
        /// <summary>Association types on the current page.</summary>
        public List<AssociationType> AssociationTypes { get; set; } = new();
    }

    /// <summary>An Akeneo association type, which defines a named relationship between products.</summary>
    public class AssociationType : HalItemInheritance
    {

        /// <summary>Unique code identifying the association type.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

        /// <summary>Whether this association type carries a quantity value on each linked product.</summary>
        [JsonPropertyName("is_quantified")]
        public bool IsQuantified { get; set; }

        /// <summary>Whether this association is two-way (associating A with B also associates B with A).</summary>
        [JsonPropertyName("is_two_way")]
        public bool IsTwoWay { get; set; }

    }

    #endregion


}

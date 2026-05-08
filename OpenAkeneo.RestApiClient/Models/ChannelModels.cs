using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Channel

    /// <summary>Paginated list of channels.</summary>
    public class ChannelList : HalBaseInheritance
    {
        /// <summary>Channels on the current page.</summary>
        public List<Channel> Channels { get; set; } = new();
    }

    /// <summary>An Akeneo channel (scope), representing a sales outlet with its own locale and currency settings.</summary>
    public class Channel : HalItemInheritance
    {

        /// <summary>Unique code identifying the channel.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Currency codes active for this channel.</summary>
        [JsonPropertyName("currencies")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Currencies { get; set; }

        /// <summary>Locale codes active for this channel.</summary>
        [JsonPropertyName("locales")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Locales { get; set; }

        /// <summary>Code of the root category tree used by this channel.</summary>
        [JsonPropertyName("category_tree")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CategoryTree { get; set; }

        /// <summary>Measurement unit conversion overrides for this channel; keyed by metric family code.</summary>
        [JsonPropertyName("conversion_units")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? ConversionUnits { get; set; }

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Labels { get; set; }

    }

    #endregion

}

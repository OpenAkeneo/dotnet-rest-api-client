using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Locale

    /// <summary>Paginated list of locales.</summary>
    public class LocaleList : HalBaseInheritance
    {
        /// <summary>Locales on the current page.</summary>
        public List<Locale> Locales { get; set; } = new();
    }

    /// <summary>An Akeneo locale (language/region combination).</summary>
    public class Locale : HalItemInheritance
    {

        /// <summary>IETF locale code (e.g. <c>en_US</c>, <c>fr_FR</c>).</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Whether this locale is currently active in the PIM.</summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

    }

    #endregion

}

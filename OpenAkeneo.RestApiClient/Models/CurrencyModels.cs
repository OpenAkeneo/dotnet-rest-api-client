using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Currency

    /// <summary>Paginated list of currencies.</summary>
    public class CurrencyList : HalBaseInheritance
    {
        /// <summary>Currencies on the current page.</summary>
        public List<Currency> Currencies { get; set; } = new();
    }

    /// <summary>An Akeneo currency.</summary>
    public class Currency : HalItemInheritance
    {

        /// <summary>ISO 4217 currency code (e.g. <c>USD</c>, <c>EUR</c>).</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Whether this currency is currently active in the PIM.</summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

    }

    #endregion

}

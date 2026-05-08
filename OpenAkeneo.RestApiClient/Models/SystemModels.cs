using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region System

    /// <summary>Version and edition information about the Akeneo PIM instance.</summary>
    public class SystemInformation
    {

        /// <summary>Akeneo PIM version string (e.g. <c>7.0.12</c>).</summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = default!;

        /// <summary>PIM edition (e.g. <c>Community</c>, <c>Growth</c>, <c>Enterprise</c>).</summary>
        [JsonPropertyName("edition")]
        public string Edition { get; set; } = default!;

    }

    #endregion

}

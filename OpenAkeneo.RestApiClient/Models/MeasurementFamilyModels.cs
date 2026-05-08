using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Measurement family

    /// <summary>An Akeneo measurement family, grouping related units of measure (e.g. Weight, Length).</summary>
    public class MeasurementFamily
    {

        /// <summary>Unique code identifying the measurement family (e.g. <c>Weight</c>).</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string?> Labels { get; set; } = new();

        /// <summary>Code of the unit used as the base for all conversions within this family.</summary>
        [JsonPropertyName("standard_unit_code")]
        public string StandardUnitCode { get; set; } = default!;

        /// <summary>
        /// Dictionary keyed by unit code. Each value is a raw JSON element because the unit object
        /// structure can be complex (convert_from_standard operations, symbol, labels).
        /// </summary>
        [JsonPropertyName("units")]
        public Dictionary<string, MeasurementUnit> Units { get; set; } = new();

    }

    /// <summary>A single unit of measure within a measurement family.</summary>
    public class MeasurementUnit
    {

        /// <summary>Unique code identifying this unit (e.g. <c>KILOGRAM</c>).</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string?> Labels { get; set; } = new();

        /// <summary>Ordered list of arithmetic operations to convert a value from the standard unit to this unit.</summary>
        [JsonPropertyName("convert_from_standard")]
        public List<MeasurementConversion> ConvertFromStandard { get; set; } = new();

        /// <summary>Unit symbol shown in the UI (e.g. <c>kg</c>, <c>m</c>).</summary>
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

    }

    /// <summary>A single arithmetic operation in a unit conversion chain.</summary>
    public class MeasurementConversion
    {

        /// <summary>Arithmetic operator to apply (e.g. <c>mul</c>, <c>div</c>, <c>add</c>, <c>sub</c>).</summary>
        [JsonPropertyName("operator")]
        public string Operator { get; set; } = default!;

        /// <summary>Numeric value used with the operator.</summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = default!;

    }

    #endregion

}

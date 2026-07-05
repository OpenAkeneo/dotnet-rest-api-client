using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Rule definitions

    /// <summary>Paginated list of rule definitions.</summary>
    public class RuleDefinitionList : HalBaseInheritance
    {
        /// <summary>Rule definitions on the current page.</summary>
        public List<RuleDefinition> RuleDefinitions { get; set; } = new();
    }

    /// <summary>An Akeneo rule definition (Rules Engine / Enterprise feature).</summary>
    public class RuleDefinition : HalItemInheritance
    {

        /// <summary>Unique code identifying the rule.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Rule type (e.g. <c>product</c>).</summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>How the rule executes (e.g. <c>manual</c>, <c>automatic</c>).</summary>
        [JsonPropertyName("execution_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ExecutionType { get; set; }

        /// <summary>Whether the rule is enabled.</summary>
        [JsonPropertyName("enabled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(NullableBoolFromStringConverter))]
        public bool? Enabled { get; set; }

        /// <summary>Execution priority; higher runs first.</summary>
        [JsonPropertyName("priority")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Priority { get; set; }

        /// <summary>Reference entity identifier, for reference-entity-scoped rules.</summary>
        [JsonPropertyName("reference_entity_identifier")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReferenceEntityIdentifier { get; set; }

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string?>? Labels { get; set; }

        /// <summary>Rule conditions. Structure varies by condition type; kept polymorphic.</summary>
        [JsonPropertyName("conditions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Conditions { get; set; }

        /// <summary>Rule actions. Structure varies by action type; kept polymorphic.</summary>
        [JsonPropertyName("actions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Actions { get; set; }

        /// <summary>Rule triggers. Structure varies; kept polymorphic.</summary>
        [JsonPropertyName("triggers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Triggers { get; set; }

        /// <summary>Distinct action types used by the rule. Server-managed read-only.</summary>
        [JsonPropertyName("action_types")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? ActionTypes { get; set; }

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    }

    #endregion

}

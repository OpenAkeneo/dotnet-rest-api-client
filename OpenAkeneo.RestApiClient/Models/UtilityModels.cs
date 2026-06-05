using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Permissions

    /// <summary>A user's view and edit permissions scoped to individual channels.</summary>
    public class UserChannelsPermissions
    {
        /// <summary>UUID of the user these permissions apply to.</summary>
        [JsonPropertyName("userUuid")]
        public string? UserUuid { get; set; }

        /// <summary>Permission scopes keyed by channel code.</summary>
        [JsonPropertyName("channels")]
        public Dictionary<string, PermissionScope>? Channels { get; set; }
    }

    /// <summary>A user's view and edit permissions scoped to individual locales.</summary>
    public class UserLocalesPermissions
    {
        /// <summary>UUID of the user these permissions apply to.</summary>
        [JsonPropertyName("userUuid")]
        public string? UserUuid { get; set; }

        /// <summary>Permission scopes keyed by locale code.</summary>
        [JsonPropertyName("locales")]
        public Dictionary<string, PermissionScope>? Locales { get; set; }
    }

    /// <summary>View and edit permission flags for a single resource scope.</summary>
    public class PermissionScope
    {
        /// <summary>Indicates whether the user can view this scope.</summary>
        [JsonPropertyName("canView")]
        public bool? CanView { get; set; }

        /// <summary>Indicates whether the user can edit this scope.</summary>
        [JsonPropertyName("canEdit")]
        public bool? CanEdit { get; set; }
    }

    #endregion

    #region Overview

    /// <summary>Top-level overview of the Akeneo REST API, listing available routes and authentication endpoints.</summary>
    public class ApiOverview
    {
        /// <summary>Base host URL of the API.</summary>
        [JsonPropertyName("host")]
        public string? Host { get; set; }

        /// <summary>Authentication endpoints keyed by route name.</summary>
        [JsonPropertyName("authentication")]
        public Dictionary<string, ApiRouteDefinition>? Authentication { get; set; }

        /// <summary>All available API routes keyed by route name.</summary>
        [JsonPropertyName("routes")]
        public Dictionary<string, ApiRouteDefinition>? Routes { get; set; }
    }

    /// <summary>Definition of a single API route, including its path template and allowed HTTP methods.</summary>
    public class ApiRouteDefinition
    {
        /// <summary>Route path template (e.g. <c>/api/rest/v1/products/{code}</c>).</summary>
        [JsonPropertyName("route")]
        public string? Route { get; set; }

        /// <summary>HTTP methods supported by this route (e.g. <c>GET</c>, <c>POST</c>, <c>PATCH</c>).</summary>
        [JsonPropertyName("methods")]
        public List<string>? Methods { get; set; }
    }

    #endregion

    #region Extensions

    /// <summary>
    /// A UI extension registered with the current token, as returned by <c>GET /ui-extensions</c>.
    /// </summary>
    public class Extension
    {
        /// <summary>Name of the extension, shown within the Administration interface.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>Short human-readable description, shown within the Administration interface.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Version of the extension, shown within the Administration interface.</summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        /// <summary>Position of the extension (e.g. <c>pim.product.tab</c>).</summary>
        [JsonPropertyName("position")]
        public string? Position { get; set; }

        /// <summary>Type of the extension (e.g. <c>iframe</c>).</summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>Type-specific configuration options for the extension.</summary>
        [JsonPropertyName("configuration")]
        public Dictionary<string, JsonElement>? Configuration { get; set; }

        /// <summary>Status of the extension (<c>active</c> or <c>inactive</c>).</summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>Credentials registered for the extension.</summary>
        [JsonPropertyName("credentials")]
        public List<JsonElement>? Credentials { get; set; }
    }

    #endregion

    #region Data Architect Agent

    /// <summary>An AI-generated attribute modelling suggestion produced by the Akeneo Data Architect Agent.</summary>
    public class ModelizationSuggestion
    {
        /// <summary>Unique UUID of this suggestion.</summary>
        [JsonPropertyName("uuid")]
        public string? Uuid { get; set; }

        /// <summary>Processing status of the suggestion (e.g. <c>pending</c>, <c>completed</c>).</summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>Source input used to generate the suggestion (e.g. a product description or CSV excerpt).</summary>
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        /// <summary>Username or identifier of the user who requested the suggestion.</summary>
        [JsonPropertyName("author")]
        public string? Author { get; set; }

        /// <summary>Timestamp of when the suggestion was created.</summary>
        [JsonPropertyName("created_at")]
        [JsonConverter(typeof(AkeneoDateTimeOffsetConverter))]
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>Raw list of attributes suggested by the AI agent.</summary>
        [JsonPropertyName("raw_suggested_attributes")]
        public List<SuggestedAttribute>? RawSuggestedAttributes { get; set; }

        /// <summary>Free-text comments from the AI agent about the suggestion.</summary>
        [JsonPropertyName("additional_comments")]
        public string? AdditionalComments { get; set; }
    }

    /// <summary>A single attribute proposed within a modelling suggestion.</summary>
    public class SuggestedAttribute
    {
        /// <summary>Proposed attribute code.</summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>Proposed attribute type (e.g. <c>pim_catalog_text</c>).</summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>AI-generated description explaining why this attribute was suggested.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>Paginated list of modelling suggestions.</summary>
    public class ModelizationSuggestionList : HalBaseInheritance
    {
        /// <summary>Raw embedded payload from the HAL response.</summary>
        [JsonPropertyName("_embedded")]
        public EmbeddedModelizationSuggestionList? Embedded { get; set; }

        /// <summary>Convenience accessor that unwraps the embedded suggestion items.</summary>
        [JsonIgnore]
        public List<ModelizationSuggestion> Suggestions
        {
            get => Embedded?.Items ?? new List<ModelizationSuggestion>();
            set
            {
                if (Embedded == null) Embedded = new EmbeddedModelizationSuggestionList();
                Embedded.Items = value;
            }
        }
    }

    /// <summary>Embedded wrapper for a list of modelling suggestions within a HAL response.</summary>
    public class EmbeddedModelizationSuggestionList
    {
        /// <summary>Suggestion items contained in this embedded wrapper.</summary>
        [JsonPropertyName("items")]
        public List<ModelizationSuggestion>? Items { get; set; }
    }

    #endregion

}

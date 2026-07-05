using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Bulk upsert results

    /// <summary>
    /// The per-item outcome of a bulk create-or-update call (Akeneo batch PATCH endpoints).
    /// Depending on the resource, the item is identified by <see cref="Code"/>,
    /// <see cref="Identifier"/>, or <see cref="Uuid"/> — <see cref="Key"/> returns whichever is set.
    /// </summary>
    public class BulkItemResult
    {

        /// <summary>1-based line number of the item within the submitted batch. For batches larger
        /// than the API's 100-item page the client renumbers lines to be absolute across chunks.</summary>
        [JsonPropertyName("line")]
        public int Line { get; set; }

        /// <summary>Resource code, for code-keyed resources.</summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>Product identifier (SKU), for identifier-keyed products.</summary>
        [JsonPropertyName("identifier")]
        public string? Identifier { get; set; }

        /// <summary>Product UUID, for UUID-keyed products.</summary>
        [JsonPropertyName("uuid")]
        public string? Uuid { get; set; }

        /// <summary>HTTP-style status for this item: 201 created, 204 updated, 422 rejected.</summary>
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        /// <summary>Human-readable message, present when the item was rejected.</summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>Per-field validation errors, present on some 422 rejections.</summary>
        [JsonPropertyName("errors")]
        public List<BulkItemError>? Errors { get; set; }

        /// <summary>Catch-all for extra JSON properties not mapped to a named member.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        /// <summary>Whichever of <see cref="Code"/>, <see cref="Identifier"/>, <see cref="Uuid"/> identifies this item.</summary>
        [JsonIgnore]
        public string? Key => Code ?? Identifier ?? Uuid;

        /// <summary>True when the item was created (201) or updated (204).</summary>
        [JsonIgnore]
        public bool Succeeded => StatusCode is >= 200 and < 300;

    }

    /// <summary>A single per-field validation error inside a rejected bulk item.</summary>
    public class BulkItemError
    {

        /// <summary>The attribute or property that failed validation.</summary>
        [JsonPropertyName("property")]
        public string? Property { get; set; }

        /// <summary>Human-readable description of the validation failure.</summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

    }

    #endregion

}

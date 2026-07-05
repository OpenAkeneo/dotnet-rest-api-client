using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region HAL core models

    /// <summary>Paginated HAL response envelope returned by Akeneo list endpoints.</summary>
    public class HalResponse
    {

        /// <summary>Pagination navigation links (self, first, next, previous).</summary>
        [JsonPropertyName("_links")]
        public HalLinks? Links { get; set; }

        /// <summary>Embedded resource collections keyed by relation name.</summary>
        [JsonPropertyName("_embedded")]
        public Dictionary<string, JsonElement>? Embedded { get; set; }

        /// <summary>Current page number (1-based).</summary>
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        /// <summary>Total number of items across all pages.</summary>
        [JsonPropertyName("items_count")]
        public int ItemsCount { get; set; }

    }

    /// <summary>
    /// Typed HAL envelope: deserialises the embedded items directly into <typeparamref name="T"/>
    /// in a single pass (the untyped <see cref="HalResponse"/> retains items as
    /// <see cref="JsonElement"/>s that must be deserialised a second time).
    /// </summary>
    public class HalResponse<T>
    {

        /// <summary>Pagination navigation links (self, first, next, previous).</summary>
        [JsonPropertyName("_links")]
        public HalLinks? Links { get; set; }

        /// <summary>Embedded items container.</summary>
        [JsonPropertyName("_embedded")]
        public HalEmbeddedItems<T>? Embedded { get; set; }

        /// <summary>Current page number (1-based).</summary>
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        /// <summary>Total number of items across all pages (when requested with <c>with_count</c>).</summary>
        [JsonPropertyName("items_count")]
        public int ItemsCount { get; set; }

    }

    /// <summary>The <c>_embedded</c> container of a typed HAL response.</summary>
    public class HalEmbeddedItems<T>
    {

        /// <summary>The embedded resource items.</summary>
        [JsonPropertyName("items")]
        public List<T>? Items { get; set; }

    }

    /// <summary>A single HAL hyperlink containing an href URL.</summary>
    public class HalHref
    {

        /// <summary>The URL of the link.</summary>
        [JsonPropertyName("href")]
        public string? Href { get; set; }

    }

    /// <summary>Navigation links present on a paginated HAL response.</summary>
    public class HalLinks
    {

        /// <summary>Link to the current page.</summary>
        [JsonPropertyName("self")]
        public HalHref? Self { get; set; }

        /// <summary>Link to the first page. Null when on the first page.</summary>
        [JsonPropertyName("first")]
        public HalHref? First { get; set; }

        /// <summary>Link to the next page. Null when on the last page.</summary>
        [JsonPropertyName("next")]
        public HalHref? Next { get; set; }

        /// <summary>Link to the previous page. Null when on the first page.</summary>
        [JsonPropertyName("previous")]
        public HalHref? Previous { get; set; }

    }

    #endregion

    #region HAL item specific models

    /// <summary>Base class for list-level HAL responses that carry a <c>_links</c> object with pagination links.</summary>
    public class HalBaseInheritance
    {

        /// <summary>Pagination navigation links.</summary>
        [JsonPropertyName("_links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HalLinks? Links { get; set; }

    }

    /// <summary>Base class for individual resource items that carry item-level HAL links.</summary>
    public class HalItemInheritance
    {

        /// <summary>HAL links for this resource item (self, image_download).</summary>
        [JsonPropertyName("_links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HalItemLinks? Links { get; set; }

    }

    /// <summary>HAL links attached to an individual resource item.</summary>
    public class HalItemLinks
    {
        /// <summary>Self link pointing to this resource's API URL.</summary>
        [JsonPropertyName("self")]
        public HalHref? Self { get; set; }

        /// <summary>Download link for the item's main image, if available.</summary>
        [JsonPropertyName("image_download")]
        public HalHref? ImageDownload { get; set; }
    }

    #endregion

}

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    /// <summary>An Akeneo App catalog, which scopes a connected app's access to a subset of products.</summary>
    public class Catalog : HalItemInheritance
    {
        /// <summary>Unique UUID identifying the catalog.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Human-readable name of the catalog.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>Whether the catalog is currently enabled and accessible.</summary>
        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        /// <summary>Currency codes managed by this catalog.</summary>
        [JsonPropertyName("managed_currencies")]
        public List<string>? ManagedCurrencies { get; set; }

        /// <summary>Locale codes managed by this catalog.</summary>
        [JsonPropertyName("managed_locales")]
        public List<string>? ManagedLocales { get; set; }
    }

    /// <summary>Paginated list of App catalogs.</summary>
    public class CatalogList : HalBaseInheritance
    {
        /// <summary>Raw embedded payload from the HAL response.</summary>
        [JsonPropertyName("_embedded")]
        public EmbeddedCatalogList? Embedded { get; set; }

        /// <summary>Convenience accessor that unwraps the embedded catalog items.</summary>
        [JsonIgnore]
        public List<Catalog> Catalogs
        {
            get => Embedded?.Items ?? new List<Catalog>();
            set
            {
                if (Embedded == null) Embedded = new EmbeddedCatalogList();
                Embedded.Items = value;
            }
        }
    }

    /// <summary>Embedded wrapper for a list of catalogs within a HAL response.</summary>
    public class EmbeddedCatalogList
    {
        /// <summary>Catalog items contained in this embedded wrapper.</summary>
        [JsonPropertyName("items")]
        public List<Catalog>? Items { get; set; }
    }

    /// <summary>Paginated list of product UUIDs exposed by a catalog.</summary>
    public class CatalogProductUuidList : HalBaseInheritance
    {
        /// <summary>Raw embedded payload from the HAL response.</summary>
        [JsonPropertyName("_embedded")]
        public EmbeddedProductUuidList? Embedded { get; set; }

        /// <summary>Convenience accessor that unwraps the embedded UUID items.</summary>
        [JsonIgnore]
        public List<string> Uuids
        {
            get => Embedded?.Items ?? new List<string>();
            set
            {
                if (Embedded == null) Embedded = new EmbeddedProductUuidList();
                Embedded.Items = value;
            }
        }
    }

    /// <summary>Embedded wrapper for a list of product UUIDs within a HAL response.</summary>
    public class EmbeddedProductUuidList
    {
        /// <summary>Product UUID strings contained in this embedded wrapper.</summary>
        [JsonPropertyName("items")]
        public List<string>? Items { get; set; }
    }

    /// <summary>The product mapping schema for a catalog; structure varies per catalog configuration.</summary>
    public class CatalogMappingSchema
    {
        // Dynamic payload, as the mapping schema varies greatly based on setup
        /// <summary>Dynamic schema properties; keyed by property name with raw JSON values.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? Properties { get; set; }
    }

}

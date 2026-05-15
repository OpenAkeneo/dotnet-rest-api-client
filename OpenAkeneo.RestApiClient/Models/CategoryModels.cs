using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Category

    /// <summary>Paginated list of product categories.</summary>
    public class CategoryList : HalBaseInheritance
    {
        /// <summary>Categories on the current page.</summary>
        public List<Category> Categories { get; set; } = new();
    }

    /// <summary>An Akeneo product category (node in the category tree).</summary>
    public class Category : HalItemInheritance
    {

        /// <summary>Unique code identifying the category.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Code of the parent category; <c>null</c> for root categories.</summary>
        [JsonPropertyName("parent")]
        public string? Parent { get; set; }

        /// <summary>Timestamp of the last update. Ignored on write when null.</summary>
        [JsonPropertyName("updated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? Updated { get; set; }

        /// <summary>Position of this category relative to its siblings in the tree.</summary>
        [JsonPropertyName("position")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Position { get; set; }

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string> Labels { get; set; } = new();

        /// <summary>Channel codes for which this category is required.</summary>
        [JsonPropertyName("channel_requirements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? ChannelRequirements { get; set; }

        /// <summary>Enriched category attribute values keyed by attribute code.</summary>
        [JsonPropertyName("values")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, CategoryValue>? Values { get; set; }

        /// <summary>Validation rules applied to product categorisation for this category.</summary>
        [JsonPropertyName("validations")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Validations { get; set; }

    }

    /// <summary>A single enriched attribute value stored on a category.</summary>
    public class CategoryValue
    {

        /// <summary>Attribute type (e.g. <c>text</c>, <c>image</c>).</summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>Locale code this value applies to.</summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        /// <summary>Channel code this value applies to.</summary>
        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        /// <summary>Code of the attribute this value belongs to.</summary>
        [JsonPropertyName("attribute_code")]
        public string? AttributeCode { get; set; }

        /// <summary>
        /// The raw attribute value. Runtime type depends on the category attribute type:
        /// <list type="bullet">
        ///   <item><description><c>text</c>, <c>textarea</c>, <c>image</c>, <c>file</c>, <c>single_link</c>, <c>date</c> — <see cref="string"/></description></item>
        ///   <item><description><c>number</c> — <see cref="long"/>, <see cref="double"/>, or <see cref="decimal"/> depending on the value</description></item>
        ///   <item><description><c>checkbox</c> (boolean) — <see cref="bool"/></description></item>
        ///   <item><description><c>multi_link</c> — <see cref="System.Collections.Generic.List{T}"/> of <see cref="string"/></description></item>
        /// </list>
        /// Use <c>value.GetData&lt;T&gt;()</c> if you need type-safe deserialization.
        /// </summary>
        [JsonPropertyName("data")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? Data { get; set; }

        /// <summary>Deserializes <see cref="Data"/> into the specified type.</summary>
        public T? GetData<T>()
        {
            if (Data == null) return default;
            return System.Text.Json.JsonSerializer.Deserialize<T>(System.Text.Json.JsonSerializer.Serialize(Data));
        }

    }

    /// <summary>Validation constraints controlling how products may be categorised under a given category.</summary>
    public class CategoryValidation
    {

        /// <summary>Maximum number of categories a product may belong to.</summary>
        [JsonPropertyName("max_categories_per_product")]
        public int MaxCategoriesPerProduct { get; set; }

        /// <summary>When <c>true</c>, products may only be placed in leaf (childless) categories.</summary>
        [JsonPropertyName("only_leaves")]
        public bool OnlyLeaves { get; set; }

        /// <summary>When <c>true</c>, every product must be assigned to at least one category.</summary>
        [JsonPropertyName("is_mandatory")]
        public bool IsMandatory { get; set; }

    }

    #endregion

}

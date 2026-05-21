using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Converters
{
    internal class PolymorphicDataConverter : JsonConverter<object?>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadValue(ref reader, options);
        }

        private object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long l))
                        return l;
                    if (reader.TryGetDouble(out double d))
                        return d;
                    return reader.GetDecimal();
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.StartObject:
                    var dict = new Dictionary<string, object?>();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                            return dict;

                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException("Expected PropertyName token");

                        string propertyName = reader.GetString()!;
                        reader.Read(); // move to value
                        dict.Add(propertyName, ReadValue(ref reader, options));
                    }
                    throw new JsonException("Expected EndObject token");
                case JsonTokenType.StartArray:
                    var list = new List<object?>();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                            return list;

                        list.Add(ReadValue(ref reader, options));
                    }
                    throw new JsonException("Expected EndArray token");
                default:
                    using (var document = JsonDocument.ParseValue(ref reader))
                    {
                        return document.RootElement.Clone();
                    }
            }
        }

        public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // TODO: nested Dictionary<string,object?> values produced by Read() have runtime type
            // `object`, which can throw NotSupportedException in .NET 8 when this converter is not
            // registered on a shared JsonSerializerOptions. To fully fix round-tripping of
            // polymorphic product values (metric, price, table) through a PATCH body, create a
            // static shared JsonSerializerOptions with this converter registered globally and pass
            // it to every JsonSerializer.Deserialize/Serialize call in the library.
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}

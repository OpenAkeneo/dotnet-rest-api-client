using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Converters
{
    /// <summary>
    /// Handles Akeneo fields that are typed bool? but may arrive as a JSON string ("true"/"false").
    /// </summary>
    internal class NullableBoolFromStringConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.True)
                return true;

            if (reader.TokenType == JsonTokenType.False)
                return false;

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (bool.TryParse(s, out var parsed))
                    return parsed;

                // "0"/"1" style
                if (s == "1") return true;
                if (s == "0") return false;

                throw new JsonException($"Cannot convert string '{s}' to bool?.");
            }

            throw new JsonException($"Unexpected token type '{reader.TokenType}' for bool?.");
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteBooleanValue(value.Value);
        }
    }
}

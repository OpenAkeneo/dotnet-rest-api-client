using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Converters
{
    /// <summary>
    /// Deserializes a nullable <see cref="DateTimeOffset"/> from the various formats Akeneo uses across
    /// its API versions:
    /// <list type="bullet">
    ///   <item>Unix epoch-seconds integer — reference-entity records on some versions</item>
    ///   <item>ISO-8601 with colon offset: <c>2024-03-15T10:30:00+00:00</c> or <c>…Z</c></item>
    ///   <item>ISO-8601 without colon offset: <c>2024-03-15T10:30:00+0000</c></item>
    /// </list>
    /// Serializes back as an ISO-8601 string with <c>+00:00</c> offset.
    /// </summary>
    internal class AkeneoDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        // Formats Akeneo has been observed to return across API versions.
        private static readonly string[] _formats =
        [
            "yyyy-MM-ddTHH:mm:sszzz",   // +00:00 — standard, handled natively but included for completeness
            "yyyy-MM-ddTHH:mm:sszz",    // +0000  — no colon in offset
            "yyyy-MM-ddTHH:mm:ssZ",     // Z suffix
        ];

        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            // Some Akeneo versions return epoch-seconds integers for reference-entity records.
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out long seconds))
                return DateTimeOffset.FromUnixTimeSeconds(seconds);

            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString()!;

                // Let System.Text.Json try first (handles +00:00 and Z natively).
                if (reader.TryGetDateTimeOffset(out var dto))
                    return dto;

                // Fall back to explicit format parsing for variants like +0000.
                if (DateTimeOffset.TryParseExact(raw, _formats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var parsed))
                    return parsed;

                throw new JsonException(
                    $"Could not parse \"{raw}\" as a DateTimeOffset. " +
                    "Expected epoch-seconds, ISO-8601 with offset (+00:00 / +0000 / Z).");
            }

            throw new JsonException(
                $"Unexpected token {reader.TokenType} for a DateTimeOffset field.");
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>Preserved as an alias so existing <c>[JsonConverter(typeof(EpochSecondsConverter))]</c>
    /// attributes continue to compile without change.</summary>
    internal sealed class EpochSecondsConverter : AkeneoDateTimeOffsetConverter { }
}

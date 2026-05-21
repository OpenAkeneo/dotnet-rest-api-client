using System.Text.Json;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

/// <summary>
/// Offline regression tests pinning the JSON contract against Akeneo's real response
/// shapes. These do not require a live tenant — they protect against deserialization
/// bugs that depend on tenant-specific payloads slipping through the sandbox tests.
///
/// JSON payloads use ordinary string literals with explicit \u escapes so the source
/// file stays plain ASCII; embedding raw NBSP / zero-width / control characters in
/// raw string literals corrupts the file in some editors.
/// </summary>
public class ModelDeserializationTests
{
    [Fact]
    public void AkeneoAttribute_DeserializesInvalidCharactersInUnicode_AsArray()
    {
        // Akeneo returns invalid_characters_in_unicode as a JSON array of strings.
        // Regression: previously typed as string?, which threw on every non-null response.
        const string json =
            "{ \"code\": \"description\", \"type\": \"pim_catalog_textarea\", " +
            "\"invalid_characters_in_unicode\": [\"\\u00A0\", \"\\t\", \"\\u200B\"] }";

        var attr = JsonSerializer.Deserialize<AkeneoAttribute>(json);

        Assert.NotNull(attr);
        Assert.Equal("description", attr!.Code);
        Assert.NotNull(attr.InvalidCharactersInUnicode);
        Assert.Equal(3, attr.InvalidCharactersInUnicode!.Count);
        Assert.Equal(" ", attr.InvalidCharactersInUnicode[0]);
        Assert.Equal("\t", attr.InvalidCharactersInUnicode[1]);
    }

    [Fact]
    public void AkeneoAttribute_DeserializesInvalidCharactersInUnicode_AsEmptyArray()
    {
        const string json =
            "{ \"code\": \"name\", \"type\": \"pim_catalog_text\", " +
            "\"invalid_characters_in_unicode\": [] }";

        var attr = JsonSerializer.Deserialize<AkeneoAttribute>(json);

        Assert.NotNull(attr);
        Assert.NotNull(attr!.InvalidCharactersInUnicode);
        Assert.Empty(attr.InvalidCharactersInUnicode!);
    }

    [Fact]
    public void AkeneoAttribute_DeserializesInvalidCharactersInUnicode_WhenAbsent()
    {
        const string json =
            "{ \"code\": \"color\", \"type\": \"pim_catalog_simpleselect\" }";

        var attr = JsonSerializer.Deserialize<AkeneoAttribute>(json);

        Assert.NotNull(attr);
        Assert.Null(attr!.InvalidCharactersInUnicode);
    }

    [Fact]
    public void AkeneoAttribute_DeserializesFullRealisticPayload()
    {
        // A realistic Akeneo attribute response with the fields most likely to trip up
        // a wrong type — arrays, dicts, nullable scalars, plural fields.
        const string json = """
        {
          "code": "description",
          "type": "pim_catalog_textarea",
          "group": "marketing",
          "labels": { "en_US": "Description", "fr_FR": "Description" },
          "group_labels": { "en_US": "Marketing" },
          "sort_order": 4,
          "localizable": true,
          "scopable": false,
          "available_locales": ["en_US", "fr_FR"],
          "unique": false,
          "useable_as_grid_filter": true,
          "max_characters": 255,
          "validation_rule": null,
          "validation_regexp": null,
          "wysiwyg_enabled": true,
          "number_min": null,
          "number_max": null,
          "decimals_allowed": null,
          "negative_allowed": null,
          "date_min": null,
          "date_max": null,
          "metric_family": null,
          "default_metric_unit": null,
          "allowed_extensions": [],
          "max_file_size": null,
          "reference_data_name": null,
          "default_value": null,
          "invalid_characters_in_unicode": [" "]
        }
        """;

        var attr = JsonSerializer.Deserialize<AkeneoAttribute>(json);

        Assert.NotNull(attr);
        Assert.Equal("description", attr!.Code);
        Assert.Equal("pim_catalog_textarea", attr.Type);
        Assert.NotNull(attr.InvalidCharactersInUnicode);
        Assert.Single(attr.InvalidCharactersInUnicode!);
        Assert.NotNull(attr.AvailableLocales);
        Assert.Equal(2, attr.AvailableLocales!.Count);
        Assert.NotNull(attr.AllowedExtensions);
        Assert.Empty(attr.AllowedExtensions!);
    }

    [Fact]
    public void PermissionScope_DeserializesCanViewCanEdit_AsBooleans()
    {
        // Akeneo returns canView/canEdit as JSON booleans, not strings.
        // Regression: previously typed as string?, which threw on every response.
        const string json = """
        {
          "userUuid": "11111111-1111-1111-1111-111111111111",
          "channels": {
            "ecommerce": { "canView": true, "canEdit": false },
            "mobile":    { "canView": true, "canEdit": true  }
          }
        }
        """;

        var perms = JsonSerializer.Deserialize<UserChannelsPermissions>(json);

        Assert.NotNull(perms);
        Assert.NotNull(perms!.Channels);
        Assert.Equal(2, perms.Channels!.Count);
        Assert.Equal(true, perms.Channels["ecommerce"].CanView);
        Assert.Equal(false, perms.Channels["ecommerce"].CanEdit);
        Assert.Equal(true, perms.Channels["mobile"].CanEdit);
    }

    [Fact]
    public void ProductMediaFile_DeserializesLargeSize_WithoutOverflow()
    {
        // File sizes can exceed int.MaxValue (~2 GB). Regression: was int, must be long.
        const long bigSize = 3_500_000_000L;
        var json = $$"""
        {
          "code": "0/1/2/big.bin",
          "original_filename": "big.bin",
          "mime_type": "application/octet-stream",
          "size": {{bigSize}},
          "extension": "bin"
        }
        """;

        var media = JsonSerializer.Deserialize<ProductMediaFile>(json);

        Assert.NotNull(media);
        Assert.Equal(bigSize, media!.Size);
    }

    [Fact]
    public void ProductCompleteness_DeserializesNullData()
    {
        // Akeneo can return data: null for completeness entries that aren't computed.
        // Regression: was non-nullable int, must be int?.
        const string json = """
        {
          "scope": "ecommerce",
          "locale": "en_US",
          "data": null
        }
        """;

        var c = JsonSerializer.Deserialize<ProductCompleteness>(json);

        Assert.NotNull(c);
        Assert.Null(c!.Data);
    }
}

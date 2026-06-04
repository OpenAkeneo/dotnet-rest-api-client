using System.Net;
using System.Text.Json;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

public class ModelDeserializationTests
{
    #region ReferenceEntityAttribute

    [Fact]
    public void ReferenceEntityAttribute_LinkType_DeserializesReferenceEntityCode()
    {
        const string json = """
            {
                "code": "main_color",
                "type": "reference_entity_single_link",
                "reference_entity_code": "main_color",
                "value_per_locale": false,
                "value_per_channel": false,
                "is_required_for_completeness": false
            }
            """;

        var result = JsonSerializer.Deserialize<ReferenceEntityAttribute>(json);

        Assert.NotNull(result);
        Assert.Equal("main_color", result.Code);
        Assert.Equal("reference_entity_single_link", result.Type);
        Assert.Equal("main_color", result.ReferenceEntityCode);
    }

    [Fact]
    public void ReferenceEntityAttribute_NonLinkType_ReferenceEntityCodeIsNull()
    {
        const string json = """
            {
                "code": "description",
                "type": "text",
                "value_per_locale": true,
                "value_per_channel": false,
                "is_required_for_completeness": false
            }
            """;

        var result = JsonSerializer.Deserialize<ReferenceEntityAttribute>(json);

        Assert.NotNull(result);
        Assert.Equal("description", result.Code);
        Assert.Null(result.ReferenceEntityCode);
    }

    #endregion

    #region AkeneoAttribute

    [Fact]
    public void AkeneoAttribute_DefaultValue_DeserializesFromBoolString()
    {
        const string json = """
            {
                "code": "is_new",
                "type": "pim_catalog_boolean",
                "sort_order": 0,
                "localizable": false,
                "scopable": false,
                "unique": false,
                "useable_as_grid_filter": false,
                "default_value": "true",
                "is_main_identifier": false,
                "is_mandatory": false
            }
            """;

        var result = JsonSerializer.Deserialize<AkeneoAttribute>(json);

        Assert.NotNull(result);
        Assert.True(result.DefaultValue);
    }

    [Fact]
    public void AkeneoAttribute_DefaultValue_DeserializesFromBool()
    {
        const string json = """
            {
                "code": "is_new",
                "type": "pim_catalog_boolean",
                "sort_order": 0,
                "localizable": false,
                "scopable": false,
                "unique": false,
                "useable_as_grid_filter": false,
                "default_value": false,
                "is_main_identifier": false,
                "is_mandatory": false
            }
            """;

        var result = JsonSerializer.Deserialize<AkeneoAttribute>(json);

        Assert.NotNull(result);
        Assert.False(result.DefaultValue);
    }

    #endregion
}

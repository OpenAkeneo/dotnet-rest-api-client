using System.Net;
using System.Text.Json;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

/// <summary>
/// WP2 — converters and value helpers (see test-plan.md). Converters are internal, so they are
/// exercised through the public models that carry them: dates via <see cref="ProductUuid.Created"/>,
/// polymorphic data via <see cref="ProductValue.Data"/>, bool? via <see cref="AkeneoAttribute.DecimalsAllowed"/>
/// (number/string/null cases already locked in <see cref="FableReviewFixTests"/>).
/// AkeneoContextHelpers is reachable directly via InternalsVisibleTo.
/// </summary>
public class ConverterAndHelperTests
{
    // -------------------------------------------------------------------------
    // AkeneoDateTimeOffsetConverter — read formats
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("\"2024-03-15T10:30:00+00:00\"", 0)]
    [InlineData("\"2024-03-15T10:30:00Z\"", 0)]
    [InlineData("\"2024-03-15T10:30:00+0000\"", 0)]   // no-colon offset (AssetMediaFileLinkedData shape)
    [InlineData("\"2024-03-15T12:30:00+0200\"", 120)]
    [InlineData("1710498600", 0)]                      // epoch seconds (reference-entity records)
    public void DateConverter_ReadsAllAkeneoFormats(string createdJson, int offsetMinutes)
    {
        var product = JsonSerializer.Deserialize<ProductUuid>($$"""{"created":{{createdJson}}}""")!;

        Assert.NotNull(product.Created);
        Assert.Equal(TimeSpan.FromMinutes(offsetMinutes), product.Created!.Value.Offset);
        // All test inputs represent the same instant: 2024-03-15T10:30:00Z.
        Assert.Equal(new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero), product.Created.Value.ToUniversalTime());
    }

    [Fact]
    public void DateConverter_FractionalSeconds_ArePreserved() // SaaS responses carry microseconds
    {
        var product = JsonSerializer.Deserialize<ProductUuid>("""{"created":"2024-03-15T10:30:00.123456+00:00"}""")!;
        Assert.Equal(new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero).AddTicks(1234560), product.Created);
    }

    [Fact]
    public void DateConverter_Null_ReadsAsNull()
    {
        var product = JsonSerializer.Deserialize<ProductUuid>("""{"created":null}""")!;
        Assert.Null(product.Created);
    }

    [Theory]
    [InlineData("\"not-a-date\"")]
    [InlineData("\"2024-13-45T99:99:99\"")]
    [InlineData("true")]
    public void DateConverter_UnparseableValue_ThrowsJsonException(string createdJson)
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<ProductUuid>($$"""{"created":{{createdJson}}}"""));
    }

    [Fact]
    public void DateConverter_WriteRoundTrip_PreservesInstantAndOffset()
    {
        var original = new ProductUuid { Created = new DateTimeOffset(2024, 3, 15, 12, 30, 0, TimeSpan.FromHours(2)) };

        var roundTripped = JsonSerializer.Deserialize<ProductUuid>(JsonSerializer.Serialize(original))!;

        Assert.Equal(original.Created, roundTripped.Created);
        Assert.Equal(TimeSpan.FromHours(2), roundTripped.Created!.Value.Offset);
    }

    // -------------------------------------------------------------------------
    // PolymorphicDataConverter — read produces the documented runtime types
    // -------------------------------------------------------------------------

    [Fact]
    public void PolymorphicData_ReadsScalarsWithDocumentedRuntimeTypes()
    {
        Assert.IsType<string>(DataOf("\"hello\""));
        Assert.IsType<long>(DataOf("42"));
        Assert.IsType<double>(DataOf("12.5"));
        Assert.IsType<bool>(DataOf("true"));
        Assert.Null(DataOf("null"));
    }

    [Fact]
    public void PolymorphicData_ReadsArraysAndNestedObjects()
    {
        var list = Assert.IsType<List<object?>>(DataOf("""["red","blue"]"""));
        Assert.Equal(["red", "blue"], list.Cast<string>());

        var dict = Assert.IsType<Dictionary<string, object?>>(DataOf("""{"amount":"12.5000","unit":"KILOGRAM"}"""));
        Assert.Equal("KILOGRAM", dict["unit"]);

        var nested = Assert.IsType<List<object?>>(DataOf("""[{"amount":42,"currency":"EUR"}]"""));
        var entry = Assert.IsType<Dictionary<string, object?>>(nested[0]);
        Assert.Equal(42L, entry["amount"]);
    }

    // NOTE: integers beyond long round through double and may lose precision (reviewed finding
    // 1.11, accepted); this test documents the behaviour rather than endorsing it.
    [Fact]
    public void PolymorphicData_HugeNumber_DegradesToDouble()
    {
        Assert.IsType<double>(DataOf("79228162514264337593543950336"));
    }

    [Fact]
    public void PolymorphicData_WriteRoundTrips_MetricAndPriceValues()
    {
        var json = """
            {"locale":null,"scope":null,"data":{"amount":"12.5000","unit":"KILOGRAM"}}
            """;
        var value = JsonSerializer.Deserialize<ProductValue>(json)!;

        var serialized = JsonSerializer.Serialize(value);
        var back = JsonSerializer.Deserialize<ProductValue>(serialized)!;

        var metric = back.GetData<MetricValue>()!;
        Assert.Equal("12.5000", metric.Amount);
        Assert.Equal("KILOGRAM", metric.Unit);

        var priceJson = """{"data":[{"amount":"42.00","currency":"EUR"}]}""";
        var price = JsonSerializer.Deserialize<ProductValue>(priceJson)!;
        var priceBack = JsonSerializer.Deserialize<ProductValue>(JsonSerializer.Serialize(price))!;
        var priceList = Assert.IsType<List<object?>>(priceBack.Data);
        var priceEntry = Assert.IsType<Dictionary<string, object?>>(priceList[0]);
        Assert.Equal("EUR", priceEntry["currency"]);
    }

    private static object? DataOf(string dataJson)
        => JsonSerializer.Deserialize<ProductValue>($$"""{"data":{{dataJson}}}""")!.Data;

    // -------------------------------------------------------------------------
    // ProductValue typed accessors
    // -------------------------------------------------------------------------

    [Fact]
    public void GetData_DeserializesToRequestedType()
    {
        var value = JsonSerializer.Deserialize<ProductValue>("""{"data":{"amount":"7.5","unit":"GRAM"}}""")!;
        var metric = value.GetData<MetricValue>()!;
        Assert.Equal("7.5", metric.Amount);
        Assert.Equal("GRAM", metric.Unit);
    }

    [Fact]
    public void GetLinkedData_DeserializesAssetShareLinks()
    {
        var json = """
            {"data":["asset1"],"linked_data":{"asset1":{"share_links":[
                {"attribute":"media","scope":null,"locale":null,"_links":{"self":{"href":"https://cdn.example/img.png"}}}
            ]}}}
            """;
        var value = JsonSerializer.Deserialize<ProductValue>(json)!;

        var linked = value.GetLinkedData<Dictionary<string, AssetCollectionLinkedDataEntry>>()!;
        Assert.Equal("https://cdn.example/img.png", linked["asset1"].ShareLinks![0].Links!.Self!.Href);
    }

    [Theory]
    [InlineData("""{"data":"plain"}""", "plain")]
    [InlineData("""{"data":42}""", "42")]
    [InlineData("""{"data":["first","second"]}""", "first")]
    [InlineData("""{"data":[null,"second"]}""", "second")]
    [InlineData("""{"data":true}""", null)]  // bool → null (documented)
    [InlineData("""{"data":null}""", null)]
    public void GetStringData_CoversDocumentedShapes(string json, string? expected)
    {
        var value = JsonSerializer.Deserialize<ProductValue>(json)!;
        Assert.Equal(expected, value.GetStringData());
    }

    // -------------------------------------------------------------------------
    // Model gap found during WP4 prep: AssetAttribute.value_per_channel
    // -------------------------------------------------------------------------

    [Fact]
    public void AssetAttribute_ValuePerChannel_RoundTrips()
    {
        var attribute = JsonSerializer.Deserialize<AssetAttribute>(
            """{"code":"media","type":"media_file","value_per_locale":false,"value_per_channel":true}""")!;
        Assert.True(attribute.ValuePerChannel);

        var serialized = JsonSerializer.Serialize(attribute);
        Assert.Contains("\"value_per_channel\":true", serialized);
    }

    // -------------------------------------------------------------------------
    // AkeneoContextHelpers (via InternalsVisibleTo)
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildQueryString_EscapesKeysAndValues()
    {
        var qs = AkeneoContextHelpers.BuildQueryString(new Dictionary<string, string>
        {
            ["search"] = """{"enabled":[{"operator":"="}]}""",
            ["a b"] = "x&y=z+1"
        });

        Assert.StartsWith("?", qs);
        Assert.Contains("search=%7B%22enabled%22", qs);
        Assert.Contains("a%20b=x%26y%3Dz%2B1", qs);
    }

    [Fact]
    public void BuildQueryString_EmptyOrNull_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, AkeneoContextHelpers.BuildQueryString(null));
        Assert.Equal(string.Empty, AkeneoContextHelpers.BuildQueryString(new Dictionary<string, string>()));
    }

    [Fact]
    public void ParseHalResponse_NoEmbedded_ReturnsEmptyListWithLinks()
    {
        var (links, items) = AkeneoContextHelpers.ParseHalResponse<ProductUuid>(
            """{"_links":{"self":{"href":"/x"}}}""", "/x");

        Assert.Empty(items);
        Assert.Equal("/x", links!.Self!.Href);
    }

    [Fact]
    public void DeserializeOrThrow_MalformedJson_ThrowsAkeneoApiExceptionWithBody()
    {
        var ex = Assert.Throws<AkeneoApiException>(() =>
            AkeneoContextHelpers.DeserializeOrThrow<ProductUuid>("{not json", "/api/rest/v1/products-uuid/x"));

        Assert.Equal(HttpStatusCode.OK, ex.StatusCode);
        Assert.Equal("{not json", ex.ResponseBody);
        Assert.Contains("/products-uuid/x", ex.RequestUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    public void DeserializeOrThrow_EmptyOrNullJson_ThrowsAkeneoApiException(string body)
    {
        Assert.Throws<AkeneoApiException>(() =>
            AkeneoContextHelpers.DeserializeOrThrow<ProductUuid>(body, "/url"));
    }

    [Theory]
    [InlineData(0, 50, true)]
    [InlineData(1, 0, true)]
    [InlineData(1, 101, true)]
    [InlineData(1, 1, false)]
    [InlineData(999, 100, false)]
    public void ValidatePagination_EnforcesBounds(int page, int limit, bool shouldThrow)
    {
        if (shouldThrow)
            Assert.Throws<ArgumentOutOfRangeException>(() => AkeneoContextHelpers.ValidatePagination(page, limit));
        else
            AkeneoContextHelpers.ValidatePagination(page, limit);
    }

    [Theory]
    [InlineData("https://x/api?search_after=abc", "abc")]
    [InlineData("https://x/api?search_after=abc&limit=100", "abc")]
    [InlineData("https://x/api?limit=100&search_after=a%2Fb%20c", "a/b c")]
    [InlineData("https://x/api?limit=100", null)]
    public void ExtractSearchAfter_HandlesPositionAndEscaping(string url, string? expected)
    {
        Assert.Equal(expected, AkeneoContextHelpers.ExtractSearchAfter(url));
    }

    [Theory]
    [InlineData("https://x/api/rest/v1/products-uuid/gen-1", "gen-1")]
    [InlineData("/api/rest/v1/products-uuid/gen-2", "gen-2")]
    [InlineData("/api/rest/v1/products-uuid/gen-3/", "gen-3")]
    [InlineData("/api/rest/v1/products-uuid/gen%204?x=1", "gen 4")]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void ExtractLastPathSegment_HandlesShapes(string? location, string? expected)
    {
        Assert.Equal(expected, AkeneoContextHelpers.ExtractLastPathSegment(location));
    }
}

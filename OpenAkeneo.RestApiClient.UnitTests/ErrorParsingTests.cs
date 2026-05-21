using System.Net;

namespace OpenAkeneo.RestApiClient.UnitTests;

public class ErrorParsingTests
{
    [Fact]
    public async Task JsonBodyWithMessageField_UsesMessageAsApiMessage()
    {
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Json(HttpStatusCode.NotFound, """{"code":404,"message":"Product not found."}"""));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpGetAsync("/api/rest/v1/products/missing", TestContext.Current.CancellationToken));

        Assert.Equal("Product not found.", ex.ApiMessage);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task NonJsonBody_UsesRawBodyAsApiMessage()
    {
        // 403 is non-transient so Polly will not retry it
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.Forbidden, "Access denied"));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpGetAsync("/api/rest/v1/products", TestContext.Current.CancellationToken));

        Assert.Equal("Access denied", ex.ApiMessage);
    }

    [Fact]
    public async Task NonJsonBodyOver500Chars_IsTruncatedWithEllipsis()
    {
        var longBody = new string('x', 600);
        // 403 is non-transient so Polly will not retry it
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.Forbidden, longBody));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpGetAsync("/api/rest/v1/products", TestContext.Current.CancellationToken));

        Assert.Equal(501, ex.ApiMessage.Length); // 500 chars + "…"
        Assert.EndsWith("…", ex.ApiMessage);
    }

    [Fact]
    public async Task EmptyBody_ReturnsEmptyBodyPlaceholder()
    {
        // 403 is non-transient so Polly will not retry it
        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Text(HttpStatusCode.Forbidden, ""));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpGetAsync("/api/rest/v1/products", TestContext.Current.CancellationToken));

        Assert.Equal("(empty response body)", ex.ApiMessage);
    }

    [Fact]
    public async Task ValidationError422_PopulatesFieldErrors()
    {
        // End-to-end test: a real 422 body flows through ParseAkeneoError and surfaces
        // as typed FieldErrors on the exception — not just buried in ResponseBody.
        const string body = """
            {
              "code": 422,
              "message": "Validation failed.",
              "errors": [
                { "property": "values.name",  "message": "This value is too long." },
                { "property": "values.price", "message": "This value must be a number." }
              ]
            }
            """;

        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Json(HttpStatusCode.UnprocessableEntity, body));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpPatchAsync("/api/rest/v1/products/p1", "{}", TestContext.Current.CancellationToken));

        Assert.Equal("Validation failed.", ex.ApiMessage);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, ex.StatusCode);
        Assert.NotNull(ex.FieldErrors);
        Assert.Equal(2, ex.FieldErrors!.Count);
        Assert.Equal("values.name",  ex.FieldErrors[0].Property);
        Assert.Equal("This value is too long.", ex.FieldErrors[0].Message);
        Assert.Equal("values.price", ex.FieldErrors[1].Property);
        Assert.Equal("This value must be a number.", ex.FieldErrors[1].Message);
    }

    [Fact]
    public async Task ValidationError422_NoErrorsArray_FieldErrorsIsNull()
    {
        // Some 422s from Akeneo omit the errors array entirely.
        const string body = """{"code":422,"message":"Validation failed."}""";

        var handler = new FakeHttpHandler(
            FakeHttpHandler.TokenResponse(),
            FakeHttpHandler.Json(HttpStatusCode.UnprocessableEntity, body));

        var svc = Helpers.BuildService(handler);
        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => svc.HttpPatchAsync("/api/rest/v1/products/p1", "{}", TestContext.Current.CancellationToken));

        Assert.Equal("Validation failed.", ex.ApiMessage);
        Assert.Null(ex.FieldErrors);
    }
}

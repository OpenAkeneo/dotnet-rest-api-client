using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.UnitTests;

internal static class Helpers
{
    internal static AkeneoRestApiSettings Settings(string baseUrl = "https://akeneo.test") => new()
    {
        ClientId = "client-id",
        ClientSecret = "client-secret",
        Username = "user",
        Password = "pass",
        RestApiUrl = baseUrl
    };

    internal static AkeneoRestApiService BuildService(FakeHttpHandler handler)
        => new(new HttpClient(handler), Settings());

    internal static AkeneoContext BuildContext(FakeHttpHandler handler)
        => new(BuildService(handler));

    /// <summary>
    /// Returns a minimal HAL list JSON with a single embedded item under _embedded.items.
    /// </summary>
    internal static string HalList(string itemJson)
        => $$$"""{"_links":{"self":{"href":"/api/rest/v1/test"}},"_embedded":{"items":[{{{itemJson}}}]}}""";
}

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
}

using OpenAkeneo.RestApiClient;
using OpenAkeneo.RestApiClient.Models;
using Microsoft.Extensions.Configuration;

namespace OpenAkeneo.RestApiClient.SandboxTests;

/// <summary>
/// Shared fixture providing a single initialized AkeneoContext and AkeneoRestApiService
/// per test class. Use with IClassFixture&lt;TestBase&gt;.
/// </summary>
public class TestBase : IAsyncLifetime
{
    public readonly AkeneoContext Context;
    public readonly AkeneoRestApiService ApiService;

    private readonly HttpClient _httpClient;

    public TestBase()
    {
        // appsettings.json is optional (gitignored). Set credentials via dotnet user-secrets:
        //   dotnet user-secrets set "AkeneoSettings:ClientId" "your_client_id"
        //   dotnet user-secrets set "AkeneoSettings:ClientSecret" "your_client_secret"
        //   dotnet user-secrets set "AkeneoSettings:Username" "your_api_username"
        //   dotnet user-secrets set "AkeneoSettings:Password" "your_api_password"
        //   dotnet user-secrets set "AkeneoSettings:RestApiUrl" "https://your-instance.cloud.akeneo.com"
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<TestBase>(optional: true)
            .Build();

        var settings = configuration.GetSection("AkeneoSettings").Get<AkeneoRestApiSettings>()
            ?? throw new InvalidOperationException(
                "AkeneoSettings are missing. Copy appsettings.example.json to appsettings.json and fill in credentials, " +
                "or set them via dotnet user-secrets (see CONTRIBUTING.md).");

        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        ApiService = new AkeneoRestApiService(_httpClient, settings);
        Context = new AkeneoContext(ApiService);
    }

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }
}

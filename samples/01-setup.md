# Setup and Initialisation

Load settings from a credential-based source or `appsettings.json` and create an `AkeneoContext`. Copy `appsettings.example.json` to `appsettings.json` and fill in your credentials before running.

```csharp
using Microsoft.Extensions.Configuration;
using OpenAkeneo.RestApiClient;
using OpenAkeneo.RestApiClient.Models;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = configuration.GetSection("AkeneoSettings").Get<AkeneoRestApiSettings>()
    ?? throw new InvalidOperationException("AkeneoSettings missing from appsettings.json");

var context = new AkeneoContext(settings);

var info = await context.GetSystemInformationAsync();
Console.WriteLine($"Connected to {context.ConnectionName} — Akeneo {info.Version}");
```

The `AkeneoContext` constructor accepts `AkeneoRestApiSettings` directly and handles `HttpClient` creation internally. Optionally, pass an `AkeneoRestApiService` instance if you need to configure logging or share an `HttpClient`:

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var logger = loggerFactory.CreateLogger<AkeneoRestApiService>();

var service = new AkeneoRestApiService(new HttpClient(), settings, logger);
var context = new AkeneoContext(service);
```

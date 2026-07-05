using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient
{
    /// <summary>Extension methods for registering Akeneo client services with the DI container.</summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="AkeneoRestApiService"/> and <see cref="AkeneoContext"/> with the DI container.
        /// Uses the typed-client pattern so <see cref="System.Net.Http.IHttpClientFactory"/> manages handler
        /// rotation, preventing stale-DNS and socket exhaustion in long-running applications.
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="settings">The Akeneo connection settings.</param>
        public static IServiceCollection AddAkeneoClient(this IServiceCollection services, AkeneoRestApiSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            settings.Validate();

            services.AddSingleton(settings);

            // One token cache per connection: the typed client below is transient (so
            // IHttpClientFactory can rotate handlers), but every instance shares this cache,
            // ensuring a single OAuth token is fetched and reused across resolutions.
            services.AddSingleton<AkeneoTokenCache>();

            services.AddHttpClient<AkeneoRestApiService>((sp, client) => { })
                    .Services
                    .AddTransient<IAkeneoRestApiService>(sp => sp.GetRequiredService<AkeneoRestApiService>());

            services.AddTransient<AkeneoContext>(sp =>
                new AkeneoContext(sp.GetRequiredService<AkeneoRestApiService>()));

            return services;
        }
    }
}

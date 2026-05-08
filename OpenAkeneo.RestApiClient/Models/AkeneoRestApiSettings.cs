namespace OpenAkeneo.RestApiClient.Models
{
    /// <summary>Configuration settings required to connect to an Akeneo REST API instance.</summary>
    public class AkeneoRestApiSettings
    {

        /// <summary>Logical identifier for this connection. Defaults to <see cref="Username"/> if not set.</summary>
        public string? Id { get; init; }

        /// <summary>Human-readable name for this connection. Defaults to <see cref="RestApiUrl"/> if not set.</summary>
        public string? Name { get; init; }

        /// <summary>OAuth2 client ID issued by Akeneo.</summary>
        public required string ClientId { get; init; }

        /// <summary>OAuth2 client secret issued by Akeneo.</summary>
        public required string ClientSecret { get; init; }

        /// <summary>Akeneo user account username used for authentication.</summary>
        public required string Username { get; init; }

        /// <summary>Akeneo user account password used for authentication.</summary>
        public required string Password { get; init; }

        /// <summary>Base URL of the Akeneo REST API (e.g. <c>https://my-akeneo.example.com</c>).</summary>
        public required string RestApiUrl { get; init; }

        /// <summary>Optional file path used to cache the OAuth2 token between sessions.</summary>
        public string? TokenFilePath { get; init; }

        /// <summary>
        /// Validates that all required fields are non-empty. Call at SDK boot to fail fast
        /// rather than encountering cryptic errors during token pipeline execution.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when any required field is null or whitespace.</exception>
        public void Validate()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ClientId, nameof(ClientId));
            ArgumentException.ThrowIfNullOrWhiteSpace(ClientSecret, nameof(ClientSecret));
            ArgumentException.ThrowIfNullOrWhiteSpace(Username, nameof(Username));
            ArgumentException.ThrowIfNullOrWhiteSpace(Password, nameof(Password));
            ArgumentException.ThrowIfNullOrWhiteSpace(RestApiUrl, nameof(RestApiUrl));
        }

    }
}

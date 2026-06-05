using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Catalog for Apps

        // TODO: Catalogs https://api.akeneo.com/api-reference.html#Catalogs

        // TODO: Catalog products https://api.akeneo.com/api-reference.html#Catalogproducts

        // TODO: Mapping schema for products https://api.akeneo.com/api-reference.html#Mappingschemaforproducts

        #endregion


        #region Utilities

        // TODO: Overview https://api.akeneo.com/api-reference.html#Overview

        // TODO: Authentication https://api.akeneo.com/api-reference.html#Authentication

        /// <summary>Returns system information for the Akeneo instance (version, edition, etc.).</summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="SystemInformation"/> object.</returns>
        public async Task<SystemInformation> GetSystemInformationAsync(CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/system-information";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<SystemInformation>(responseString, url);
        }

        #endregion


        #region Extensions

        // TODO: UI Extensions

        #endregion


        #region Permissions, Overview & Extensions

        /// <summary>Returns the channel permissions for a given user.</summary>
        /// <param name="userUuid">The user UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="UserChannelsPermissions"/> object.</returns>
        public async Task<UserChannelsPermissions> GetUserChannelsPermissionsAsync(string userUuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/permissions/{Uri.EscapeDataString(userUuid)}/channels";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<UserChannelsPermissions>(responseString, url);
        }

        /// <summary>Returns the locale permissions for a given user.</summary>
        /// <param name="userUuid">The user UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="UserLocalesPermissions"/> object.</returns>
        public async Task<UserLocalesPermissions> GetUserLocalesPermissionsAsync(string userUuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/permissions/{Uri.EscapeDataString(userUuid)}/locales";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<UserLocalesPermissions>(responseString, url);
        }

        /// <summary>Returns the Akeneo REST API overview (root HAL response with top-level navigation links).</summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An <see cref="ApiOverview"/> object.</returns>
        public async Task<ApiOverview> GetApiOverviewAsync(CancellationToken ct = default)
        {
            var url = $"/api/rest/v1";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<ApiOverview>(responseString, url);
        }

        /// <summary>
        /// Returns all UI extensions associated with the current token.
        /// Unlike most Akeneo list endpoints, <c>GET /ui-extensions</c> is not paginated and
        /// returns a bare JSON array, so this method returns the full list directly.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="Extension"/> objects.</returns>
        public async Task<List<Extension>> GetExtensionListAsync(CancellationToken ct = default)
        {
            var url = "/api/rest/v1/ui-extensions";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<List<Extension>>(responseString, url);
        }

        /// <summary>Returns a page of Data Architect modelization suggestions.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ModelizationSuggestionList"/> with HAL navigation links.</returns>
        public async Task<ModelizationSuggestionList> GetModelizationSuggestionListAsync(int page = 1, int limit = 100, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/data-model-designer/modelization-suggestions?page={page}&limit={limit}";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var responseJson = AkeneoContextHelpers.DeserializeOrThrow<HalResponse>(responseString, url);

            var result = new ModelizationSuggestionList { Links = responseJson.Links };

            if (responseJson.Embedded != null && responseJson.Embedded.TryGetValue("items", out var itemsElement))
                result.Suggestions = itemsElement.EnumerateArray().Select(x => x.Deserialize<ModelizationSuggestion>()!).ToList();

            return result;
        }

        /// <summary>Returns a single Data Architect modelization suggestion by its UUID.</summary>
        /// <param name="uuid">The suggestion UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ModelizationSuggestion"/>.</returns>
        public async Task<ModelizationSuggestion> GetModelizationSuggestionAsync(string uuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/data-model-designer/modelization-suggestion/{Uri.EscapeDataString(uuid)}";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<ModelizationSuggestion>(responseString, url);
        }

        #endregion


    }
}

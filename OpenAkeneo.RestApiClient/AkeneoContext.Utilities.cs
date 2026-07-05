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

        /// <summary>Creates a new UI extension and returns the created entity.</summary>
        /// <param name="extension">The extension to create (name, type, position, configuration…).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created <see cref="Extension"/> (including its server-assigned UUID).</returns>
        public async Task<Extension> CreateExtensionAsync(Extension extension, CancellationToken ct = default)
        {
            const string url = "/api/rest/v1/ui-extensions";
            var body = JsonSerializer.Serialize(extension);
            var responseString = await _service.HttpPostAsync(url, body, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<Extension>(responseString, url);
        }

        /// <summary>Updates a UI extension and returns the updated entity.</summary>
        /// <param name="uuid">The extension UUID.</param>
        /// <param name="extension">The properties to update.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="Extension"/>.</returns>
        public async Task<Extension> UpdateExtensionAsync(string uuid, Extension extension, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/ui-extensions/{Uri.EscapeDataString(uuid)}";
            var body = JsonSerializer.Serialize(extension);
            var responseString = await _service.HttpPatchAsync(url, body, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<Extension>(responseString, url);
        }

        /// <summary>Deletes a UI extension.</summary>
        /// <param name="uuid">The extension UUID to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task DeleteExtensionAsync(string uuid, CancellationToken ct = default)
        {
            await _service.HttpDeleteAsync($"/api/rest/v1/ui-extensions/{Uri.EscapeDataString(uuid)}", ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a UI extension with a file upload (<c>sdk_script</c> extensions). String fields use
        /// the multipart form names from the Akeneo API reference, e.g. <c>name</c>, <c>version</c>,
        /// <c>configuration[default_label]</c>, <c>credentials[0][code]</c>.
        /// </summary>
        /// <param name="uuid">The extension UUID.</param>
        /// <param name="fileBytes">The updated script file content.</param>
        /// <param name="fileName">Original file name (e.g. <c>extension.js</c>).</param>
        /// <param name="fields">Additional multipart string fields to update.</param>
        /// <param name="contentType">MIME type of the file (defaults to <c>text/javascript</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw JSON response body (the updated extension).</returns>
        public async Task<string> UpdateExtensionWithFileAsync(string uuid, byte[] fileBytes, string fileName, IReadOnlyDictionary<string, string>? fields = null, string contentType = "text/javascript", CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/ui-extensions/{Uri.EscapeDataString(uuid)}";
            return await _service.HttpPostMultipartForBodyAsync(url, "file", fileBytes, fileName, contentType, fields, ct).ConfigureAwait(false);
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
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ModelizationSuggestion>(responseString, url);

            return new ModelizationSuggestionList { Links = links, Suggestions = items };
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

        /// <summary>
        /// Submits an attribute modelization suggestion to the Data Architect agent.
        /// The request body is passed as raw JSON (fields per the Akeneo API reference:
        /// <c>source</c>, <c>description</c>, <c>code</c>, <c>type</c>, <c>additional_comments</c>).
        /// </summary>
        /// <param name="requestJson">Raw JSON request body.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw JSON response body (the created suggestion).</returns>
        public async Task<string> SuggestModelizationAttributeAsync(string requestJson, CancellationToken ct = default)
        {
            return await _service.HttpPostAsync("/api/rest/v1/data-model-designer/modelization-suggestion/attribute", requestJson, ct).ConfigureAwait(false);
        }

        /// <summary>Approves a modelization suggestion, optionally overriding attribute properties.</summary>
        /// <param name="uuid">The suggestion UUID.</param>
        /// <param name="overridesJson">Optional raw JSON object with attribute-property overrides.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw JSON response body.</returns>
        public async Task<string> ApproveModelizationSuggestionAsync(string uuid, string? overridesJson = null, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/data-model-designer/modelization-suggestion/{Uri.EscapeDataString(uuid)}/approve";
            return await _service.HttpPostAsync(url, overridesJson, (Dictionary<string, string>?)null, ct).ConfigureAwait(false);
        }

        /// <summary>Declines a modelization suggestion.</summary>
        /// <param name="uuid">The suggestion UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task DeclineModelizationSuggestionAsync(string uuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/data-model-designer/modelization-suggestion/{Uri.EscapeDataString(uuid)}/decline";
            await _service.HttpPostAsync(url, null, (Dictionary<string, string>?)null, ct).ConfigureAwait(false);
        }

        #endregion


    }
}

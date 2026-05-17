using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Reference entity

        /// <summary>Returns all reference entities as a materialised list by following keyset pagination automatically.</summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="ReferenceEntity"/> objects.</returns>
        public async Task<List<ReferenceEntity>> GetReferenceEntityListFullAsync(CancellationToken ct = default)
        {
            var list = new List<ReferenceEntity>();
            string? cursor = null;
            do
            {
                var page = await GetReferenceEntityListAsync(cursor, ct).ConfigureAwait(false);
                list.AddRange(page.ReferenceEntities);
                cursor = page.Links?.Next?.Href is not null
                    ? ExtractSearchAfter(page.Links.Next.Href)
                    : null;
            } while (cursor is not null);
            return list;
        }

        private static string? ExtractSearchAfter(string url)
        {
            var idx = url.IndexOf("search_after=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            idx += "search_after=".Length;
            var end = url.IndexOf('&', idx);
            return Uri.UnescapeDataString(end < 0 ? url[idx..] : url[idx..end]);
        }

        /// <summary>Returns a page of reference entities, optionally starting after a cursor value.</summary>
        /// <param name="searchAfter">Cursor value for keyset pagination.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ReferenceEntityList"/> with HAL navigation links.</returns>
        public async Task<ReferenceEntityList> GetReferenceEntityListAsync(string? searchAfter = null, CancellationToken ct = default)
        {
            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(searchAfter))
                queryParameters.Add("search_after", searchAfter);

            return await GetReferenceEntityListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a reference entity page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ReferenceEntityList"/> with HAL navigation links.</returns>
        public async Task<ReferenceEntityList> GetReferenceEntityListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ReferenceEntity>(responseString, url);

            return new ReferenceEntityList { Links = links, ReferenceEntities = items };
        }

        /// <summary>Returns a single reference entity by its code.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ReferenceEntity"/>.</returns>
        public async Task<ReferenceEntity> GetReferenceEntityAsync(string referenceEntityCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntity>(responseString, url);
        }

        /// <summary>Creates or updates a reference entity via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="referenceEntity">The reference entity to create or update. <see cref="ReferenceEntity.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="ReferenceEntity"/> as returned by the API.</returns>
        public async Task<ReferenceEntity> CreateOrUpdateReferenceEntityAsync(ReferenceEntity referenceEntity, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntity.Code)}";
            var body = JsonSerializer.Serialize(referenceEntity);
            return await PatchAndFetchAsync(url, body, () => GetReferenceEntityAsync(referenceEntity.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Reference entity attribute

        /// <summary>Returns all attributes for a given reference entity.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ReferenceEntityAttributeList"/>.</returns>
        public async Task<ReferenceEntityAttributeList> GetReferenceEntityAttributeListAsync(string referenceEntityCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityAttributeList>(responseString, url);
        }

        /// <summary>Returns a single attribute for a reference entity.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ReferenceEntityAttribute"/>.</returns>
        public async Task<ReferenceEntityAttribute> GetReferenceEntityAttributeAsync(string referenceEntityCode, string attributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attributeCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityAttribute>(responseString, url);
        }

        /// <summary>Creates or updates a reference entity attribute via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="attribute">The attribute to create or update.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="ReferenceEntityAttribute"/> as returned by the API.</returns>
        public async Task<ReferenceEntityAttribute> CreateOrUpdateReferenceEntityAttributeAsync(string referenceEntityCode, ReferenceEntityAttribute attribute, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attribute.Code)}";
            var body = JsonSerializer.Serialize(attribute);
            return await PatchAndFetchAsync(url, body, () => GetReferenceEntityAttributeAsync(referenceEntityCode, attribute.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Reference entity attribute option

        /// <summary>Returns all options for a given reference entity attribute.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ReferenceEntityAttributeOptionList"/>.</returns>
        public async Task<ReferenceEntityAttributeOptionList> GetReferenceEntityAttributeOptionListAsync(string referenceEntityCode, string attributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attributeCode)}/options";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityAttributeOptionList>(responseString, url);
        }

        /// <summary>Returns a single option for a reference entity attribute.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="optionCode">The option code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ReferenceEntityAttributeOption"/>.</returns>
        public async Task<ReferenceEntityAttributeOption> GetReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, string optionCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attributeCode)}/options/{Uri.EscapeDataString(optionCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityAttributeOption>(responseString, url);
        }

        /// <summary>Creates or updates a reference entity attribute option via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="option">The option to create or update.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="ReferenceEntityAttributeOption"/> as returned by the API.</returns>
        public async Task<ReferenceEntityAttributeOption> CreateOrUpdateReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, ReferenceEntityAttributeOption option, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attributeCode)}/options/{Uri.EscapeDataString(option.Code)}";
            var body = JsonSerializer.Serialize(option);
            return await PatchAndFetchAsync(url, body, () => GetReferenceEntityAttributeOptionAsync(referenceEntityCode, attributeCode, option.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Reference entity record

        /// <summary>Streams all records for a given reference entity, following keyset pagination automatically.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="channel">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="searchAfter">Optional cursor to resume streaming from a known position.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An async stream of <see cref="ReferenceEntityRecord"/> objects.</returns>
        public async IAsyncEnumerable<ReferenceEntityRecord> StreamReferenceEntityRecordsAsync(
            string referenceEntityCode,
            string? search = null,
            string? channel = null,
            string? locales = null,
            string? searchAfter = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            string? cursor = searchAfter;
            do
            {
                var page = await GetReferenceEntityRecordListAsync(referenceEntityCode, search, channel, locales, cursor, ct).ConfigureAwait(false);
                foreach (var record in page.ReferenceEntityRecords)
                    yield return record;
                cursor = page.Links?.Next?.Href is not null
                    ? ExtractSearchAfter(page.Links.Next.Href)
                    : null;
            } while (cursor is not null);
        }

        /// <summary>Returns all records for a given reference entity as a materialised list by following keyset pagination automatically.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="channel">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="searchAfter">Optional cursor to start from a known position.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="ReferenceEntityRecord"/> objects.</returns>
        public async Task<List<ReferenceEntityRecord>> GetReferenceEntityRecordListFullAsync(
            string referenceEntityCode,
            string? search = null,
            string? channel = null,
            string? locales = null,
            string? searchAfter = null,
            CancellationToken ct = default)
        {
            var list = new List<ReferenceEntityRecord>();
            await foreach (var record in StreamReferenceEntityRecordsAsync(referenceEntityCode, search, channel, locales, searchAfter, ct: ct))
                list.Add(record);
            return list;
        }

        /// <summary>Returns a page of records for a given reference entity.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="channel">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="searchAfter">Cursor for keyset pagination.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ReferenceEntityRecordList"/> with HAL navigation links.</returns>
        public async Task<ReferenceEntityRecordList> GetReferenceEntityRecordListAsync(string referenceEntityCode, string? search = null, string? channel = null, string? locales = null, string? searchAfter = null, CancellationToken ct = default)
        {
            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            if (!string.IsNullOrEmpty(channel))
                queryParameters.Add("channel", channel);

            if (!string.IsNullOrEmpty(locales))
                queryParameters.Add("locales", locales);

            if (!string.IsNullOrEmpty(searchAfter))
                queryParameters.Add("search_after", searchAfter);

            return await GetReferenceEntityRecordListAsync(referenceEntityCode, queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a reference entity record page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ReferenceEntityRecordList"/> with HAL navigation links.</returns>
        public async Task<ReferenceEntityRecordList> GetReferenceEntityRecordListAsync(string referenceEntityCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/records";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ReferenceEntityRecord>(responseString, url);

            return new ReferenceEntityRecordList { Links = links, ReferenceEntityRecords = items };
        }

        /// <summary>Returns a single record for a reference entity.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="recordCode">The record code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ReferenceEntityRecord"/>.</returns>
        public async Task<ReferenceEntityRecord> GetReferenceEntityRecordAsync(string referenceEntityCode, string recordCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/records/{Uri.EscapeDataString(recordCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityRecord>(responseString, url);
        }

        /// <summary>Creates or updates a reference entity record via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="record">The record to create or update. <see cref="ReferenceEntityRecord.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="ReferenceEntityRecord"/> as returned by the API.</returns>
        public async Task<ReferenceEntityRecord> CreateOrUpdateReferenceEntityRecordAsync(string referenceEntityCode, ReferenceEntityRecord record, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/records/{Uri.EscapeDataString(record.Code)}";
            var body = JsonSerializer.Serialize(record);
            return await PatchAndFetchAsync(url, body, () => GetReferenceEntityRecordAsync(referenceEntityCode, record.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Reference entity media file

        /// <summary>Downloads the binary content of a reference entity media file.</summary>
        /// <param name="mediaFileCode">The media file code as returned by the record attribute value.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw file bytes.</returns>
        public async Task<byte[]> DownloadReferenceEntityMediaFileAsync(string mediaFileCode, CancellationToken ct = default)
        {
            var codeEscaped = string.Join("/", mediaFileCode.Split('/').Select(Uri.EscapeDataString));
            return await _service.HttpGetBytesAsync($"/api/rest/v1/reference-entities-media-files/{codeEscaped}", ct).ConfigureAwait(false);
        }

        /// <summary>Uploads a reference entity media file and returns the created file code from the response.</summary>
        /// <param name="fileBytes">Raw file bytes.</param>
        /// <param name="fileName">Original file name (e.g. <c>portrait.jpg</c>).</param>
        /// <param name="contentType">MIME type (e.g. <c>image/jpeg</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string (contains the created file code).</returns>
        public async Task<string> UploadReferenceEntityMediaFileAsync(byte[] fileBytes, string fileName, string contentType, CancellationToken ct = default)
        {
            return await _service.HttpPostMultipartAsync("/api/rest/v1/reference-entities-media-files", "file", fileBytes, fileName, contentType, ct).ConfigureAwait(false);
        }

        #endregion

    }
}

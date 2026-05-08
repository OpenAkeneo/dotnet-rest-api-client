using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Reference entity

        /// <summary>Returns a page of reference entities, optionally starting after a cursor value.</summary>
        /// <param name="searchAfter">Cursor value for keyset pagination (use the last code from the previous page).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ReferenceEntityList"/> with HAL navigation links.</returns>
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

        public async Task<ReferenceEntityList> GetReferenceEntityListAsync(string? searchAfter = null, CancellationToken ct = default)
        {
            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(searchAfter))
                queryParameters.Add("search_after", searchAfter);

            return await GetReferenceEntityListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<ReferenceEntityList> GetReferenceEntityListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ReferenceEntity>(responseString, url);

            return new ReferenceEntityList { Links = links, ReferenceEntities = items };
        }

        public async Task<ReferenceEntity> GetReferenceEntityAsync(string referenceEntityCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntity>(responseString, url);
        }

        public async Task<ReferenceEntity> CreateOrUpdateReferenceEntityAsync(ReferenceEntity referenceEntity, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntity.Code)}";
            var body = JsonSerializer.Serialize(referenceEntity);
            return await PatchAndFetchAsync(url, body, () => GetReferenceEntityAsync(referenceEntity.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Reference entity attribute

        public async Task<ReferenceEntityAttributeList> GetReferenceEntityAttributeListAsync(string referenceEntityCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityAttributeList>(responseString, url);
        }

        public async Task<ReferenceEntityAttribute> GetReferenceEntityAttributeAsync(string referenceEntityCode, string attributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attributeCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityAttribute>(responseString, url);
        }

        public async Task<ReferenceEntityAttribute> CreateOrUpdateReferenceEntityAttributeAsync(string referenceEntityCode, ReferenceEntityAttribute attribute, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attribute.Code)}";
            var body = JsonSerializer.Serialize(attribute);
            return await PatchAndFetchAsync(url, body, () => GetReferenceEntityAttributeAsync(referenceEntityCode, attribute.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Reference entity attribute option

        public async Task<ReferenceEntityAttributeOptionList> GetReferenceEntityAttributeOptionListAsync(string referenceEntityCode, string attributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attributeCode)}/options";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityAttributeOptionList>(responseString, url);
        }

        public async Task<ReferenceEntityAttributeOption> GetReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, string optionCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attributeCode)}/options/{Uri.EscapeDataString(optionCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityAttributeOption>(responseString, url);
        }

        public async Task<ReferenceEntityAttributeOption> CreateOrUpdateReferenceEntityAttributeOptionAsync(string referenceEntityCode, string attributeCode, ReferenceEntityAttributeOption option, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/attributes/{Uri.EscapeDataString(attributeCode)}/options/{Uri.EscapeDataString(option.Code)}";
            var body = JsonSerializer.Serialize(option);
            return await PatchAndFetchAsync(url, body, () => GetReferenceEntityAttributeOptionAsync(referenceEntityCode, attributeCode, option.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Reference entity record

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

        public async Task<ReferenceEntityRecordList> GetReferenceEntityRecordListAsync(string referenceEntityCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/records";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ReferenceEntityRecord>(responseString, url);

            return new ReferenceEntityRecordList { Links = links, ReferenceEntityRecords = items };
        }

        public async Task<ReferenceEntityRecord> GetReferenceEntityRecordAsync(string referenceEntityCode, string recordCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/records/{Uri.EscapeDataString(recordCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ReferenceEntityRecord>(responseString, url);
        }

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

        #endregion

    }
}

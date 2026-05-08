using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Attribute

        /// <summary>Streams all attributes from the Akeneo catalog one at a time, following HAL pagination automatically.</summary>
        /// <param name="search">Optional JSON-encoded Akeneo search filter.</param>
        /// <param name="withCount">When <c>true</c>, the response includes the total item count.</param>
        /// <param name="withTableSelectOptions">When <c>true</c>, table attribute select options are included.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<AkeneoAttribute> StreamAttributesAsync(string? search = null, bool withCount = false, bool withTableSelectOptions = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetAttributeListAsync(page, 100, search, withCount, withTableSelectOptions, ct).ConfigureAwait(false);
                foreach (var item in partial.Attributes)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        /// <summary>Returns all attributes as a materialised list by exhausting <see cref="StreamAttributesAsync"/>.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="withTableSelectOptions">Include table attribute select options.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="AkeneoAttribute"/> objects.</returns>
        public async Task<List<AkeneoAttribute>> GetAttributeListFullAsync(string? search = null, bool withCount = false, bool withTableSelectOptions = false, CancellationToken ct = default)
        {
            var list = new List<AkeneoAttribute>();
            await foreach (var item in StreamAttributesAsync(search, withCount, withTableSelectOptions, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of attributes.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="withTableSelectOptions">Include table attribute select options.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AttributeList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<AttributeList> GetAttributeListAsync(int page = 1, int limit = 100, string? search = null, bool withCount = false, bool withTableSelectOptions = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");
            queryParameters.Add("with_table_select_options", withTableSelectOptions ? "true" : "false");

            return await GetAttributeListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns an attribute page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AttributeList"/> with HAL navigation links.</returns>
        public async Task<AttributeList> GetAttributeListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attributes";

            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<AkeneoAttribute>(responseString, url);

            return new AttributeList { Links = links, Attributes = items };
        }

        /// <summary>Returns a single attribute by its code.</summary>
        /// <param name="attributeCode">The Akeneo attribute code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="AkeneoAttribute"/>.</returns>
        public async Task<AkeneoAttribute> GetAttributeAsync(string attributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attributes/{Uri.EscapeDataString(attributeCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AkeneoAttribute>(responseString, url);
        }

        /// <summary>Creates or updates an attribute via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="attribute">The attribute to create or update. <see cref="AkeneoAttribute.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="AkeneoAttribute"/> as returned by the API.</returns>
        public async Task<AkeneoAttribute> CreateOrUpdateAttributeAsync(AkeneoAttribute attribute, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attributes/{Uri.EscapeDataString(attribute.Code)}";
            var body = JsonSerializer.Serialize(attribute);

            return await PatchAndFetchAsync(url, body, () => GetAttributeAsync(attribute.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Attribute option

        /// <summary>Streams all options for a given attribute, following HAL pagination automatically.</summary>
        /// <param name="attributeCode">The attribute whose options to enumerate.</param>
        /// <param name="withCount">When <c>true</c>, the response includes the total item count.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<AttributeOption> StreamAttributeOptionsAsync(string attributeCode, bool withCount = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetAttributeOptionListAsync(attributeCode, page, 100, withCount, ct).ConfigureAwait(false);
                foreach (var item in partial.AttributeOptions)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        /// <summary>Returns all options for a given attribute as a materialised list.</summary>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="AttributeOption"/> objects.</returns>
        public async Task<List<AttributeOption>> GetAttributeOptionListFullAsync(string attributeCode, bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<AttributeOption>();
            await foreach (var item in StreamAttributeOptionsAsync(attributeCode, withCount, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of options for a given attribute.</summary>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AttributeOptionList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<AttributeOptionList> GetAttributeOptionListAsync(string attributeCode, int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetAttributeOptionListAsync(attributeCode, queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns an attribute option page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AttributeOptionList"/> with HAL navigation links.</returns>
        public async Task<AttributeOptionList> GetAttributeOptionListAsync(string attributeCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attributes/{Uri.EscapeDataString(attributeCode)}/options";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<AttributeOption>(responseString, url);

            return new AttributeOptionList { Links = links, AttributeOptions = items };
        }

        /// <summary>Returns a single attribute option by its code.</summary>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="attributeOptionCode">The option code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="AttributeOption"/>.</returns>
        public async Task<AttributeOption> GetAttributeOptionAsync(string attributeCode, string attributeOptionCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attributes/{Uri.EscapeDataString(attributeCode)}/options/{Uri.EscapeDataString(attributeOptionCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AttributeOption>(responseString, url);
        }

        /// <summary>Creates or updates an attribute option via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="attributeCode">The attribute code.</param>
        /// <param name="attributeOption">The option to create or update. <see cref="AttributeOption.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="AttributeOption"/> as returned by the API.</returns>
        public async Task<AttributeOption> CreateOrUpdateAttributeOptionAsync(string attributeCode, AttributeOption attributeOption, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attributes/{Uri.EscapeDataString(attributeCode)}/options/{Uri.EscapeDataString(attributeOption.Code)}";
            var body = JsonSerializer.Serialize(attributeOption);

            return await PatchAndFetchAsync(url, body, () => GetAttributeOptionAsync(attributeCode, attributeOption.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Attribute group

        /// <summary>Streams all attribute groups, following HAL pagination automatically.</summary>
        /// <param name="search">Optional JSON-encoded Akeneo search filter.</param>
        /// <param name="withCount">When <c>true</c>, the response includes the total item count.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<AttributeGroup> StreamAttributeGroupsAsync(string? search = null, bool withCount = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetAttributeGroupListAsync(page, 100, search, withCount, ct).ConfigureAwait(false);
                foreach (var item in partial.AttributeGroups)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        /// <summary>Returns all attribute groups as a materialised list.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="AttributeGroup"/> objects.</returns>
        public async Task<List<AttributeGroup>> GetAttributeGroupListFullAsync(string? search = null, bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<AttributeGroup>();
            await foreach (var item in StreamAttributeGroupsAsync(search, withCount, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of attribute groups.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AttributeGroupList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<AttributeGroupList> GetAttributeGroupListAsync(int page = 1, int limit = 100, string? search = null, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetAttributeGroupListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns an attribute group page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AttributeGroupList"/> with HAL navigation links.</returns>
        public async Task<AttributeGroupList> GetAttributeGroupListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attribute-groups";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<AttributeGroup>(responseString, url);

            return new AttributeGroupList { Links = links, AttributeGroups = items };
        }

        /// <summary>Returns a single attribute group by its code.</summary>
        /// <param name="attributeCode">The attribute group code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="AttributeGroup"/>.</returns>
        public async Task<AttributeGroup> GetAttributeGroupAsync(string attributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attribute-groups/{Uri.EscapeDataString(attributeCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AttributeGroup>(responseString, url);
        }

        /// <summary>Creates or updates an attribute group via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="attributeGroup">The group to create or update. <see cref="AttributeGroup.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="AttributeGroup"/> as returned by the API.</returns>
        public async Task<AttributeGroup> CreateOrUpdateAttributeGroupAsync(AttributeGroup attributeGroup, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/attribute-groups/{Uri.EscapeDataString(attributeGroup.Code)}";
            var body = JsonSerializer.Serialize(attributeGroup);

            return await PatchAndFetchAsync(url, body, () => GetAttributeGroupAsync(attributeGroup.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

    }
}

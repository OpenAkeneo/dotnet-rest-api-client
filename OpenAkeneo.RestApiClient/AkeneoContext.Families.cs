using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Family

        /// <summary>Streams all product families, following HAL pagination automatically.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<Family> StreamFamiliesAsync(string? search = null, bool withCount = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetFamilyListAsync(page, 100, search, withCount, ct).ConfigureAwait(false);
                foreach (var item in partial.Families)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        public async Task<List<Family>> GetFamilyListFullAsync(string? search = null, bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<Family>();
            await foreach (var item in StreamFamiliesAsync(search, withCount, ct))
                list.Add(item);
            return list;
        }

        public async Task<FamilyList> GetFamilyListAsync(int page = 1, int limit = 100, string? search = null, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetFamilyListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<FamilyList> GetFamilyListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/families";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Family>(responseString, url);

            return new FamilyList { Links = links, Families = items };
        }

        public async Task<Family> GetFamilyAsync(string familyCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/families/{Uri.EscapeDataString(familyCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Family>(responseString, url);
        }

        public async Task<Family> CreateOrUpdateFamilyAsync(Family family, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/families/{Uri.EscapeDataString(family.Code)}";
            var body = JsonSerializer.Serialize(family);
            return await PatchAndFetchAsync(url, body, () => GetFamilyAsync(family.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Family Variant

        // TODO: Family variant https://api.akeneo.com/api-reference.html#Familyvariant

        /// <summary>Streams all variants of a given product family, following HAL pagination automatically.</summary>
        /// <param name="familyCode">The family code whose variants to enumerate.</param>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<FamilyVariant> StreamFamilyVariantsAsync(string familyCode, bool withCount = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetFamilyVariantListAsync(familyCode, page, 100, withCount, ct).ConfigureAwait(false);
                foreach (var item in partial.FamilyVariants)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        public async Task<List<FamilyVariant>> GetFamilyVariantListFullAsync(string familyCode, bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<FamilyVariant>();
            await foreach (var item in StreamFamilyVariantsAsync(familyCode, withCount, ct))
                list.Add(item);
            return list;
        }

        public async Task<FamilyVariantList> GetFamilyVariantListAsync(string familyCode, int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetFamilyVariantListAsync(familyCode, queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<FamilyVariantList> GetFamilyVariantListAsync(string familyCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/families/{Uri.EscapeDataString(familyCode)}/variants";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<FamilyVariant>(responseString, url);

            return new FamilyVariantList { Links = links, FamilyVariants = items };
        }

        public async Task<FamilyVariant> GetFamilyVariantAsync(string familyCode, string code, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/families/{Uri.EscapeDataString(familyCode)}/variants/{Uri.EscapeDataString(code)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<FamilyVariant>(responseString, url);
        }

        public async Task<FamilyVariant> CreateOrUpdateFamilyVariantAsync(string familyCode, FamilyVariant variant, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/families/{Uri.EscapeDataString(familyCode)}/variants/{Uri.EscapeDataString(variant.Code)}";
            var body = JsonSerializer.Serialize(variant);
            return await PatchAndFetchAsync(url, body, () => GetFamilyVariantAsync(familyCode, variant.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

    }
}

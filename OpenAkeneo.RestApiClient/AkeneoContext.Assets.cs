using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Asset family

        /// <summary>Returns a page of asset families, optionally starting after a cursor value.</summary>
        /// <param name="searchAfter">Cursor value for keyset pagination (use the last code from the previous page).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An <see cref="AssetFamilyList"/> with HAL navigation links.</returns>
        public async Task<List<AssetFamily>> GetAssetFamilyListFullAsync(CancellationToken ct = default)
        {
            var list = new List<AssetFamily>();
            string? cursor = null;
            do
            {
                var page = await GetAssetFamilyListAsync(cursor, ct).ConfigureAwait(false);
                list.AddRange(page.AssetFamilies);
                cursor = page.Links?.Next?.Href is not null
                    ? ExtractAssetSearchAfter(page.Links.Next.Href)
                    : null;
            } while (cursor is not null);
            return list;
        }

        private static string? ExtractAssetSearchAfter(string url)
        {
            var idx = url.IndexOf("search_after=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            idx += "search_after=".Length;
            var end = url.IndexOf('&', idx);
            return Uri.UnescapeDataString(end < 0 ? url[idx..] : url[idx..end]);
        }

        public async Task<AssetFamilyList> GetAssetFamilyListAsync(string? searchAfter = null, CancellationToken ct = default)
        {
            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(searchAfter))
                queryParameters.Add("search_after", searchAfter);

            return await GetAssetFamilyListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<AssetFamilyList> GetAssetFamilyListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<AssetFamily>(responseString, url);

            return new AssetFamilyList { Links = links, AssetFamilies = items };
        }

        public async Task<AssetFamily> GetAssetFamilyAsync(string assetFamilyCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetFamily>(responseString, url);
        }

        public async Task<AssetFamily> CreateOrUpdateAssetFamilyAsync(AssetFamily assetFamily, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamily.Code)}";
            var body = JsonSerializer.Serialize(assetFamily);
            return await PatchAndFetchAsync(url, body, () => GetAssetFamilyAsync(assetFamily.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Asset attribute

        // TODO: Asset attribute https://api.akeneo.com/api-reference.html#Assetattribute
        public async Task<AssetAttributeList> GetAssetAttributeListAsync(string assetFamilyCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetAttributeList>(responseString, url);
        }

        public async Task<AssetAttribute> GetAssetAttributeAsync(string assetFamilyCode, string assetAttributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttributeCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetAttribute>(responseString, url);
        }

        public async Task<AssetAttribute> CreateOrUpdateAssetAttributeAsync(string assetFamilyCode, AssetAttribute assetAttribute, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttribute.Code)}";
            var body = JsonSerializer.Serialize(assetAttribute);
            return await PatchAndFetchAsync(url, body, () => GetAssetAttributeAsync(assetFamilyCode, assetAttribute.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Asset attribute option

        public async Task<AssetAttributeOptionList> GetAssetAttributeOptionListAsync(string assetFamilyCode, string assetAttributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttributeCode)}/options";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetAttributeOptionList>(responseString, url);
        }

        public async Task<AssetAttributeOption> GetAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, string optionCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttributeCode)}/options/{Uri.EscapeDataString(optionCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetAttributeOption>(responseString, url);
        }

        public async Task<AssetAttributeOption> CreateOrUpdateAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, AssetAttributeOption option, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttributeCode)}/options/{Uri.EscapeDataString(option.Code)}";
            var body = JsonSerializer.Serialize(option);
            return await PatchAndFetchAsync(url, body, () => GetAssetAttributeOptionAsync(assetFamilyCode, assetAttributeCode, option.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Asset media file

        /// <summary>Downloads the binary content of an asset media file.</summary>
        /// <param name="mediaFileCode">The media file code (as returned by the asset value data).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw file bytes.</returns>
        public async Task<byte[]> DownloadAssetMediaFileAsync(string mediaFileCode, CancellationToken ct = default)
        {
            return await _service.HttpGetBytesAsync($"/api/rest/v1/asset-media-files/{Uri.EscapeDataString(mediaFileCode)}", ct).ConfigureAwait(false);
        }

        #endregion

        #region Asset

        /// <summary>Streams all assets in a given family, following HAL pagination automatically.</summary>
        /// <param name="assetFamilyCode">The asset family code to list assets from.</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="searchAfter">Cursor for keyset pagination.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<Asset> StreamAssetsAsync(string assetFamilyCode, string? search = null, string? searchAfter = null, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetAssetListAsync(assetFamilyCode, page, 100, search, searchAfter, ct).ConfigureAwait(false);
                if (partial.Assets != null)
                    foreach (var item in partial.Assets)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.Assets == null || partial.Assets.Count == 0)
                    yield break;
            }
        }

        public async Task<List<Asset>> GetAssetListFullAsync(string assetFamilyCode, string? search = null, string? searchAfter = null, CancellationToken ct = default)
        {
            var list = new List<Asset>();
            await foreach (var item in StreamAssetsAsync(assetFamilyCode, search, searchAfter, ct))
                list.Add(item);
            return list;
        }

        public async Task<AssetList> GetAssetListAsync(string assetFamilyCode, int page = 1, int limit = 100, string? search = null, string? searchAfter = null, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            if (!string.IsNullOrEmpty(searchAfter))
                queryParameters.Add("search_after", searchAfter);

            return await GetAssetListAsync(assetFamilyCode, queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<AssetList> GetAssetListAsync(string assetFamilyCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/assets";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Asset>(responseString, url);

            return new AssetList { Links = links, Assets = items };
        }

        public async Task<Asset> GetAssetAsync(string assetFamilyCode, string code, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/assets/{Uri.EscapeDataString(code)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Asset>(responseString, url);
        }

        public async Task<Asset> CreateOrUpdateAssetAsync(string assetFamilyCode, Asset asset, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/assets/{Uri.EscapeDataString(asset.Code)}";
            var body = JsonSerializer.Serialize(asset);
            return await PatchAndFetchAsync(url, body, () => GetAssetAsync(assetFamilyCode, asset.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

    }
}

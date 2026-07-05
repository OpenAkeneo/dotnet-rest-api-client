using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Asset family

        /// <summary>Returns all asset families as a materialised list by following keyset pagination automatically.</summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="AssetFamily"/> objects.</returns>
        public async Task<List<AssetFamily>> GetAssetFamilyListFullAsync(CancellationToken ct = default)
        {
            var list = new List<AssetFamily>();
            await foreach (var item in StreamAssetFamiliesAsync(ct).ConfigureAwait(false))
                list.Add(item);
            return list;
        }

        /// <summary>Streams all asset families, following keyset pagination automatically.</summary>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<AssetFamily> StreamAssetFamiliesAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            string? cursor = null;
            while (true)
            {
                var page = await GetAssetFamilyListAsync(cursor, ct).ConfigureAwait(false);
                foreach (var item in page.AssetFamilies)
                    yield return item;

                if (string.IsNullOrEmpty(page.Links?.Next?.Href))
                    yield break;

                cursor = AkeneoContextHelpers.ExtractSearchAfter(page.Links.Next.Href);
                if (string.IsNullOrEmpty(cursor))
                    yield break;
            }
        }

        /// <summary>Returns a page of asset families, optionally starting after a cursor value.</summary>
        /// <param name="searchAfter">Cursor value for keyset pagination (use the last code from the previous page).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An <see cref="AssetFamilyList"/> with HAL navigation links.</returns>
        public async Task<AssetFamilyList> GetAssetFamilyListAsync(string? searchAfter = null, CancellationToken ct = default)
        {
            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(searchAfter))
                queryParameters.Add("search_after", searchAfter);

            return await GetAssetFamilyListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns an asset family page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An <see cref="AssetFamilyList"/> with HAL navigation links.</returns>
        public async Task<AssetFamilyList> GetAssetFamilyListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<AssetFamily>(responseString, url);

            return new AssetFamilyList { Links = links, AssetFamilies = items };
        }

        /// <summary>Returns a single asset family by its code.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="AssetFamily"/>.</returns>
        public async Task<AssetFamily> GetAssetFamilyAsync(string assetFamilyCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetFamily>(responseString, url);
        }

        /// <summary>Creates or updates an asset family via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="assetFamily">The asset family to create or update. <see cref="AssetFamily.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="AssetFamily"/> as returned by the API.</returns>
        public async Task<AssetFamily> CreateOrUpdateAssetFamilyAsync(AssetFamily assetFamily, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamily.Code)}";
            var body = JsonSerializer.Serialize(assetFamily);
            return await PatchAndFetchAsync(url, body, () => GetAssetFamilyAsync(assetFamily.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Asset attribute

        // TODO: Asset attribute https://api.akeneo.com/api-reference.html#Assetattribute
        /// <summary>Returns all attributes for a given asset family.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An <see cref="AssetAttributeList"/>.</returns>
        public async Task<AssetAttributeList> GetAssetAttributeListAsync(string assetFamilyCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetAttributeList>(responseString, url);
        }

        /// <summary>Returns a single attribute for an asset family.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="assetAttributeCode">The attribute code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="AssetAttribute"/>.</returns>
        public async Task<AssetAttribute> GetAssetAttributeAsync(string assetFamilyCode, string assetAttributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttributeCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetAttribute>(responseString, url);
        }

        /// <summary>Creates or updates an asset attribute via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="assetAttribute">The attribute to create or update.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="AssetAttribute"/> as returned by the API.</returns>
        public async Task<AssetAttribute> CreateOrUpdateAssetAttributeAsync(string assetFamilyCode, AssetAttribute assetAttribute, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttribute.Code)}";
            var body = JsonSerializer.Serialize(assetAttribute);
            return await PatchAndFetchAsync(url, body, () => GetAssetAttributeAsync(assetFamilyCode, assetAttribute.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Asset attribute option

        /// <summary>Returns all options for a given asset attribute.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="assetAttributeCode">The attribute code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An <see cref="AssetAttributeOptionList"/>.</returns>
        public async Task<AssetAttributeOptionList> GetAssetAttributeOptionListAsync(string assetFamilyCode, string assetAttributeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttributeCode)}/options";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetAttributeOptionList>(responseString, url);
        }

        /// <summary>Returns a single option for an asset attribute.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="assetAttributeCode">The attribute code.</param>
        /// <param name="optionCode">The option code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="AssetAttributeOption"/>.</returns>
        public async Task<AssetAttributeOption> GetAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, string optionCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttributeCode)}/options/{Uri.EscapeDataString(optionCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssetAttributeOption>(responseString, url);
        }

        /// <summary>Creates or updates an asset attribute option via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="assetAttributeCode">The attribute code.</param>
        /// <param name="option">The option to create or update.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="AssetAttributeOption"/> as returned by the API.</returns>
        public async Task<AssetAttributeOption> CreateOrUpdateAssetAttributeOptionAsync(string assetFamilyCode, string assetAttributeCode, AssetAttributeOption option, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/attributes/{Uri.EscapeDataString(assetAttributeCode)}/options/{Uri.EscapeDataString(option.Code)}";
            var body = JsonSerializer.Serialize(option);
            return await PatchAndFetchAsync(url, body, () => GetAssetAttributeOptionAsync(assetFamilyCode, assetAttributeCode, option.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Asset media file

        /// <summary>Downloads the binary content of an asset media file.</summary>
        /// <param name="mediaFileCode">The media file code (as returned by the asset value data; may contain path segments separated by <c>/</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw file bytes.</returns>
        public async Task<byte[]> DownloadAssetMediaFileAsync(string mediaFileCode, CancellationToken ct = default)
        {
            // Media-file codes contain '/' path segments that must survive as separators —
            // escape each segment individually (matches the product/reference-entity downloads).
            var codeEscaped = string.Join("/", mediaFileCode.Split('/').Select(Uri.EscapeDataString));
            return await _service.HttpGetBytesAsync($"/api/rest/v1/asset-media-files/{codeEscaped}", ct).ConfigureAwait(false);
        }

        /// <summary>Downloads an asset media file as an unbuffered stream (for large files). Dispose the stream to release the HTTP response.</summary>
        /// <param name="mediaFileCode">The media file code (may contain path segments separated by <c>/</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A stream over the file content.</returns>
        public async Task<Stream> DownloadAssetMediaFileStreamAsync(string mediaFileCode, CancellationToken ct = default)
        {
            var codeEscaped = string.Join("/", mediaFileCode.Split('/').Select(Uri.EscapeDataString));
            return await _service.HttpGetStreamAsync($"/api/rest/v1/asset-media-files/{codeEscaped}", ct).ConfigureAwait(false);
        }

        /// <summary>Uploads an asset media file and returns the created file code (resolved from the 201 response headers).</summary>
        /// <param name="fileBytes">Raw file bytes.</param>
        /// <param name="fileName">Original file name (e.g. <c>image.jpg</c>).</param>
        /// <param name="contentType">MIME type (e.g. <c>image/jpeg</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created media-file code.</returns>
        public async Task<string> UploadAssetMediaFileAsync(byte[] fileBytes, string fileName, string contentType, CancellationToken ct = default)
        {
            return await _service.HttpPostMultipartAsync("/api/rest/v1/asset-media-files", "file", fileBytes, fileName, contentType, ct: ct).ConfigureAwait(false);
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
            string? cursor = searchAfter;
            while (true)
            {
                var partial = await GetAssetListAsync(assetFamilyCode, 100, search, cursor, ct).ConfigureAwait(false);
                if (partial.Assets != null)
                    foreach (var item in partial.Assets)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.Assets == null)
                    yield break;

                cursor = AkeneoContextHelpers.ExtractSearchAfter(partial.Links.Next.Href);
                if (string.IsNullOrEmpty(cursor))
                    yield break;
            }
        }

        /// <summary>Returns all assets in a given family as a materialised list.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="searchAfter">Cursor for keyset pagination.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="Asset"/> objects in the family.</returns>
        public async Task<List<Asset>> GetAssetListFullAsync(string assetFamilyCode, string? search = null, string? searchAfter = null, CancellationToken ct = default)
        {
            var list = new List<Asset>();
            await foreach (var item in StreamAssetsAsync(assetFamilyCode, search, searchAfter, ct).ConfigureAwait(false))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of assets for a given family.</summary>
        /// <remarks>The Akeneo assets endpoint paginates only via <c>search_after</c>; there is no page-number pagination.</remarks>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="searchAfter">Cursor for keyset pagination (use the value from the previous page's <c>next</c> link).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AssetList"/> with HAL navigation links.</returns>
        public async Task<AssetList> GetAssetListAsync(string assetFamilyCode, int limit = 100, string? search = null, string? searchAfter = null, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidateLimit(limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["limit"] = limit.ToString()
            };

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            if (!string.IsNullOrEmpty(searchAfter))
                queryParameters.Add("search_after", searchAfter);

            return await GetAssetListAsync(assetFamilyCode, queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns an asset page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AssetList"/> with HAL navigation links.</returns>
        public async Task<AssetList> GetAssetListAsync(string assetFamilyCode, Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/assets";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Asset>(responseString, url);

            return new AssetList { Links = links, Assets = items };
        }

        /// <summary>Returns a single asset by its code within a given family.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="code">The asset code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Asset"/>.</returns>
        public async Task<Asset> GetAssetAsync(string assetFamilyCode, string code, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/assets/{Uri.EscapeDataString(code)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Asset>(responseString, url);
        }

        /// <summary>Creates or updates an asset via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="asset">The asset to create or update. <see cref="Asset.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="Asset"/> as returned by the API.</returns>
        public async Task<Asset> CreateOrUpdateAssetAsync(string assetFamilyCode, Asset asset, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/assets/{Uri.EscapeDataString(asset.Code)}";
            var body = JsonSerializer.Serialize(asset);
            return await PatchAndFetchAsync(url, body, () => GetAssetAsync(assetFamilyCode, asset.Code, ct), ct).ConfigureAwait(false);
        }

        /// <summary>Deletes an asset by its code within a given family.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="code">The asset code to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task DeleteAssetAsync(string assetFamilyCode, string code, CancellationToken ct = default)
        {
            await _service.HttpDeleteAsync($"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/assets/{Uri.EscapeDataString(code)}", ct).ConfigureAwait(false);
        }

        #endregion

    }
}

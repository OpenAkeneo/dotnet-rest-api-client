using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Category

        /// <summary>Streams all categories, following HAL pagination automatically.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="withPosition">Include category position in tree.</param>
        /// <param name="withEnrichedAttributes">Include enriched category attribute values.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<Category> StreamCategoriesAsync(string? search = null, bool withCount = false, bool withPosition = false, bool withEnrichedAttributes = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetCategoryListAsync(page, 100, search, withCount, withPosition, withEnrichedAttributes, ct).ConfigureAwait(false);
                foreach (var item in partial.Categories)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        /// <summary>Returns all categories as a materialised list.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="withPosition">Include category position in tree.</param>
        /// <param name="withEnrichedAttributes">Include enriched category attribute values.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="Category"/> objects.</returns>
        public async Task<List<Category>> GetCategoryListFullAsync(string? search = null, bool withCount = false, bool withPosition = false, bool withEnrichedAttributes = false, CancellationToken ct = default)
        {
            var list = new List<Category>();
            await foreach (var item in StreamCategoriesAsync(search, withCount, withPosition, withEnrichedAttributes, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of categories.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="withPosition">Include category position in tree.</param>
        /// <param name="withEnrichedAttributes">Include enriched category attribute values.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="CategoryList"/> with HAL navigation links.</returns>
        public async Task<CategoryList> GetCategoryListAsync(int page = 1, int limit = 100, string? search = null, bool withCount = false, bool withPosition = false, bool withEnrichedAttributes = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");
            queryParameters.Add("with_position", withPosition ? "true" : "false");
            queryParameters.Add("with_enriched_attributes", withEnrichedAttributes ? "true" : "false");

            return await GetCategoryListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a category page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="CategoryList"/> with HAL navigation links.</returns>
        public async Task<CategoryList> GetCategoryListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/categories";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Category>(responseString, url);

            return new CategoryList { Links = links, Categories = items };
        }

        /// <summary>Returns a single category by its code.</summary>
        /// <param name="code">The category code.</param>
        /// <param name="withPosition">Include category position in tree.</param>
        /// <param name="withEnrichedAttributes">Include enriched category attribute values.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Category"/>.</returns>
        public async Task<Category> GetCategoryAsync(string code, bool withPosition = false, bool withEnrichedAttributes = false, CancellationToken ct = default)
        {
            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("with_position", withPosition ? "true" : "false");
            queryParameters.Add("with_enriched_attributes", withEnrichedAttributes ? "true" : "false");

            return await GetCategoryAsync(code, queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a single category by its code using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="code">The category code.</param>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Category"/>.</returns>
        public async Task<Category> GetCategoryAsync(string code, Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/categories/{Uri.EscapeDataString(code)}";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Category>(responseString, url);
        }

        /// <summary>Downloads the binary content of a category media file.</summary>
        /// <param name="filePath">The media file path as returned by the category attribute value.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw file bytes.</returns>
        public async Task<byte[]> DownloadCategoryMediaFileAsync(string filePath, CancellationToken ct = default)
        {
            var pathEscaped = string.Join("/", filePath.Split('/').Select(Uri.EscapeDataString));
            return await _service.HttpGetBytesAsync($"/api/rest/v1/category-media-files/{pathEscaped}/download", ct).ConfigureAwait(false);
        }

        /// <summary>Creates or updates a category via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="category">The category to create or update. <see cref="Category.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="Category"/> as returned by the API.</returns>
        public async Task<Category> CreateOrUpdateCategoryAsync(Category category, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/categories/{Uri.EscapeDataString(category.Code)}";
            var body = JsonSerializer.Serialize(category);

            return await PatchAndFetchAsync(url, body, () => GetCategoryAsync(category.Code, ct: ct), ct).ConfigureAwait(false);
        }

        #endregion

    }
}

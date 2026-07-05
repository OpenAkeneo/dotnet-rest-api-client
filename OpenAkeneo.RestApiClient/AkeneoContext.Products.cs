using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Products

        /// <summary>
        /// Builds the query parameters used by the product streamers. Streamers use
        /// <c>pagination_type=search_after</c> because Akeneo caps page-based pagination at
        /// 10 000 items; keyset pagination has no such limit.
        /// </summary>
        private static Dictionary<string, string> BuildProductStreamParameters(string? search, string? scope, string? locales, bool withAssetShareLinks)
        {
            var queryParameters = new Dictionary<string, string>
            {
                ["pagination_type"] = "search_after",
                ["limit"] = "100"
            };

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            if (!string.IsNullOrEmpty(scope))
                queryParameters.Add("scope", scope);

            if (!string.IsNullOrEmpty(locales))
                queryParameters.Add("locales", locales);

            if (withAssetShareLinks)
                queryParameters.Add("with_asset_share_links", "true");

            return queryParameters;
        }

        #region Product UUID

        /// <summary>Streams all UUID-based products, following keyset (<c>search_after</c>) pagination automatically — not subject to Akeneo's 10 000-item page-pagination limit.</summary>
        /// <param name="search">Optional JSON-encoded Akeneo search filter.</param>
        /// <param name="scope">Optional channel scope for attribute completeness filtering.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<ProductUuid> StreamProductUuidsAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var queryParameters = BuildProductStreamParameters(search, scope, locales, withAssetShareLinks);
            while (true)
            {
                var partial = await GetProductUuidListAsync(queryParameters, ct).ConfigureAwait(false);
                if (partial.Products != null)
                    foreach (var item in partial.Products)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.Products == null)
                    yield break;

                var cursor = AkeneoContextHelpers.ExtractSearchAfter(partial.Links.Next.Href);
                if (string.IsNullOrEmpty(cursor))
                    yield break;
                queryParameters["search_after"] = cursor;
            }
        }

        /// <summary>Returns all UUID-based products as a materialised list.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="scope">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="ProductUuid"/> objects in the catalog.</returns>
        public async Task<List<ProductUuid>> GetProductUuidListFullAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
        {
            var list = new List<ProductUuid>();
            await foreach (var item in StreamProductUuidsAsync(search, scope, locales, withAssetShareLinks, ct).ConfigureAwait(false))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of UUID-based products.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="scope">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductUuidList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<ProductUuidList> GetProductUuidListAsync(int page = 1, int limit = 100, string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            if (!string.IsNullOrEmpty(scope))
                queryParameters.Add("scope", scope);

            if (!string.IsNullOrEmpty(locales))
                queryParameters.Add("locales", locales);

            if (withAssetShareLinks)
                queryParameters.Add("with_asset_share_links", "true");

            return await GetProductUuidListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a UUID-based product page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductUuidList"/> with HAL navigation links.</returns>
        public async Task<ProductUuidList> GetProductUuidListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/products-uuid";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ProductUuid>(responseString, url);

            return new ProductUuidList { Links = links, Products = items };
        }

        /// <summary>Returns a single product by its UUID.</summary>
        /// <param name="uuid">Product UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ProductUuid"/>.</returns>
        public async Task<ProductUuid> GetProductUuidAsync(string uuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/products-uuid/{Uri.EscapeDataString(uuid)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ProductUuid>(responseString, url);
        }

        /// <summary>Returns the current draft of a product by its UUID (requires the Workflow feature).</summary>
        /// <param name="uuid">Product UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The draft <see cref="ProductUuid"/>.</returns>
        public async Task<ProductUuid> GetProductUuidDraftAsync(string uuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/products-uuid/{Uri.EscapeDataString(uuid)}/draft";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ProductUuid>(responseString, url);
        }

        /// <summary>Creates or updates a product via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="product">The product to create or update. <see cref="ProductUuid.Uuid"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="ProductUuid"/> as returned by the API.</returns>
        public async Task<ProductUuid> CreateOrUpdateProductUuidAsync(ProductUuid product, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/products-uuid/{Uri.EscapeDataString(product.Uuid!)}";
            var body = JsonSerializer.Serialize(product);
            return await PatchAndFetchAsync(url, body, () => GetProductUuidAsync(product.Uuid!, ct), ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new UUID-based product via HTTP POST and returns the created entity.
        /// <see cref="ProductUuid.Uuid"/> may be left <c>null</c> — Akeneo then generates the UUID,
        /// which is resolved from the 201 <c>Location</c> response header.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created <see cref="ProductUuid"/>.</returns>
        public async Task<ProductUuid> CreateProductUuidAsync(ProductUuid product, CancellationToken ct = default)
        {
            const string url = "/api/rest/v1/products-uuid";
            var body = JsonSerializer.Serialize(product);
            var (status, _, location) = await _service.HttpPostWithLocationAsync(url, body, ct).ConfigureAwait(false);

            var uuid = product.Uuid ?? AkeneoContextHelpers.ExtractLastPathSegment(location)
                ?? throw new AkeneoApiException(url, "POST", status,
                    "Product was created without a client-side UUID, but the response carried no Location header to resolve the generated UUID from.");

            return await GetProductUuidAsync(uuid, ct).ConfigureAwait(false);
        }

        /// <summary>Deletes a UUID-based product.</summary>
        /// <param name="uuid">The product UUID to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task DeleteProductUuidAsync(string uuid, CancellationToken ct = default)
        {
            await _service.HttpDeleteAsync($"/api/rest/v1/products-uuid/{Uri.EscapeDataString(uuid)}", ct).ConfigureAwait(false);
        }

        /// <summary>Submits a draft of a UUID-based product for approval (requires the Workflow feature).</summary>
        /// <param name="uuid">The product UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task SubmitProductUuidProposalAsync(string uuid, CancellationToken ct = default)
        {
            await _service.HttpPostAsync($"/api/rest/v1/products-uuid/{Uri.EscapeDataString(uuid)}/proposal", null, (Dictionary<string, string>?)null, ct).ConfigureAwait(false);
        }

        /// <summary>Searches UUID-based products using a POST body (supports large search payloads).</summary>
        /// <param name="searchBody">JSON search payload as defined by the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductUuidList"/> with HAL navigation links.</returns>
        public async Task<ProductUuidList> SearchProductUuidsAsync(string searchBody, CancellationToken ct = default)
        {
            var url = "/api/rest/v1/products-uuid/search";
            var responseString = await _service.HttpPostAsync(url, searchBody, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ProductUuid>(responseString, url);
            return new ProductUuidList { Links = links, Products = items };
        }

        #endregion

        #region Product identifier

        /// <summary>Streams all identifier-based products (legacy API), following keyset (<c>search_after</c>) pagination automatically — not subject to Akeneo's 10 000-item page-pagination limit.</summary>
        /// <param name="search">Optional JSON-encoded Akeneo search filter.</param>
        /// <param name="scope">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<ProductIdentifier> StreamProductIdentifiersAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var queryParameters = BuildProductStreamParameters(search, scope, locales, withAssetShareLinks);
            while (true)
            {
                var partial = await GetProductIdentifierListAsync(queryParameters, ct).ConfigureAwait(false);
                if (partial.Products != null)
                    foreach (var item in partial.Products)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.Products == null)
                    yield break;

                var cursor = AkeneoContextHelpers.ExtractSearchAfter(partial.Links.Next.Href);
                if (string.IsNullOrEmpty(cursor))
                    yield break;
                queryParameters["search_after"] = cursor;
            }
        }

        /// <summary>Returns all identifier-based products as a materialised list.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="scope">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="ProductIdentifier"/> objects in the catalog.</returns>
        public async Task<List<ProductIdentifier>> GetProductIdentifierListFullAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
        {
            var list = new List<ProductIdentifier>();
            await foreach (var item in StreamProductIdentifiersAsync(search, scope, locales, withAssetShareLinks, ct).ConfigureAwait(false))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of identifier-based products.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="scope">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductIdentifierList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<ProductIdentifierList> GetProductIdentifierListAsync(int page = 1, int limit = 100, string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            if (!string.IsNullOrEmpty(scope))
                queryParameters.Add("scope", scope);

            if (!string.IsNullOrEmpty(locales))
                queryParameters.Add("locales", locales);

            if (withAssetShareLinks)
                queryParameters.Add("with_asset_share_links", "true");

            return await GetProductIdentifierListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns an identifier-based product page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductIdentifierList"/> with HAL navigation links.</returns>
        public async Task<ProductIdentifierList> GetProductIdentifierListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/products";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ProductIdentifier>(responseString, url);

            return new ProductIdentifierList { Links = links, Products = items };
        }

        /// <summary>Returns a single product by its identifier (SKU).</summary>
        /// <param name="identifier">The product identifier (SKU).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ProductIdentifier"/>.</returns>
        public async Task<ProductIdentifier> GetProductIdentifierAsync(string identifier, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/products/{Uri.EscapeDataString(identifier)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ProductIdentifier>(responseString, url);
        }

        /// <summary>Returns the current draft of a product by its identifier (requires the Workflow feature).</summary>
        /// <param name="identifier">The product identifier (SKU).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The draft <see cref="ProductIdentifier"/>.</returns>
        public async Task<ProductIdentifier> GetProductIdentifierDraftAsync(string identifier, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/products/{Uri.EscapeDataString(identifier)}/draft";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ProductIdentifier>(responseString, url);
        }

        /// <summary>Creates or updates an identifier-based product via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="product">The product to create or update. <see cref="ProductIdentifier.Identifier"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="ProductIdentifier"/> as returned by the API.</returns>
        public async Task<ProductIdentifier> CreateOrUpdateProductIdentifierAsync(ProductIdentifier product, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/products/{Uri.EscapeDataString(product.Identifier)}";
            var body = JsonSerializer.Serialize(product);
            return await PatchAndFetchAsync(url, body, () => GetProductIdentifierAsync(product.Identifier, ct), ct).ConfigureAwait(false);
        }

        /// <summary>Creates a new identifier-based product via HTTP POST and returns the created entity.</summary>
        /// <param name="product">The product to create.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created <see cref="ProductIdentifier"/>.</returns>
        public async Task<ProductIdentifier> CreateProductIdentifierAsync(ProductIdentifier product, CancellationToken ct = default)
        {
            var body = JsonSerializer.Serialize(product);
            await _service.HttpPostAsync("/api/rest/v1/products", body, ct).ConfigureAwait(false);
            return await GetProductIdentifierAsync(product.Identifier, ct).ConfigureAwait(false);
        }

        /// <summary>Deletes an identifier-based product.</summary>
        /// <param name="identifier">The product identifier (SKU) to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task DeleteProductIdentifierAsync(string identifier, CancellationToken ct = default)
        {
            await _service.HttpDeleteAsync($"/api/rest/v1/products/{Uri.EscapeDataString(identifier)}", ct).ConfigureAwait(false);
        }

        /// <summary>Submits a draft of an identifier-based product for approval (requires the Workflow feature).</summary>
        /// <param name="identifier">The product identifier (SKU).</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task SubmitProductIdentifierProposalAsync(string identifier, CancellationToken ct = default)
        {
            await _service.HttpPostAsync($"/api/rest/v1/products/{Uri.EscapeDataString(identifier)}/proposal", null, (Dictionary<string, string>?)null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Product model

        /// <summary>Streams all product models, following keyset (<c>search_after</c>) pagination automatically — not subject to Akeneo's 10 000-item page-pagination limit.</summary>
        /// <param name="search">Optional JSON-encoded Akeneo search filter.</param>
        /// <param name="scope">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<ProductModel> StreamProductModelsAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var queryParameters = BuildProductStreamParameters(search, scope, locales, withAssetShareLinks);
            while (true)
            {
                var partial = await GetProductModelListAsync(queryParameters, ct).ConfigureAwait(false);
                if (partial.ProductModels != null)
                    foreach (var item in partial.ProductModels)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.ProductModels == null)
                    yield break;

                var cursor = AkeneoContextHelpers.ExtractSearchAfter(partial.Links.Next.Href);
                if (string.IsNullOrEmpty(cursor))
                    yield break;
                queryParameters["search_after"] = cursor;
            }
        }

        /// <summary>Returns all product models as a materialised list.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="scope">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="ProductModel"/> objects in the catalog.</returns>
        public async Task<List<ProductModel>> GetProductModelListFullAsync(string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
        {
            var list = new List<ProductModel>();
            await foreach (var item in StreamProductModelsAsync(search, scope, locales, withAssetShareLinks, ct).ConfigureAwait(false))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of product models.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="scope">Optional channel scope.</param>
        /// <param name="locales">Optional comma-separated locale codes.</param>
        /// <param name="withAssetShareLinks">When <c>true</c>, populates <c>linked_data</c> on asset_collection values with public CDN share link URLs.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductModelList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<ProductModelList> GetProductModelListAsync(int page = 1, int limit = 100, string? search = null, string? scope = null, string? locales = null, bool withAssetShareLinks = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            if (!string.IsNullOrEmpty(search))
                queryParameters.Add("search", search);

            if (!string.IsNullOrEmpty(scope))
                queryParameters.Add("scope", scope);

            if (!string.IsNullOrEmpty(locales))
                queryParameters.Add("locales", locales);

            if (withAssetShareLinks)
                queryParameters.Add("with_asset_share_links", "true");

            return await GetProductModelListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a product model page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductModelList"/> with HAL navigation links.</returns>
        public async Task<ProductModelList> GetProductModelListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/product-models";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ProductModel>(responseString, url);

            return new ProductModelList { Links = links, ProductModels = items };
        }

        /// <summary>Returns a single product model by its code.</summary>
        /// <param name="code">The product model code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ProductModel"/>.</returns>
        public async Task<ProductModel> GetProductModelAsync(string code, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/product-models/{Uri.EscapeDataString(code)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ProductModel>(responseString, url);
        }

        /// <summary>Returns the current draft of a product model by its code (requires the Workflow feature).</summary>
        /// <param name="code">The product model code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The draft <see cref="ProductModel"/>.</returns>
        public async Task<ProductModel> GetProductModelDraftAsync(string code, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/product-models/{Uri.EscapeDataString(code)}/draft";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ProductModel>(responseString, url);
        }

        /// <summary>Creates or updates a product model via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="productModel">The product model to create or update. <see cref="ProductModel.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="ProductModel"/> as returned by the API.</returns>
        public async Task<ProductModel> CreateOrUpdateProductModelAsync(ProductModel productModel, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/product-models/{Uri.EscapeDataString(productModel.Code)}";
            var body = JsonSerializer.Serialize(productModel);
            return await PatchAndFetchAsync(url, body, () => GetProductModelAsync(productModel.Code, ct), ct).ConfigureAwait(false);
        }

        /// <summary>Creates a new product model via HTTP POST and returns the created entity.</summary>
        /// <param name="productModel">The product model to create.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created <see cref="ProductModel"/>.</returns>
        public async Task<ProductModel> CreateProductModelAsync(ProductModel productModel, CancellationToken ct = default)
        {
            var body = JsonSerializer.Serialize(productModel);
            await _service.HttpPostAsync("/api/rest/v1/product-models", body, ct).ConfigureAwait(false);
            return await GetProductModelAsync(productModel.Code, ct).ConfigureAwait(false);
        }

        /// <summary>Deletes a product model by its code.</summary>
        /// <param name="code">The product model code to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task DeleteProductModelAsync(string code, CancellationToken ct = default)
        {
            await _service.HttpDeleteAsync($"/api/rest/v1/product-models/{Uri.EscapeDataString(code)}", ct).ConfigureAwait(false);
        }

        /// <summary>Submits a draft of a product model for approval (requires the Workflow feature).</summary>
        /// <param name="code">The product model code.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task SubmitProductModelProposalAsync(string code, CancellationToken ct = default)
        {
            await _service.HttpPostAsync($"/api/rest/v1/product-models/{Uri.EscapeDataString(code)}/proposal", null, (Dictionary<string, string>?)null, ct).ConfigureAwait(false);
        }

        #endregion

        #region Product media file

        /// <summary>Streams all product media files, following HAL pagination automatically.</summary>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<ProductMediaFile> StreamProductMediaFilesAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetProductMediaFileListAsync(page, 100, ct: ct).ConfigureAwait(false);
                if (partial.MediaFiles != null)
                    foreach (var item in partial.MediaFiles)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.MediaFiles == null)
                    yield break;
            }
        }

        /// <summary>Returns all product media files as a materialised list.</summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="ProductMediaFile"/> objects.</returns>
        public async Task<List<ProductMediaFile>> GetProductMediaFileListFullAsync(CancellationToken ct = default)
        {
            var list = new List<ProductMediaFile>();
            await foreach (var item in StreamProductMediaFilesAsync(ct).ConfigureAwait(false))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of product media files.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductMediaFileList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<ProductMediaFileList> GetProductMediaFileListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString(),
                ["with_count"] = withCount ? "true" : "false"
            };

            return await GetProductMediaFileListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a product media file page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ProductMediaFileList"/> with HAL navigation links.</returns>
        public async Task<ProductMediaFileList> GetProductMediaFileListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/media-files";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<ProductMediaFile>(responseString, url);

            return new ProductMediaFileList { Links = links, MediaFiles = items };
        }

        /// <summary>Returns the metadata for a single product media file by its code.</summary>
        /// <param name="code">The media file code (may contain path segments separated by <c>/</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ProductMediaFile"/>.</returns>
        public async Task<ProductMediaFile> GetProductMediaFileAsync(string code, CancellationToken ct = default)
        {
            var codeEscaped = string.Join("/", code.Split('/').Select(Uri.EscapeDataString));
            var url = $"/api/rest/v1/media-files/{codeEscaped}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<ProductMediaFile>(responseString, url);
        }

        /// <summary>Downloads the binary content of a product media file.</summary>
        /// <param name="code">The media file code (may contain path segments separated by <c>/</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw file bytes.</returns>
        public async Task<byte[]> DownloadProductMediaFileAsync(string code, CancellationToken ct = default)
        {
            var codeEscaped = string.Join("/", code.Split('/').Select(Uri.EscapeDataString));
            return await _service.HttpGetBytesAsync($"/api/rest/v1/media-files/{codeEscaped}/download", ct).ConfigureAwait(false);
        }

        /// <summary>Downloads a product media file as an unbuffered stream (for large files). Dispose the stream to release the HTTP response.</summary>
        /// <param name="code">The media file code (may contain path segments separated by <c>/</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A stream over the file content.</returns>
        public async Task<Stream> DownloadProductMediaFileStreamAsync(string code, CancellationToken ct = default)
        {
            var codeEscaped = string.Join("/", code.Split('/').Select(Uri.EscapeDataString));
            return await _service.HttpGetStreamAsync($"/api/rest/v1/media-files/{codeEscaped}/download", ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads a product media file and returns the created media-file code (resolved from the
        /// 201 response headers). Akeneo links the upload to the attribute value described by
        /// <paramref name="productJson"/> or <paramref name="productModelJson"/> — the API requires
        /// exactly one of the two.
        /// </summary>
        /// <param name="fileBytes">Raw file bytes.</param>
        /// <param name="fileName">Original file name (e.g. <c>photo.jpg</c>).</param>
        /// <param name="contentType">MIME type (e.g. <c>image/jpeg</c>).</param>
        /// <param name="productJson">
        /// JSON object with the target product reference, e.g.
        /// <c>{"identifier":"my-sku","attribute":"picture","scope":null,"locale":null}</c>.
        /// </param>
        /// <param name="productModelJson">
        /// JSON object with the target product model reference, e.g.
        /// <c>{"code":"my-model","attribute":"picture","scope":null,"locale":null}</c>.
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created media-file code.</returns>
        /// <exception cref="ArgumentException">Thrown when both <paramref name="productJson"/> and <paramref name="productModelJson"/> are provided.</exception>
        public async Task<string> UploadProductMediaFileAsync(byte[] fileBytes, string fileName, string contentType, string? productJson = null, string? productModelJson = null, CancellationToken ct = default)
        {
            // POST /media-files requires a "product" or "product_model" part alongside "file"
            // (exactly one of the two, per the Akeneo REST API spec).
            if (productJson != null && productModelJson != null)
                throw new ArgumentException("Provide either productJson or productModelJson, not both.", nameof(productModelJson));

            Dictionary<string, string>? extraParts = null;
            if (productJson != null)
                extraParts = new() { ["product"] = productJson };
            else if (productModelJson != null)
                extraParts = new() { ["product_model"] = productModelJson };

            return await _service.HttpPostMultipartAsync("/api/rest/v1/media-files", "file", fileBytes, fileName, contentType, extraParts, ct).ConfigureAwait(false);
        }

        #endregion

        #endregion

    }
}

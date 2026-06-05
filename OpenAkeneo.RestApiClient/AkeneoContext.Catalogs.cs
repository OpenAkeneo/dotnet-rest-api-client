using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Catalogs

        /// <summary>Returns a page of catalogs (Catalog for Apps feature).</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="CatalogList"/> with HAL navigation links.</returns>
        public async Task<CatalogList> GetCatalogListAsync(int page = 1, int limit = 100, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/catalogs?page={page}&limit={limit}";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var responseJson = AkeneoContextHelpers.DeserializeOrThrow<HalResponse>(responseString, url);

            var result = new CatalogList { Links = responseJson.Links };

            if (responseJson.Embedded != null && responseJson.Embedded.TryGetValue("items", out var itemsElement))
                result.Catalogs = itemsElement.EnumerateArray().Select(x => x.Deserialize<Catalog>()!).ToList();

            return result;
        }

        /// <summary>Streams all catalogs, following HAL pagination automatically.</summary>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<Catalog> StreamCatalogsAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            int page = 1;
            while (true)
            {
                var partial = await GetCatalogListAsync(page, 100, ct).ConfigureAwait(false);
                if (partial.Catalogs == null || partial.Catalogs.Count == 0)
                    yield break;
                foreach (var item in partial.Catalogs)
                    yield return item;
                if (partial.Links?.Next == null)
                    yield break;
                page++;
            }
        }

        /// <summary>Returns all catalogs as a materialised list.</summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="Catalog"/> objects.</returns>
        public async Task<List<Catalog>> GetCatalogListFullAsync(CancellationToken ct = default)
        {
            var list = new List<Catalog>();
            await foreach (var item in StreamCatalogsAsync(ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single catalog by its ID.</summary>
        /// <param name="id">The catalog UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Catalog"/>.</returns>
        public async Task<Catalog> GetCatalogAsync(string id, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/catalogs/{Uri.EscapeDataString(id)}";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<Catalog>(responseString, url);
        }

        /// <summary>Returns a page of product UUIDs belonging to a catalog.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="CatalogProductUuidList"/> with HAL navigation links.</returns>
        public async Task<CatalogProductUuidList> GetCatalogProductUuidListAsync(string catalogId, int page = 1, int limit = 100, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/product-uuids?page={page}&limit={limit}";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var responseJson = AkeneoContextHelpers.DeserializeOrThrow<HalResponse>(responseString, url);

            var result = new CatalogProductUuidList { Links = responseJson.Links };

            if (responseJson.Embedded != null && responseJson.Embedded.TryGetValue("items", out var itemsElement))
                result.Uuids = itemsElement.EnumerateArray().Select(x => x.GetString()!).ToList();

            return result;
        }

        /// <summary>Returns a page of full product objects belonging to a catalog.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ProductUuidList"/> with HAL navigation links.</returns>
        public async Task<ProductUuidList> GetCatalogProductListAsync(string catalogId, int page = 1, int limit = 100, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/products?page={page}&limit={limit}";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var responseJson = AkeneoContextHelpers.DeserializeOrThrow<HalResponse>(responseString, url);

            var result = new ProductUuidList { Links = responseJson.Links };

            if (responseJson.Embedded != null && responseJson.Embedded.TryGetValue("items", out var itemsElement))
                result.Products = itemsElement.EnumerateArray().Select(x => x.Deserialize<ProductUuid>()!).ToList();

            return result;
        }

        /// <summary>Streams all product UUIDs belonging to a catalog, following HAL pagination automatically.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An async stream of product UUID strings.</returns>
        public async IAsyncEnumerable<string> StreamCatalogProductUuidsAsync(string catalogId, [EnumeratorCancellation] CancellationToken ct = default)
        {
            int page = 1;
            while (true)
            {
                var partial = await GetCatalogProductUuidListAsync(catalogId, page, 100, ct).ConfigureAwait(false);
                if (partial.Uuids == null || partial.Uuids.Count == 0)
                    yield break;
                foreach (var uuid in partial.Uuids)
                    yield return uuid;
                if (partial.Links?.Next == null)
                    yield break;
                page++;
            }
        }

        /// <summary>Returns all product UUIDs belonging to a catalog as a materialised list.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all product UUID strings.</returns>
        public async Task<List<string>> GetCatalogProductUuidListFullAsync(string catalogId, CancellationToken ct = default)
        {
            var list = new List<string>();
            await foreach (var uuid in StreamCatalogProductUuidsAsync(catalogId, ct))
                list.Add(uuid);
            return list;
        }

        /// <summary>Streams all full product objects belonging to a catalog, following HAL pagination automatically.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An async stream of <see cref="ProductUuid"/> objects.</returns>
        public async IAsyncEnumerable<ProductUuid> StreamCatalogProductsAsync(string catalogId, [EnumeratorCancellation] CancellationToken ct = default)
        {
            int page = 1;
            while (true)
            {
                var partial = await GetCatalogProductListAsync(catalogId, page, 100, ct).ConfigureAwait(false);
                if (partial.Products == null || partial.Products.Count == 0)
                    yield break;
                foreach (var product in partial.Products)
                    yield return product;
                if (partial.Links?.Next == null)
                    yield break;
                page++;
            }
        }

        /// <summary>Returns all full product objects belonging to a catalog as a materialised list.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="ProductUuid"/> objects.</returns>
        public async Task<List<ProductUuid>> GetCatalogProductListFullAsync(string catalogId, CancellationToken ct = default)
        {
            var list = new List<ProductUuid>();
            await foreach (var product in StreamCatalogProductsAsync(catalogId, ct))
                list.Add(product);
            return list;
        }

        /// <summary>Returns a single catalog product by its UUID.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="uuid">The product UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="ProductUuid"/>.</returns>
        public async Task<ProductUuid> GetCatalogProductAsync(string catalogId, string uuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/products/{Uri.EscapeDataString(uuid)}";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<ProductUuid>(responseString, url);
        }

        /// <summary>Returns a raw JSON string of mapped products for a catalog. The schema depends on the catalog's mapping configuration.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw JSON response string.</returns>
        public async Task<string> GetCatalogMappedProductListAsync(string catalogId, int page = 1, int limit = 100, CancellationToken ct = default)
        {
            return await _service.HttpGetAsync($"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/mapped-products?page={page}&limit={limit}", ct).ConfigureAwait(false);
        }

        /// <summary>Returns a raw JSON string of mapped product models for a catalog.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw JSON response string.</returns>
        public async Task<string> GetCatalogMappedModelListAsync(string catalogId, int page = 1, int limit = 100, CancellationToken ct = default)
        {
            return await _service.HttpGetAsync($"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/mapped-models?page={page}&limit={limit}", ct).ConfigureAwait(false);
        }

        /// <summary>Returns a raw JSON string of mapped variants of a product model for a catalog.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="modelCode">The product model code whose variants to retrieve.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Raw JSON response string.</returns>
        public async Task<string> GetCatalogMappedVariantListAsync(string catalogId, string modelCode, int page = 1, int limit = 100, CancellationToken ct = default)
        {
            return await _service.HttpGetAsync($"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/mapped-models/{Uri.EscapeDataString(modelCode)}/variants?page={page}&limit={limit}", ct).ConfigureAwait(false);
        }

        /// <summary>Returns the product mapping schema defined for a catalog.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The <see cref="CatalogMappingSchema"/> for the catalog.</returns>
        public async Task<CatalogMappingSchema> GetCatalogMappingSchemaAsync(string catalogId, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/mapping-schemas/product";
            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<CatalogMappingSchema>(responseString, url);
        }

        /// <summary>Creates a new catalog and returns the created entity.</summary>
        /// <param name="catalog">The catalog to create (must include at least <c>name</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created <see cref="Catalog"/>.</returns>
        public async Task<Catalog> CreateCatalogAsync(Catalog catalog, CancellationToken ct = default)
        {
            var url = "/api/rest/v1/catalogs";
            var body = JsonSerializer.Serialize(catalog);
            var responseString = await _service.HttpPostAsync(url, body, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<Catalog>(responseString, url);
        }

        /// <summary>Updates a catalog's properties (e.g. name, enabled) and returns the updated entity.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="catalog">The properties to update.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="Catalog"/>.</returns>
        public async Task<Catalog> UpdateCatalogAsync(string catalogId, Catalog catalog, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}";
            var body = JsonSerializer.Serialize(catalog);
            return await PatchAndFetchAsync(url, body, () => GetCatalogAsync(catalogId, ct), ct).ConfigureAwait(false);
        }

        /// <summary>Deletes a catalog.</summary>
        /// <param name="catalogId">The catalog UUID to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task DeleteCatalogAsync(string catalogId, CancellationToken ct = default)
        {
            await _service.HttpDeleteAsync($"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}", ct).ConfigureAwait(false);
        }

        /// <summary>Duplicates an existing catalog and returns the new catalog.</summary>
        /// <param name="catalogId">The UUID of the catalog to duplicate.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The newly created duplicate <see cref="Catalog"/>.</returns>
        public async Task<Catalog> DuplicateCatalogAsync(string catalogId, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/duplicate";
            var responseString = await _service.HttpPostAsync(url, string.Empty, ct).ConfigureAwait(false);
            return AkeneoContextHelpers.DeserializeOrThrow<Catalog>(responseString, url);
        }

        /// <summary>Creates or replaces the product mapping schema for a catalog.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="schemaJson">The mapping schema as a raw JSON string.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response body string (typically empty on success).</returns>
        public async Task<string> SetCatalogMappingSchemaAsync(string catalogId, string schemaJson, CancellationToken ct = default)
        {
            return await _service.HttpPutAsync($"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/mapping-schemas/product", schemaJson, ct).ConfigureAwait(false);
        }

        /// <summary>Deletes the product mapping schema for a catalog.</summary>
        /// <param name="catalogId">The catalog UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task DeleteCatalogMappingSchemaAsync(string catalogId, CancellationToken ct = default)
        {
            await _service.HttpDeleteAsync($"/api/rest/v1/catalogs/{Uri.EscapeDataString(catalogId)}/mapping-schemas/product", ct).ConfigureAwait(false);
        }

        #endregion


    }
}

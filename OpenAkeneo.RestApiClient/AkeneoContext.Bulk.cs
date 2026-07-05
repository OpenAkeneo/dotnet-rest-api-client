using OpenAkeneo.RestApiClient.Models;
using System.Text;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Bulk create-or-update (Akeneo batch PATCH endpoints)

        /// <summary>Akeneo caps batch PATCH calls at 100 items; larger inputs are chunked transparently.</summary>
        private const int BulkChunkSize = 100;

        private const string CollectionContentType = "application/vnd.akeneo.collection+json";

        /// <summary>
        /// Sends items to a newline-delimited-JSON batch endpoint
        /// (<c>Content-Type: application/vnd.akeneo.collection+json</c>), chunking by 100 and
        /// renumbering result lines to absolute positions across chunks.
        /// </summary>
        private async Task<List<BulkItemResult>> BulkUpsertNdjsonAsync<T>(string url, IEnumerable<T> items, CancellationToken ct)
        {
            var results = new List<BulkItemResult>();
            var offset = 0;

            foreach (var chunk in items.Chunk(BulkChunkSize))
            {
                var body = new StringBuilder();
                foreach (var item in chunk)
                    body.AppendLine(JsonSerializer.Serialize(item));

                var response = await _service.HttpPatchAsync(url, body.ToString(), CollectionContentType, ct).ConfigureAwait(false);

                // The response is one JSON object per line, mirroring the request lines.
                foreach (var line in response.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    var result = AkeneoContextHelpers.DeserializeOrThrow<BulkItemResult>(line, url);
                    result.Line += offset;
                    results.Add(result);
                }

                offset += chunk.Length;
            }

            return results;
        }

        /// <summary>
        /// Sends items to a JSON-array batch endpoint (reference-entity records and assets),
        /// chunking by 100 and renumbering result lines to absolute positions across chunks.
        /// </summary>
        private async Task<List<BulkItemResult>> BulkUpsertJsonArrayAsync<T>(string url, IEnumerable<T> items, CancellationToken ct)
        {
            var results = new List<BulkItemResult>();
            var offset = 0;

            foreach (var chunk in items.Chunk(BulkChunkSize))
            {
                var body = JsonSerializer.Serialize(chunk);
                var response = await _service.HttpPatchAsync(url, body, ct).ConfigureAwait(false);

                var chunkResults = AkeneoContextHelpers.DeserializeOrThrow<List<BulkItemResult>>(response, url);
                for (var i = 0; i < chunkResults.Count; i++)
                {
                    // These endpoints do not return a line number — synthesise an absolute one.
                    if (chunkResults[i].Line == 0)
                        chunkResults[i].Line = offset + i + 1;
                    else
                        chunkResults[i].Line += offset;
                }
                results.AddRange(chunkResults);

                offset += chunk.Length;
            }

            return results;
        }

        /// <summary>Creates or updates several UUID-keyed products in one call per 100 items.</summary>
        /// <param name="products">The products to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted product, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateProductUuidsAsync(IEnumerable<ProductUuid> products, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/products-uuid", products, ct);

        /// <summary>Creates or updates several identifier-keyed products in one call per 100 items.</summary>
        /// <param name="products">The products to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted product, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateProductIdentifiersAsync(IEnumerable<ProductIdentifier> products, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/products", products, ct);

        /// <summary>Creates or updates several product models in one call per 100 items.</summary>
        /// <param name="productModels">The product models to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted model, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateProductModelsAsync(IEnumerable<ProductModel> productModels, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/product-models", productModels, ct);

        /// <summary>Creates or updates several families in one call per 100 items.</summary>
        /// <param name="families">The families to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted family, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateFamiliesAsync(IEnumerable<Family> families, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/families", families, ct);

        /// <summary>Creates or updates several variants of one family in one call per 100 items.</summary>
        /// <param name="familyCode">The parent family code.</param>
        /// <param name="variants">The family variants to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted variant, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateFamilyVariantsAsync(string familyCode, IEnumerable<FamilyVariant> variants, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync($"/api/rest/v1/families/{Uri.EscapeDataString(familyCode)}/variants", variants, ct);

        /// <summary>Creates or updates several attributes in one call per 100 items.</summary>
        /// <param name="attributes">The attributes to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted attribute, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateAttributesAsync(IEnumerable<AkeneoAttribute> attributes, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/attributes", attributes, ct);

        /// <summary>Creates or updates several options of one attribute in one call per 100 items.</summary>
        /// <param name="attributeCode">The parent attribute code.</param>
        /// <param name="options">The options to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted option, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateAttributeOptionsAsync(string attributeCode, IEnumerable<AttributeOption> options, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync($"/api/rest/v1/attributes/{Uri.EscapeDataString(attributeCode)}/options", options, ct);

        /// <summary>Creates or updates several attribute groups in one call per 100 items.</summary>
        /// <param name="attributeGroups">The attribute groups to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted group, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateAttributeGroupsAsync(IEnumerable<AttributeGroup> attributeGroups, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/attribute-groups", attributeGroups, ct);

        /// <summary>Creates or updates several association types in one call per 100 items.</summary>
        /// <param name="associationTypes">The association types to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted association type, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateAssociationTypesAsync(IEnumerable<AssociationType> associationTypes, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/association-types", associationTypes, ct);

        /// <summary>Creates or updates several channels in one call per 100 items.</summary>
        /// <param name="channels">The channels to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted channel, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateChannelsAsync(IEnumerable<Channel> channels, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/channels", channels, ct);

        /// <summary>Creates or updates several categories in one call per 100 items.</summary>
        /// <param name="categories">The categories to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted category, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateCategoriesAsync(IEnumerable<Category> categories, CancellationToken ct = default)
            => BulkUpsertNdjsonAsync("/api/rest/v1/categories", categories, ct);

        /// <summary>Creates or updates several records of one reference entity in one call per 100 items.</summary>
        /// <param name="referenceEntityCode">The reference entity code.</param>
        /// <param name="records">The records to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted record, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateReferenceEntityRecordsAsync(string referenceEntityCode, IEnumerable<ReferenceEntityRecord> records, CancellationToken ct = default)
            => BulkUpsertJsonArrayAsync($"/api/rest/v1/reference-entities/{Uri.EscapeDataString(referenceEntityCode)}/records", records, ct);

        /// <summary>Creates or updates several assets of one asset family in one call per 100 items.</summary>
        /// <param name="assetFamilyCode">The asset family code.</param>
        /// <param name="assets">The assets to upsert.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One <see cref="BulkItemResult"/> per submitted asset, in order.</returns>
        public Task<List<BulkItemResult>> BulkCreateOrUpdateAssetsAsync(string assetFamilyCode, IEnumerable<Asset> assets, CancellationToken ct = default)
            => BulkUpsertJsonArrayAsync($"/api/rest/v1/asset-families/{Uri.EscapeDataString(assetFamilyCode)}/assets", assets, ct);

        #endregion

    }
}

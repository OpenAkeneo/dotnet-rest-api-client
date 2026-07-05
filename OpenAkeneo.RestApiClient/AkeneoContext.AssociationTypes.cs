using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Association type

        /// <summary>Streams all association types, following HAL pagination automatically.</summary>
        /// <param name="withCount">When <c>true</c>, the response includes the total item count.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<AssociationType> StreamAssociationTypesAsync(bool withCount = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetAssociationTypeListAsync(page, 100, withCount, ct).ConfigureAwait(false);
                foreach (var item in partial.AssociationTypes)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        /// <summary>Returns all association types as a materialised list.</summary>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="AssociationType"/> objects.</returns>
        public async Task<List<AssociationType>> GetAssociationTypeListFullAsync(bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<AssociationType>();
            await foreach (var item in StreamAssociationTypesAsync(withCount, ct).ConfigureAwait(false))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of association types.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="withCount">Include total count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AssociationTypeList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<AssociationTypeList> GetAssociationTypeListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetAssociationTypeListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns an association type page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="AssociationTypeList"/> with HAL navigation links.</returns>
        public async Task<AssociationTypeList> GetAssociationTypeListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/association-types";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<AssociationType>(responseString, url);

            return new AssociationTypeList { Links = links, AssociationTypes = items };
        }

        /// <summary>Returns a single association type by its code.</summary>
        /// <param name="code">The association type code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="AssociationType"/>.</returns>
        public async Task<AssociationType> GetAssociationTypeAsync(string code, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/association-types/{Uri.EscapeDataString(code)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<AssociationType>(responseString, url);
        }

        /// <summary>Creates or updates an association type via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="associationType">The association type to create or update. <see cref="AssociationType.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="AssociationType"/> as returned by the API.</returns>
        public async Task<AssociationType> CreateOrUpdateAssociationTypeAsync(AssociationType associationType, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/association-types/{Uri.EscapeDataString(associationType.Code)}";
            var body = JsonSerializer.Serialize(associationType);

            return await PatchAndFetchAsync(url, body, () => GetAssociationTypeAsync(associationType.Code, ct), ct).ConfigureAwait(false);
        }

        /// <summary>Creates a new association type via HTTP POST and returns the created entity.</summary>
        /// <param name="associationType">The association type to create.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The created <see cref="AssociationType"/>.</returns>
        public async Task<AssociationType> CreateAssociationTypeAsync(AssociationType associationType, CancellationToken ct = default)
        {
            // POST returns 201 with an empty body per the Akeneo spec, so fetch the created entity.
            var body = JsonSerializer.Serialize(associationType);
            await _service.HttpPostAsync("/api/rest/v1/association-types", body, ct).ConfigureAwait(false);
            return await GetAssociationTypeAsync(associationType.Code, ct).ConfigureAwait(false);
        }

        #endregion

    }
}

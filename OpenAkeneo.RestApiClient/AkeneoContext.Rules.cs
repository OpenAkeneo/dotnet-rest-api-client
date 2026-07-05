using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Rule definitions

        /// <summary>Streams all rule definitions, following HAL pagination automatically.</summary>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<RuleDefinition> StreamRuleDefinitionsAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetRuleDefinitionListAsync(page, 100, ct: ct).ConfigureAwait(false);
                if (partial.RuleDefinitions != null)
                    foreach (var item in partial.RuleDefinitions)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.RuleDefinitions == null)
                    yield break;
            }
        }

        /// <summary>Returns all rule definitions as a materialised list.</summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="RuleDefinition"/> objects.</returns>
        public async Task<List<RuleDefinition>> GetRuleDefinitionListFullAsync(CancellationToken ct = default)
        {
            var list = new List<RuleDefinition>();
            await foreach (var item in StreamRuleDefinitionsAsync(ct).ConfigureAwait(false))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of rule definitions.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="withCount">Include total count in the API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="RuleDefinitionList"/> with HAL navigation links.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/> or <paramref name="limit"/> is out of range.</exception>
        public async Task<RuleDefinitionList> GetRuleDefinitionListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString(),
                ["with_count"] = withCount ? "true" : "false"
            };

            return await GetRuleDefinitionListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a rule definition page using an arbitrary set of pre-built query parameters
        /// (the endpoint also supports <c>codes</c>, <c>type</c>, <c>execution_type</c> and <c>enabled</c> filters).</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="RuleDefinitionList"/> with HAL navigation links.</returns>
        public async Task<RuleDefinitionList> GetRuleDefinitionListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/rule-definitions";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<RuleDefinition>(responseString, url);

            return new RuleDefinitionList { Links = links, RuleDefinitions = items };
        }

        /// <summary>Returns a single rule definition by its code.</summary>
        /// <param name="code">The rule code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="RuleDefinition"/>.</returns>
        public async Task<RuleDefinition> GetRuleDefinitionAsync(string code, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/rule-definitions/{Uri.EscapeDataString(code)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<RuleDefinition>(responseString, url);
        }

        /// <summary>Creates or replaces a rule definition via HTTP PUT then returns the refreshed entity.</summary>
        /// <param name="rule">The rule to create or replace. <see cref="RuleDefinition.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The stored <see cref="RuleDefinition"/> as returned by the API.</returns>
        public async Task<RuleDefinition> CreateOrReplaceRuleDefinitionAsync(RuleDefinition rule, CancellationToken ct = default)
        {
            // PUT returns 201/204 with no reliable body per the Akeneo spec, so fetch afterwards.
            var url = $"/api/rest/v1/rule-definitions/{Uri.EscapeDataString(rule.Code)}";
            var body = JsonSerializer.Serialize(rule);
            await _service.HttpPutAsync(url, body, ct).ConfigureAwait(false);
            return await GetRuleDefinitionAsync(rule.Code, ct).ConfigureAwait(false);
        }

        #endregion

    }
}

using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Workflows

        /// <summary>
        /// Streams all workflows, following HAL pagination automatically.
        /// Note: Workflows are an optional Akeneo Enterprise feature. The endpoint may not respond
        /// if the feature is disabled; apply a caller-side timeout when consuming this stream.
        /// </summary>
        /// <param name="ct">Cancellation token. Pass a <see cref="CancellationTokenSource"/> with a timeout
        /// to guard against endpoints that hang instead of returning an error.</param>
        public async IAsyncEnumerable<Workflow> StreamWorkflowsAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetWorkflowListAsync(page, 100, ct).ConfigureAwait(false);
                if (partial.Workflows != null)
                    foreach (var item in partial.Workflows)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.Workflows == null || partial.Workflows.Count == 0)
                    yield break;
            }
        }

        /// <summary>Returns all workflows as a materialised list.</summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="Workflow"/> objects.</returns>
        public async Task<List<Workflow>> GetWorkflowListFullAsync(CancellationToken ct = default)
        {
            var list = new List<Workflow>();
            await foreach (var item in StreamWorkflowsAsync(ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of workflows.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="WorkflowList"/> with HAL navigation links.</returns>
        public async Task<WorkflowList> GetWorkflowListAsync(int page = 1, int limit = 100, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            return await GetWorkflowListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a workflow page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="WorkflowList"/> with HAL navigation links.</returns>
        public async Task<WorkflowList> GetWorkflowListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/workflows";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Workflow>(responseString, url);

            return new WorkflowList { Links = links, Workflows = items };
        }

        /// <summary>Returns a single workflow by its UUID.</summary>
        /// <param name="uuid">The workflow UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Workflow"/>.</returns>
        public async Task<Workflow> GetWorkflowAsync(string uuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/workflows/{Uri.EscapeDataString(uuid)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Workflow>(responseString, url);
        }

        #endregion


        #region Workflow step assignees

        /// <summary>Streams all assignees for a given workflow step, following HAL pagination automatically.</summary>
        /// <param name="stepUuid">The workflow step UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<WorkflowStepAssignee> StreamWorkflowStepAssigneesAsync(string stepUuid, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetWorkflowStepAssigneeListAsync(stepUuid, page, 100, ct).ConfigureAwait(false);
                if (partial.Assignees != null)
                    foreach (var item in partial.Assignees)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.Assignees == null || partial.Assignees.Count == 0)
                    yield break;
            }
        }

        /// <summary>Returns all assignees for a workflow step as a materialised list.</summary>
        /// <param name="stepUuid">The workflow step UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="WorkflowStepAssignee"/> objects.</returns>
        public async Task<List<WorkflowStepAssignee>> GetWorkflowStepAssigneeListFullAsync(string stepUuid, CancellationToken ct = default)
        {
            var list = new List<WorkflowStepAssignee>();
            await foreach (var item in StreamWorkflowStepAssigneesAsync(stepUuid, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of assignees for a workflow step.</summary>
        /// <param name="stepUuid">The workflow step UUID.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="WorkflowStepAssigneeList"/> with HAL navigation links.</returns>
        public async Task<WorkflowStepAssigneeList> GetWorkflowStepAssigneeListAsync(string stepUuid, int page = 1, int limit = 100, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            return await GetWorkflowStepAssigneeListAsync(stepUuid, queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a workflow step assignee page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="stepUuid">The workflow step UUID.</param>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="WorkflowStepAssigneeList"/> with HAL navigation links.</returns>
        public async Task<WorkflowStepAssigneeList> GetWorkflowStepAssigneeListAsync(string stepUuid, Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/workflows/steps/{Uri.EscapeDataString(stepUuid)}/assignees";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<WorkflowStepAssignee>(responseString, url);

            return new WorkflowStepAssigneeList { Links = links, Assignees = items };
        }

        #endregion


        #region Workflow tasks

        /// <summary>Streams all workflow tasks, following HAL pagination automatically.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withAttributes">When <c>true</c>, includes attribute values in the response.</param>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<WorkflowTask> StreamWorkflowTasksAsync(string? search = null, bool withAttributes = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetWorkflowTaskListAsync(page, 100, search, withAttributes, ct).ConfigureAwait(false);
                if (partial.Tasks != null)
                    foreach (var item in partial.Tasks)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.Tasks == null || partial.Tasks.Count == 0)
                    yield break;
            }
        }

        /// <summary>Returns all workflow tasks as a materialised list.</summary>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withAttributes">When <c>true</c>, includes attribute values in the response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="WorkflowTask"/> objects.</returns>
        public async Task<List<WorkflowTask>> GetWorkflowTaskListFullAsync(string? search = null, bool withAttributes = false, CancellationToken ct = default)
        {
            var list = new List<WorkflowTask>();
            await foreach (var item in StreamWorkflowTasksAsync(search, withAttributes, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of workflow tasks.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="search">Optional JSON-encoded search filter.</param>
        /// <param name="withAttributes">When <c>true</c>, includes attribute values in the response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="WorkflowTaskList"/> with HAL navigation links.</returns>
        public async Task<WorkflowTaskList> GetWorkflowTaskListAsync(int page = 1, int limit = 100, string? search = null, bool withAttributes = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            if (!string.IsNullOrEmpty(search))
                queryParameters["search"] = search;

            queryParameters["with_attributes"] = withAttributes ? "true" : "false";

            return await GetWorkflowTaskListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a workflow task page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="WorkflowTaskList"/> with HAL navigation links.</returns>
        public async Task<WorkflowTaskList> GetWorkflowTaskListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/workflows/tasks";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<WorkflowTask>(responseString, url);

            return new WorkflowTaskList { Links = links, Tasks = items };
        }

        /// <summary>Returns a single workflow task by its UUID.</summary>
        /// <param name="uuid">The workflow task UUID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="WorkflowTask"/>.</returns>
        public async Task<WorkflowTask> GetWorkflowTaskAsync(string uuid, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/workflows/tasks/{Uri.EscapeDataString(uuid)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<WorkflowTask>(responseString, url);
        }

        #endregion


    }
}

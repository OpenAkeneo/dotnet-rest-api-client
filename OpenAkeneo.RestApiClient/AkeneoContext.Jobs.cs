using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Jobs

        /// <summary>Streams all job definitions, following HAL pagination automatically.</summary>
        /// <param name="ct">Cancellation token.</param>
        public async IAsyncEnumerable<Job> StreamJobsAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetJobListAsync(page, 100, ct).ConfigureAwait(false);
                if (partial.Jobs != null)
                    foreach (var item in partial.Jobs)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.Jobs == null || partial.Jobs.Count == 0)
                    yield break;
            }
        }

        public async Task<List<Job>> GetJobListFullAsync(CancellationToken ct = default)
        {
            var list = new List<Job>();
            await foreach (var item in StreamJobsAsync(ct))
                list.Add(item);
            return list;
        }

        public async Task<JobList> GetJobListAsync(int page = 1, int limit = 100, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            return await GetJobListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<JobList> GetJobListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/jobs";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Job>(responseString, url);

            return new JobList { Links = links, Jobs = items };
        }

        public async Task<Job> GetJobAsync(string code, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/jobs/{Uri.EscapeDataString(code)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Job>(responseString, url);
        }

        public async IAsyncEnumerable<JobExecutionBrief> StreamJobExecutionsAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetJobExecutionListAsync(page, 100, ct).ConfigureAwait(false);
                if (partial.JobExecutions != null)
                    foreach (var item in partial.JobExecutions)
                        yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href) || partial.JobExecutions == null || partial.JobExecutions.Count == 0)
                    yield break;
            }
        }

        public async Task<List<JobExecutionBrief>> GetJobExecutionListFullAsync(CancellationToken ct = default)
        {
            var list = new List<JobExecutionBrief>();
            await foreach (var item in StreamJobExecutionsAsync(ct))
                list.Add(item);
            return list;
        }

        public async Task<JobExecutionList> GetJobExecutionListAsync(int page = 1, int limit = 100, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["limit"] = limit.ToString()
            };

            return await GetJobExecutionListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<JobExecutionList> GetJobExecutionListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/jobs/executions";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<JobExecutionBrief>(responseString, url);

            return new JobExecutionList { Links = links, JobExecutions = items };
        }

        public async Task<JobExecution> GetJobExecutionAsync(int id, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/jobs/executions/{id}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<JobExecution>(responseString, url);
        }

        /// <summary>Launches an export job by its code.</summary>
        /// <param name="code">The export job code.</param>
        /// <param name="isDryRun">When <c>true</c>, the job runs in dry-run mode without committing changes.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="JobExecutionResult"/> containing the new execution ID.</returns>
        public async Task<JobExecutionResult> LaunchExportJobAsync(string code, bool isDryRun = false, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/jobs/export/{Uri.EscapeDataString(code)}";
            var body = JsonSerializer.Serialize(new { is_dry_run = isDryRun });

            var responseString = await _service.HttpPostAsync(url, body, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<JobExecutionResult>(responseString, url);
        }

        /// <summary>Launches an import job by its code.</summary>
        /// <param name="code">The import job code.</param>
        /// <param name="isDryRun">When <c>true</c>, the job runs in dry-run mode without committing changes.</param>
        /// <param name="importMode">Optional import mode override (e.g. <c>"add_or_update"</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="JobExecutionResult"/> containing the new execution ID.</returns>
        public async Task<JobExecutionResult> LaunchImportJobAsync(string code, bool isDryRun = false, string? importMode = null, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/jobs/import/{Uri.EscapeDataString(code)}";

            var bodyObj = new Dictionary<string, object>();
            bodyObj["is_dry_run"] = isDryRun;
            if (!string.IsNullOrEmpty(importMode))
                bodyObj["import_mode"] = importMode;

            var body = JsonSerializer.Serialize(bodyObj);
            var responseString = await _service.HttpPostAsync(url, body, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<JobExecutionResult>(responseString, url);
        }

        #endregion

    }
}

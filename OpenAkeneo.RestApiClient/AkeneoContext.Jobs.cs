using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Jobs

        // NOTE: Only the export/import launch endpoints below are part of the public Akeneo
        // OpenAPI specification. The previously-present GET /jobs, GET /jobs/{code},
        // GET /jobs/executions and GET /jobs/executions/{id} methods were undocumented and have
        // been removed. The Job, JobList, JobExecution(Brief/List) models are retained as response
        // shapes for callers that deserialize raw payloads.

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

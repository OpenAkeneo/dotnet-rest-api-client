using System;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Job

    /// <summary>Paginated list of import/export job profiles.</summary>
    public class JobList : HalBaseInheritance
    {
        /// <summary>Job profiles on the current page.</summary>
        public List<Job> Jobs { get; set; } = new();
    }

    /// <summary>An Akeneo import/export job profile definition.</summary>
    public class Job
    {
        /// <summary>Unique code identifying the job profile.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string?> Labels { get; set; } = new();

        /// <summary>Job type (e.g. <c>import</c>, <c>export</c>).</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;
    }

    /// <summary>Result returned after launching a job, containing the new execution ID.</summary>
    public class JobExecutionResult
    {

        /// <summary>ID of the newly created job execution.</summary>
        [JsonPropertyName("job_execution_id")]
        public int JobExecutionId { get; set; }

    }

    /// <summary>Paginated list of job execution summaries.</summary>
    public class JobExecutionList : HalBaseInheritance
    {
        /// <summary>Job execution summaries on the current page.</summary>
        public List<JobExecutionBrief> JobExecutions { get; set; } = new();
    }

    /// <summary>Summary information about a job execution.</summary>
    public class JobExecutionBrief
    {
        /// <summary>Unique numeric ID of this job execution.</summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>Code of the job profile that was executed.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Current execution status (e.g. <c>COMPLETED</c>, <c>FAILED</c>, <c>IN_PROGRESS</c>).</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        /// <summary>Username of the user who launched the job.</summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = default!;

        /// <summary>Timestamp when the execution started; <c>null</c> if not yet started.</summary>
        [JsonPropertyName("started")]
        [JsonConverter(typeof(AkeneoDateTimeOffsetConverter))]
        public DateTimeOffset? Started { get; set; }

        /// <summary>Timestamp when the execution finished; <c>null</c> if still running.</summary>
        [JsonPropertyName("stopped")]
        [JsonConverter(typeof(AkeneoDateTimeOffsetConverter))]
        public DateTimeOffset? Stopped { get; set; }
    }

    /// <summary>Detailed information about a completed job execution, including warning and error counts.</summary>
    public class JobExecution : JobExecutionBrief
    {
        /// <summary>Number of warnings encountered during the execution.</summary>
        [JsonPropertyName("warning_count")]
        public int WarningCount { get; set; }

        /// <summary>Number of errors encountered during the execution.</summary>
        [JsonPropertyName("error_count")]
        public int ErrorCount { get; set; }
    }

    #endregion

}

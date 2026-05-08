using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAkeneo.RestApiClient.Converters;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Workflow

    /// <summary>Paginated list of workflows.</summary>
    public class WorkflowList : HalBaseInheritance
    {
        /// <summary>Workflows on the current page.</summary>
        public List<Workflow> Workflows { get; set; } = new();
    }

    /// <summary>An Akeneo workflow (publication process) definition.</summary>
    public class Workflow : HalItemInheritance
    {

        /// <summary>Unique UUID identifying the workflow.</summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = default!;

        /// <summary>Unique code identifying the workflow.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string?> Labels { get; set; } = new();

        /// <summary>Whether this workflow is currently active.</summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>Ordered list of steps that make up this workflow.</summary>
        [JsonPropertyName("steps")]
        public List<WorkflowStep>? Steps { get; set; }

    }

    /// <summary>A single step within a workflow.</summary>
    public class WorkflowStep
    {

        /// <summary>Unique UUID identifying this step.</summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = default!;

        /// <summary>Unique code identifying this step within the workflow.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

        /// <summary>Step type (e.g. <c>review</c>, <c>publish</c>).</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;

        /// <summary>Localised display labels keyed by locale code.</summary>
        [JsonPropertyName("labels")]
        public Dictionary<string, string?> Labels { get; set; } = new();

        /// <summary>Localised descriptions of the step's purpose, keyed by locale code.</summary>
        [JsonPropertyName("descriptions")]
        public Dictionary<string, string?> Descriptions { get; set; } = new();

        /// <summary>Time allotted for this step; format depends on configuration.</summary>
        [JsonPropertyName("allotted_time")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? AllottedTime { get; set; }

        /// <summary>Locale codes in scope for each channel at this step; keyed by channel code.</summary>
        [JsonPropertyName("channels_and_locales")]
        public Dictionary<string, List<string>> ChannelsAndLocales { get; set; } = new();

    }

    #endregion


    #region Workflow step assignee

    /// <summary>Paginated list of users assigned to a workflow step.</summary>
    public class WorkflowStepAssigneeList : HalBaseInheritance
    {
        /// <summary>Assignees on the current page.</summary>
        public List<WorkflowStepAssignee> Assignees { get; set; } = new();
    }

    /// <summary>A user assigned to review or action a workflow step.</summary>
    public class WorkflowStepAssignee
    {

        /// <summary>Unique UUID identifying the user.</summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = default!;

        /// <summary>User's first name.</summary>
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = default!;

        /// <summary>User's last name.</summary>
        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = default!;

        /// <summary>User's email address.</summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

    }

    #endregion


    #region Workflow task

    /// <summary>Paginated list of workflow tasks.</summary>
    public class WorkflowTaskList : HalBaseInheritance
    {
        /// <summary>Workflow tasks on the current page.</summary>
        public List<WorkflowTask> Tasks { get; set; } = new();
    }

    /// <summary>A workflow task representing a unit of review work on a product or product model.</summary>
    public class WorkflowTask : HalItemInheritance
    {

        /// <summary>Unique UUID identifying this task.</summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = default!;

        /// <summary>Current status of the task (e.g. <c>in_progress</c>, <c>done</c>).</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        /// <summary>ISO 8601 timestamp of when the task was created.</summary>
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = default!;

        /// <summary>The product this task is associated with, if applicable.</summary>
        [JsonPropertyName("product")]
        public WorkflowTaskProduct? Product { get; set; }

        /// <summary>The product model this task is associated with, if applicable.</summary>
        [JsonPropertyName("product_model")]
        public WorkflowTaskProductModel? ProductModel { get; set; }

        /// <summary>ISO 8601 date by which this task should be completed.</summary>
        [JsonPropertyName("due_date")]
        public string? DueDate { get; set; }

        /// <summary>Whether this task was rejected during the review step.</summary>
        [JsonPropertyName("rejected")]
        public bool Rejected { get; set; }

        /// <summary>
        /// Only included in the response when with_attributes=true is requested.
        /// </summary>
        [JsonPropertyName("pending_attributes")]
        [JsonConverter(typeof(PolymorphicDataConverter))]
        public object? PendingAttributes { get; set; }

    }

    /// <summary>Reference to the product associated with a workflow task.</summary>
    public class WorkflowTaskProduct
    {

        /// <summary>UUID of the associated product.</summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = default!;

    }

    /// <summary>Reference to the product model associated with a workflow task.</summary>
    public class WorkflowTaskProductModel
    {

        /// <summary>Code of the associated product model.</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = default!;

    }

    #endregion

}

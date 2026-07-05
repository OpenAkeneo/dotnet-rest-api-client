using System.Text.Json.Serialization;

namespace OpenAkeneo.RestApiClient.Models
{

    #region Workflow execution requests

    /// <summary>
    /// A single item for <c>POST /workflows/executions</c>: one workflow to start for one
    /// product OR one product model. Use the factory methods to build valid combinations.
    /// </summary>
    public class WorkflowExecutionRequest
    {

        /// <summary>The workflow to start.</summary>
        [JsonPropertyName("workflow")]
        public WorkflowRef Workflow { get; set; } = default!;

        /// <summary>The product to include, for product-scoped executions.</summary>
        [JsonPropertyName("product")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ProductRef? Product { get; set; }

        /// <summary>The product model to include, for model-scoped executions.</summary>
        [JsonPropertyName("product_model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ProductModelRef? ProductModel { get; set; }

        /// <summary>Builds an execution request for a product.</summary>
        public static WorkflowExecutionRequest ForProduct(string workflowUuid, string productUuid) => new()
        {
            Workflow = new WorkflowRef { Uuid = workflowUuid },
            Product = new ProductRef { Uuid = productUuid }
        };

        /// <summary>Builds an execution request for a product model.</summary>
        public static WorkflowExecutionRequest ForProductModel(string workflowUuid, string productModelCode) => new()
        {
            Workflow = new WorkflowRef { Uuid = workflowUuid },
            ProductModel = new ProductModelRef { Code = productModelCode }
        };

        /// <summary>Reference to a workflow by UUID.</summary>
        public class WorkflowRef
        {
            /// <summary>The workflow UUID.</summary>
            [JsonPropertyName("uuid")]
            public string Uuid { get; set; } = default!;
        }

        /// <summary>Reference to a product by UUID.</summary>
        public class ProductRef
        {
            /// <summary>The product UUID.</summary>
            [JsonPropertyName("uuid")]
            public string Uuid { get; set; } = default!;
        }

        /// <summary>Reference to a product model by code.</summary>
        public class ProductModelRef
        {
            /// <summary>The product model code.</summary>
            [JsonPropertyName("code")]
            public string Code { get; set; } = default!;
        }

    }

    #endregion

}

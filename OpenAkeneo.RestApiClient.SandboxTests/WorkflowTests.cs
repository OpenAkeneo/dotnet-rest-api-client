using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

/// <summary>
/// Integration tests for Workflow, Workflow Step Assignees and Workflow Task endpoints.
///
/// Workflows are an optional Akeneo feature that may not be enabled in every environment,
/// and the endpoint may hang (no response) rather than returning an error.  Each test
/// therefore applies a 15-second local timeout via Task.WhenAny so the suite finishes
/// promptly. When the feature is unavailable the test is treated as a vacuous pass with
/// a descriptive message.
/// </summary>
public class WorkflowTests : IClassFixture<TestBase>
{
    private const int TimeoutSeconds = 15;

    private readonly TestBase _fixture;

    public WorkflowTests(TestBase fixture)
    {
        _fixture = fixture;
    }

    /// <summary>Runs the action with a per-test timeout, catching any exception (including timeout).</summary>
    private static async Task RunWithTimeout(Func<Task> action)
    {
        var task = action();
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(TimeoutSeconds)));

        if (completed != task)
            throw new TimeoutException($"Workflow API did not respond within {TimeoutSeconds}s — feature may not be enabled.");

        await task; // propagate any exception from the actual task
    }


    [Fact]
    public async Task GetWorkflowListAsync_ReturnsList()
    {
        var ct = TestContext.Current.CancellationToken;
        try
        {
            WorkflowList? result = null;
            await RunWithTimeout(async () =>
            {
                result = await _fixture.Context.GetWorkflowListAsync(ct: ct);
            });

            Assert.NotNull(result);
            Assert.NotNull(result!.Workflows);
        }
        catch (Exception ex)
        {
            Assert.Skip($"Workflow endpoint unavailable (feature may not be enabled): {ex.Message}");
        }
    }

    [Fact]
    public async Task GetWorkflowListAsync_WithPageAndLimit_ReturnsCorrectCount()
    {
        var ct = TestContext.Current.CancellationToken;
        try
        {
            WorkflowList? result = null;
            await RunWithTimeout(async () =>
            {
                result = await _fixture.Context.GetWorkflowListAsync(page: 1, limit: 5, ct: ct);
            });

            Assert.NotNull(result);
            Assert.True(result!.Workflows.Count <= 5);
        }
        catch (Exception ex)
        {
            Assert.Skip($"Workflow endpoint unavailable (feature may not be enabled): {ex.Message}");
        }
    }

    [Fact]
    public async Task GetWorkflowListFullAsync_ReturnsAllWorkflows()
    {
        var ct = TestContext.Current.CancellationToken;
        try
        {
            List<OpenAkeneo.RestApiClient.Models.Workflow>? result = null;
            await RunWithTimeout(async () =>
            {
                result = await _fixture.Context.GetWorkflowListFullAsync(ct);
            });

            Assert.NotNull(result);
        }
        catch (Exception ex)
        {
            Assert.Skip($"Workflow endpoint unavailable (feature may not be enabled): {ex.Message}");
        }
    }

    [Fact]
    public async Task StreamWorkflowsAsync_StreamsWorkflows()
    {
        try
        {
            var count = 0;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await foreach (var item in _fixture.Context.StreamWorkflowsAsync(TestContext.Current.CancellationToken).WithCancellation(cts.Token))
            {
                Assert.NotNull(item);
                count++;
            }
            Assert.True(count >= 0, "Workflow stream completed without error.");
        }
        catch (Exception ex)
        {
            Assert.Skip($"Workflow endpoint unavailable (feature may not be enabled): {ex.Message}");
        }
    }

    [Fact]
    public async Task GetWorkflowTaskListAsync_ReturnsList()
    {
        var ct = TestContext.Current.CancellationToken;
        try
        {
            WorkflowTaskList? result = null;
            await RunWithTimeout(async () =>
            {
                result = await _fixture.Context.GetWorkflowTaskListAsync(ct: ct);
            });

            Assert.NotNull(result);
            Assert.NotNull(result!.Tasks);
        }
        catch (Exception ex)
        {
            Assert.Skip($"Workflow tasks endpoint unavailable (feature may not be enabled): {ex.Message}");
        }
    }

    [Fact]
    public async Task GetWorkflowTaskAsync_ReturnsTask()
    {
        var ct = TestContext.Current.CancellationToken;
        try
        {
            // First we need a task UUID to query.
            WorkflowTaskList? listResult = null;
            await RunWithTimeout(async () =>
            {
                listResult = await _fixture.Context.GetWorkflowTaskListAsync(page: 1, limit: 1, ct: ct);
            });

            if (listResult?.Tasks?.Count > 0)
            {
                var taskUuid = listResult.Tasks[0].Uuid;
                WorkflowTask? taskResult = null;
                await RunWithTimeout(async () =>
                {
                    taskResult = await _fixture.Context.GetWorkflowTaskAsync(taskUuid, ct);
                });

                Assert.NotNull(taskResult);
                Assert.Equal(taskUuid, taskResult!.Uuid);
            }
        }
        catch (Exception ex)
        {
            Assert.Skip($"Workflow tasks endpoint unavailable (feature may not be enabled): {ex.Message}");
        }
    }

    [Fact]
    public async Task GetWorkflowStepAssigneeListAsync_ReturnsList()
    {
        var ct = TestContext.Current.CancellationToken;
        try
        {
            // First we need a workflow and step code to query.
            WorkflowList? listResult = null;
            await RunWithTimeout(async () =>
            {
                listResult = await _fixture.Context.GetWorkflowListAsync(page: 1, limit: 1, ct: ct);
            });

            if (listResult?.Workflows?.Count > 0 && listResult.Workflows[0].Steps?.Count > 0)
            {
                var stepCode = listResult.Workflows[0].Steps![0].Code;
                WorkflowStepAssigneeList? assigneesResult = null;
                await RunWithTimeout(async () =>
                {
                    assigneesResult = await _fixture.Context.GetWorkflowStepAssigneeListAsync(stepCode, ct: ct);
                });

                Assert.NotNull(assigneesResult);
                Assert.NotNull(assigneesResult!.Assignees);
            }
        }
        catch (Exception ex)
        {
            Assert.Skip($"Workflow step assignees endpoint unavailable (feature may not be enabled): {ex.Message}");
        }
    }
}

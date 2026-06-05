using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class UtilityTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    public UtilityTests(TestBase fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetUserChannelsPermissionsAsync_ReturnsPermissionsOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetUserChannelsPermissionsAsync("admin", TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            Assert.Equal("admin", result.UserUuid);
        }
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("NotFound"))
        {
            Assert.Skip($"Endpoint forbidden or admin user not found: {ex.Message}");
        }
    }

    [Fact]
    public async Task GetUserLocalesPermissionsAsync_ReturnsPermissionsOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetUserLocalesPermissionsAsync("admin", TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            Assert.Equal("admin", result.UserUuid);
        }
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("NotFound"))
        {
            Assert.Skip($"Endpoint forbidden or admin user not found: {ex.Message}");
        }
    }

    [Fact]
    public async Task GetApiOverviewAsync_ReturnsOverview()
    {
        var result = await _fixture.Context.GetApiOverviewAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.NotNull(result.Host);
        Assert.NotNull(result.Routes);
    }

    [Fact]
    public async Task GetExtensionListAsync_ReturnsExtensionsOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetExtensionListAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(result);
        }
        // NOTE: NotFound is intentionally NOT swallowed here. A 404 means the endpoint path is
        // wrong (routing bug), which must fail the test. Forbidden means the feature is not
        // enabled on this tier, which is an environmental skip.
        catch (Exception ex) when (ex.Message.Contains("Forbidden"))
        {
            Assert.Skip($"Feature might not be enabled on this tier: {ex.Message}");
        }
    }

    [Fact]
    public async Task GetModelizationSuggestionListAsync_ReturnsListOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetModelizationSuggestionListAsync(limit: 5, ct: TestContext.Current.CancellationToken);
            Assert.NotNull(result);
        }
        // NOTE: NotFound is intentionally NOT swallowed here. A 404 means the endpoint path is
        // wrong (routing bug), which must fail the test. Forbidden/UnprocessableEntity mean the
        // Data Architect feature is not enabled, which is an environmental skip.
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("UnprocessableEntity"))
        {
            Assert.Skip($"Data Architect might not be enabled: {ex.Message}");
        }
    }
}

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
            var result = await _fixture.Context.GetUserChannelsPermissionsAsync("admin");
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
            var result = await _fixture.Context.GetUserLocalesPermissionsAsync("admin");
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
        var result = await _fixture.Context.GetApiOverviewAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Host);
        Assert.NotNull(result.Routes);
    }

    [Fact]
    public async Task GetExtensionListAsync_ReturnsExtensionsOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetExtensionListAsync(limit: 5);
            Assert.NotNull(result);
        }
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("NotFound"))
        {
            Assert.Skip($"Feature might not be enabled on this tier: {ex.Message}");
        }
    }

    [Fact]
    public async Task GetModelizationSuggestionListAsync_ReturnsListOrForbidden()
    {
        try
        {
            var result = await _fixture.Context.GetModelizationSuggestionListAsync(limit: 5);
            Assert.NotNull(result);
        }
        catch (Exception ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("NotFound") || ex.Message.Contains("UnprocessableEntity"))
        {
            Assert.Skip($"Data Architect might not be enabled: {ex.Message}");
        }
    }
}

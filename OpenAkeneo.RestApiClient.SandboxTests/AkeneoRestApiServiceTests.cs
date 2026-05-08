namespace OpenAkeneo.RestApiClient.SandboxTests;

public class AkeneoRestApiServiceTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    public AkeneoRestApiServiceTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetTokenAsync_ReturnsValidToken()
    {
        var token = await _fixture.ApiService.GetTokenAsync();

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task GetTokenAsync_CalledTwice_ReturnsSameTokenFromCache()
    {
        var token1 = await _fixture.ApiService.GetTokenAsync();
        var token2 = await _fixture.ApiService.GetTokenAsync();

        Assert.Equal(token1, token2);
    }

    [Fact]
    public async Task GetTokenAsync_ForceRefresh_ReturnsFreshToken()
    {
        var token1 = await _fixture.ApiService.GetTokenAsync();
        var token2 = await _fixture.ApiService.GetTokenAsync(forceRefresh: true);

        Assert.NotNull(token2);
        Assert.NotEmpty(token2);
    }

    [Fact]
    public async Task HttpGetAsync_SystemInformationEndpoint_ReturnsContent()
    {
        var result = await _fixture.ApiService.HttpGetAsync("/api/rest/v1/system-information");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}

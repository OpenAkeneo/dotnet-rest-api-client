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
        var token = await _fixture.ApiService.GetTokenAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task GetTokenAsync_CalledTwice_ReturnsSameTokenFromCache()
    {
        var ct = TestContext.Current.CancellationToken;
        var token1 = await _fixture.ApiService.GetTokenAsync(ct: ct);
        var token2 = await _fixture.ApiService.GetTokenAsync(ct: ct);

        Assert.Equal(token1, token2);
    }

    [Fact]
    public async Task GetTokenAsync_ForceRefresh_ReturnsFreshToken()
    {
        var ct = TestContext.Current.CancellationToken;
        var token1 = await _fixture.ApiService.GetTokenAsync(ct: ct);
        var token2 = await _fixture.ApiService.GetTokenAsync(forceRefresh: true, ct: ct);

        Assert.NotNull(token2);
        Assert.NotEmpty(token2);
    }

    [Fact]
    public async Task HttpGetAsync_SystemInformationEndpoint_ReturnsContent()
    {
        var result = await _fixture.ApiService.HttpGetAsync("/api/rest/v1/system-information", TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}

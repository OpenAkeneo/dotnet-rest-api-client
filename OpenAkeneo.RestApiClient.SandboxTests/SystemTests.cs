namespace OpenAkeneo.RestApiClient.SandboxTests;

public class SystemTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    public SystemTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetSystemInformationAsync_ReturnsSystemInformation()
    {
        var result = await _fixture.Context.GetSystemInformationAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Version);
        Assert.NotEmpty(result.Edition);
    }
}

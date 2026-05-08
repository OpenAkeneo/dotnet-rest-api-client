namespace OpenAkeneo.RestApiClient.SandboxTests;

public class LocaleTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string LocaleCode = "en_US";

    public LocaleTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetLocaleListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetLocaleListAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Locales);
    }

    [Fact]
    public async Task GetLocaleListAsync_WithPageAndLimit_ReturnsCorrectCount()
    {
        var result = await _fixture.Context.GetLocaleListAsync(page: 1, limit: 5);

        Assert.NotNull(result);
        Assert.True(result.Locales.Count <= 5);
    }

    [Fact]
    public async Task GetLocaleListFullAsync_ReturnsAllLocales()
    {
        var result = await _fixture.Context.GetLocaleListFullAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamLocalesAsync_StreamsLocales()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamLocalesAsync())
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetLocaleAsync_ReturnsLocale()
    {
        var result = await _fixture.Context.GetLocaleAsync(LocaleCode);

        Assert.NotNull(result);
        Assert.Equal(LocaleCode, result.Code);
    }
}

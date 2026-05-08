namespace OpenAkeneo.RestApiClient.SandboxTests;

public class CurrencyTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string CurrencyCode = "EUR";

    public CurrencyTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetCurrencyListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetCurrencyListAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Currencies);
    }

    [Fact]
    public async Task GetCurrencyListAsync_WithPageAndLimit_ReturnsCorrectCount()
    {
        var result = await _fixture.Context.GetCurrencyListAsync(page: 1, limit: 5);

        Assert.NotNull(result);
        Assert.True(result.Currencies.Count <= 5);
    }

    [Fact]
    public async Task GetCurrencyListFullAsync_ReturnsAllCurrencies()
    {
        var result = await _fixture.Context.GetCurrencyListFullAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamCurrenciesAsync_StreamsCurrencies()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamCurrenciesAsync())
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetCurrencyAsync_ReturnsCurrency()
    {
        var result = await _fixture.Context.GetCurrencyAsync(CurrencyCode);

        Assert.NotNull(result);
        Assert.Equal(CurrencyCode, result.Code);
    }
}

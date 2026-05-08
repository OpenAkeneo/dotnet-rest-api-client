namespace OpenAkeneo.RestApiClient.SandboxTests;

public class AttributeTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string AttributeCode = "accessories_care_instructions";

    public AttributeTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetAttributeListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAttributeListAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Attributes);
    }

    [Fact]
    public async Task GetAttributeListAsync_WithPageAndLimit_ReturnsCorrectCount()
    {
        var result = await _fixture.Context.GetAttributeListAsync(page: 1, limit: 5, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.Attributes.Count <= 5);
    }

    [Fact]
    public async Task GetAttributeListFullAsync_ReturnsAllAttributes()
    {
        var result = await _fixture.Context.GetAttributeListFullAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamAttributesAsync_StreamsAttributes()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamAttributesAsync(ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetAttributeAsync_ReturnsAttribute()
    {
        var result = await _fixture.Context.GetAttributeAsync(AttributeCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AttributeCode, result.Code);
        Assert.NotEmpty(result.Type);
        Assert.NotEmpty(result.Group);
    }
}

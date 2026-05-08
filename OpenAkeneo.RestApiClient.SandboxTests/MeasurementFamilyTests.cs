namespace OpenAkeneo.RestApiClient.SandboxTests;

public class MeasurementFamilyTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    public MeasurementFamilyTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetMeasurementFamilyListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetMeasurementFamilyListAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetMeasurementFamilyListAsync_ItemsHaveCode()
    {
        var result = await _fixture.Context.GetMeasurementFamilyListAsync(TestContext.Current.CancellationToken);

        Assert.All(result, family => Assert.False(string.IsNullOrEmpty(family.Code)));
    }

    [Fact]
    public async Task GetMeasurementFamilyListAsync_ItemsHaveStandardUnitCode()
    {
        var result = await _fixture.Context.GetMeasurementFamilyListAsync(TestContext.Current.CancellationToken);

        Assert.All(result, family => Assert.False(string.IsNullOrEmpty(family.StandardUnitCode)));
    }

    [Fact]
    public async Task GetMeasurementFamilyListAsync_ItemsHaveUnits()
    {
        var result = await _fixture.Context.GetMeasurementFamilyListAsync(TestContext.Current.CancellationToken);

        Assert.All(result, family => Assert.NotEmpty(family.Units));
    }
}

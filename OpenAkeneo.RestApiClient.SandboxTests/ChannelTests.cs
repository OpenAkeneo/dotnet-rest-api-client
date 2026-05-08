using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class ChannelTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string ChannelCode = "ecommerce";
    private const string OpenAkeneoTestChannelCode = "openakeneo_test_channel";

    public ChannelTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetChannelListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetChannelListAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Channels);
    }

    [Fact]
    public async Task GetChannelListAsync_WithPageAndLimit_ReturnsCorrectCount()
    {
        var result = await _fixture.Context.GetChannelListAsync(page: 1, limit: 5);

        Assert.NotNull(result);
        Assert.True(result.Channels.Count <= 5);
    }

    [Fact]
    public async Task GetChannelListFullAsync_ReturnsAllChannels()
    {
        var result = await _fixture.Context.GetChannelListFullAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamChannelsAsync_StreamsChannels()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamChannelsAsync())
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetChannelAsync_ReturnsChannel()
    {
        var result = await _fixture.Context.GetChannelAsync(ChannelCode);

        Assert.NotNull(result);
        Assert.Equal(ChannelCode, result.Code);
        Assert.NotEmpty(result.Locales);
        Assert.NotEmpty(result.Currencies);
    }


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateChannelAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        // Step 1 — Create the channel with a known category tree and locale.
        var created = new Channel
        {
            Code = OpenAkeneoTestChannelCode,
            Currencies = new List<string> { "EUR" },
            Locales = new List<string> { "en_US" },
            CategoryTree = "master",
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Channel" }
        };
        var createResult = await _fixture.Context.CreateOrUpdateChannelAsync(created);

        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoTestChannelCode, createResult.Code);
        Assert.Contains("en_US", createResult.Locales);
        Assert.Equal("(OpenAkeneo) Test Channel", createResult.Labels?["en_US"]);

        // Step 2 — Patch the label and verify.
        var updated = new Channel
        {
            Code = OpenAkeneoTestChannelCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Channel Updated" }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateChannelAsync(updated);

        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoTestChannelCode, updateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Channel Updated", updateResult.Labels?["en_US"]);
    }

    #endregion
}

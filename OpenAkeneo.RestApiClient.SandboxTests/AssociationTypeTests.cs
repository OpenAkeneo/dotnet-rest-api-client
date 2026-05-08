using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class AssociationTypeTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string AssociationTypeCode = "X_SELL";
    private const string OpenAkeneoTestAssociationTypeCode = "OPENAKENEO_TEST_ASSOC";

    public AssociationTypeTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetAssociationTypeListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetAssociationTypeListAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AssociationTypes);
    }

    [Fact]
    public async Task GetAssociationTypeListFullAsync_ReturnsAllAssociationTypes()
    {
        var result = await _fixture.Context.GetAssociationTypeListFullAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamAssociationTypesAsync_StreamsAssociationTypes()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamAssociationTypesAsync(ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetAssociationTypeAsync_ReturnsAssociationType()
    {
        var result = await _fixture.Context.GetAssociationTypeAsync(AssociationTypeCode, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AssociationTypeCode, result.Code);
    }


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateAssociationTypeAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Step 1 — Create the association type.
        var created = new AssociationType
        {
            Code = OpenAkeneoTestAssociationTypeCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Association" }
        };
        var createResult = await _fixture.Context.CreateOrUpdateAssociationTypeAsync(created, ct);

        Assert.NotNull(createResult);
        Assert.Equal(OpenAkeneoTestAssociationTypeCode, createResult.Code);
        Assert.Equal("(OpenAkeneo) Test Association", createResult.Labels?["en_US"]);

        // Step 2 — Patch the label and verify.
        var updated = new AssociationType
        {
            Code = OpenAkeneoTestAssociationTypeCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Association Updated" }
        };
        var updateResult = await _fixture.Context.CreateOrUpdateAssociationTypeAsync(updated, ct);

        Assert.NotNull(updateResult);
        Assert.Equal(OpenAkeneoTestAssociationTypeCode, updateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Association Updated", updateResult.Labels?["en_US"]);
    }

    #endregion
}

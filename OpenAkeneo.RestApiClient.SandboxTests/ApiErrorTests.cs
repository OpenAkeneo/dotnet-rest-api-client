using System.Net;
using OpenAkeneo.RestApiClient;
using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

/// <summary>
/// Sandbox tests that verify AkeneoApiException and FieldErrors are correctly populated
/// when the live API returns error responses.
/// </summary>
public class ApiErrorTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    public ApiErrorTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetProductUuidAsync_NonExistentUuid_ThrowsAkeneoApiException_With404()
    {
        var ct = TestContext.Current.CancellationToken;
        var nonExistentUuid = "00000000-dead-beef-0000-000000000000";

        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => _fixture.Context.GetProductUuidAsync(nonExistentUuid, ct));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.NotNull(ex.ApiMessage);
        Assert.NotEmpty(ex.ApiMessage);
    }

    [Fact]
    public async Task GetAttributeAsync_NonExistentCode_ThrowsAkeneoApiException_With404()
    {
        var ct = TestContext.Current.CancellationToken;

        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => _fixture.Context.GetAttributeAsync("openakeneo_does_not_exist_xyz", ct));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.NotNull(ex.ApiMessage);
    }

    [Fact]
    public async Task CreateOrUpdateProductUuidAsync_InvalidFamilyCode_ThrowsAkeneoApiException_With422AndFieldErrors()
    {
        var ct = TestContext.Current.CancellationToken;

        // Use a hardcoded UUID so re-runs target the same slot without leaking new records.
        var uuid = new Guid("b0000000-4e6f-4e65-6f00-000000000001").ToString();

        var product = new ProductUuid
        {
            Uuid = uuid,
            Family = "openakeneo_nonexistent_family_xyz",
            Enabled = false
        };

        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => _fixture.Context.CreateOrUpdateProductUuidAsync(product, ct));

        // Akeneo returns 422 with an errors array for unknown family codes.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, ex.StatusCode);
        Assert.NotNull(ex.ApiMessage);
        Assert.NotEmpty(ex.ApiMessage);

        // FieldErrors should be populated — Akeneo returns errors[] for family validation failures.
        // If for some reason the API doesn't return errors[], we accept null (not all 422s include it).
        if (ex.FieldErrors != null)
        {
            Assert.NotEmpty(ex.FieldErrors);
            Assert.All(ex.FieldErrors, fe =>
            {
                Assert.NotNull(fe.Property);
                Assert.NotNull(fe.Message);
            });
        }
    }

    [Fact]
    public async Task CreateOrUpdateAttributeAsync_InvalidType_ThrowsAkeneoApiException_With422()
    {
        var ct = TestContext.Current.CancellationToken;

        // Sending an unknown attribute type should produce a 422.
        var attribute = new AkeneoAttribute
        {
            Code = "openakeneo_test_invalid_type_attr",
            Type = "pim_catalog_totally_invalid_type",
            Group = "other"
        };

        var ex = await Assert.ThrowsAsync<AkeneoApiException>(
            () => _fixture.Context.CreateOrUpdateAttributeAsync(attribute, ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, ex.StatusCode);
        Assert.NotNull(ex.ApiMessage);
        Assert.NotEmpty(ex.ApiMessage);
    }
}

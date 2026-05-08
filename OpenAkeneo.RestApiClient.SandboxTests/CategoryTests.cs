using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class CategoryTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    private const string CategoryCode = "0001";
    private const string MediaFileCode = "c/7/3/c/c73cc4c27c46f22447bcda64db3345269e29ecf4_Test_Image_01.png";

    // OpenAkeneo test data — prefixed to be easily identifiable in any Akeneo instance.
    private const string OpenAkeneoRootCategoryCode = "openakeneo_root";
    private const string OpenAkeneoTestCategoryCode = "openakeneo_test_category";

    public CategoryTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async Task GetCategoryListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetCategoryListAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Categories);
    }

    [Fact]
    public async Task GetCategoryListAsync_WithPageAndLimit_ReturnsCorrectCount()
    {
        var result = await _fixture.Context.GetCategoryListAsync(page: 1, limit: 5);

        Assert.NotNull(result);
        Assert.True(result.Categories.Count <= 5);
    }

    [Fact]
    public async Task GetCategoryListFullAsync_ReturnsAllCategories()
    {
        var result = await _fixture.Context.GetCategoryListFullAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StreamCategoriesAsync_StreamsCategories()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamCategoriesAsync())
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetCategoryAsync_ReturnsCategory()
    {
        var result = await _fixture.Context.GetCategoryAsync(CategoryCode);

        Assert.NotNull(result);
        Assert.Equal(CategoryCode, result.Code);
    }

    [Fact]
    public async Task DownloadCategoryMediaFileAsync_ReturnsBytes()
    {
        var result = await _fixture.Context.DownloadCategoryMediaFileAsync(MediaFileCode);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }


    #region Write operations — lifecycle test

    [Fact]
    public async Task CreateOrUpdateCategoryAsync_Lifecycle_CreateThenUpdateThenVerify()
    {
        // Step 1 — Create the OpenAkeneo root category.
        var root = new Category
        {
            Code = OpenAkeneoRootCategoryCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Root" }
        };
        var rootResult = await _fixture.Context.CreateOrUpdateCategoryAsync(root);
        Assert.NotNull(rootResult);
        Assert.Equal(OpenAkeneoRootCategoryCode, rootResult.Code);
        Assert.Equal("(OpenAkeneo) Root", rootResult.Labels["en_US"]);

        // Step 2 — Create a child category under the root.
        var child = new Category
        {
            Code = OpenAkeneoTestCategoryCode,
            Parent = OpenAkeneoRootCategoryCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Category" }
        };
        var childResult = await _fixture.Context.CreateOrUpdateCategoryAsync(child);
        Assert.NotNull(childResult);
        Assert.Equal(OpenAkeneoTestCategoryCode, childResult.Code);
        Assert.Equal(OpenAkeneoRootCategoryCode, childResult.Parent);
        Assert.Equal("(OpenAkeneo) Test Category", childResult.Labels["en_US"]);

        // Step 3 — Update the root category label.
        var rootUpdated = new Category
        {
            Code = OpenAkeneoRootCategoryCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Root Updated" }
        };
        var rootUpdateResult = await _fixture.Context.CreateOrUpdateCategoryAsync(rootUpdated);
        Assert.NotNull(rootUpdateResult);
        Assert.Equal(OpenAkeneoRootCategoryCode, rootUpdateResult.Code);
        Assert.Equal("(OpenAkeneo) Root Updated", rootUpdateResult.Labels["en_US"]);

        // Step 4 — Update the child category label.
        var childUpdated = new Category
        {
            Code = OpenAkeneoTestCategoryCode,
            Labels = new Dictionary<string, string> { ["en_US"] = "(OpenAkeneo) Test Category Updated" }
        };
        var childUpdateResult = await _fixture.Context.CreateOrUpdateCategoryAsync(childUpdated);
        Assert.NotNull(childUpdateResult);
        Assert.Equal(OpenAkeneoTestCategoryCode, childUpdateResult.Code);
        Assert.Equal("(OpenAkeneo) Test Category Updated", childUpdateResult.Labels["en_US"]);
    }

    #endregion
}

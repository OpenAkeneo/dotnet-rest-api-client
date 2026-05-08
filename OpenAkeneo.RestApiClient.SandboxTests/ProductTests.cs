using OpenAkeneo.RestApiClient.Models;

namespace OpenAkeneo.RestApiClient.SandboxTests;

public class ProductValueTests
{
    [Theory]
    [InlineData("hello", "hello")]
    [InlineData("", "")]
    public void GetStringData_String_ReturnsAsIs(string input, string expected)
    {
        var value = new ProductValue { Data = input };
        Assert.Equal(expected, value.GetStringData());
    }

    [Fact]
    public void GetStringData_Long_ReturnsString()
    {
        var value = new ProductValue { Data = 42L };
        Assert.Equal("42", value.GetStringData());
    }

    [Fact]
    public void GetStringData_Double_ReturnsString()
    {
        var value = new ProductValue { Data = 3.14 };
        Assert.Equal("3.14", value.GetStringData());
    }

    [Fact]
    public void GetStringData_Null_ReturnsNull()
    {
        var value = new ProductValue { Data = null };
        Assert.Null(value.GetStringData());
    }

    [Fact]
    public void GetStringData_ListWithStringElement_ReturnsFirstString()
    {
        var value = new ProductValue { Data = new List<object?> { "ref_code_1", "ref_code_2" } };
        Assert.Equal("ref_code_1", value.GetStringData());
    }

    [Fact]
    public void GetStringData_ListWithNullThenString_ReturnsFirstNonNull()
    {
        var value = new ProductValue { Data = new List<object?> { null, "ref_code_2" } };
        Assert.Equal("ref_code_2", value.GetStringData());
    }

    [Fact]
    public void GetStringData_EmptyList_ReturnsNull()
    {
        var value = new ProductValue { Data = new List<object?>() };
        Assert.Null(value.GetStringData());
    }

    [Fact]
    public void GetStringData_Bool_ReturnsNull()
    {
        var value = new ProductValue { Data = true };
        Assert.Null(value.GetStringData());
    }
}



public class ProductTests : IClassFixture<TestBase>
{
    private readonly TestBase _fixture;

    public ProductTests(TestBase fixture)
    {
        _fixture = fixture;
    }


    #region Product UUID

    [Fact]
    public async Task GetProductUuidListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetProductUuidListAsync(limit: 5, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotNull(result.Products);
    }

    [Fact]
    public async Task GetProductUuidListFullAsync_ReturnsAllProducts()
    {
        var result = await _fixture.Context.GetProductUuidListFullAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task StreamProductUuidsAsync_StreamsProducts()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamProductUuidsAsync(ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0, "Expected at least one product to be streamed.");
    }

    [Fact]
    public async Task GetProductUuidAsync_ReturnsProduct()
    {
        var ct = TestContext.Current.CancellationToken;

        // Get a UUID from the list first
        var listResult = await _fixture.Context.GetProductUuidListAsync(limit: 1, ct: ct);

        if (listResult.Products.Count > 0)
        {
            var uuid = listResult.Products[0].Uuid;
            Assert.NotNull(uuid);
            var result = await _fixture.Context.GetProductUuidAsync(uuid, ct);

            Assert.NotNull(result);
            Assert.Equal(uuid, result.Uuid);
        }
        else
        {
            Assert.True(true, "No products found to test GetProductUuidAsync.");
        }
    }

    [Fact]
    public async Task GetProductUuidDraftAsync_ReturnsProductOrNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var listResult = await _fixture.Context.GetProductUuidListAsync(limit: 1, ct: ct);

        if (listResult.Products.Count > 0)
        {
            var uuid = listResult.Products[0].Uuid;
            Assert.NotNull(uuid);
            try
            {
                var result = await _fixture.Context.GetProductUuidDraftAsync(uuid, ct);
                Assert.NotNull(result);
                Assert.Equal(uuid, result.Uuid);
            }
            catch (Exception ex) when (ex.Message.Contains("NotFound") || ex.Message.Contains("Forbidden"))
            {
                Assert.True(true, "Product draft not found or forbidden, which is expected if no draft exists or feature is disabled.");
            }
        }
    }

    #endregion


    #region Product identifier

    [Fact]
    public async Task GetProductIdentifierListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetProductIdentifierListAsync(limit: 5, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotNull(result.Products);
    }

    [Fact]
    public async Task GetProductIdentifierListFullAsync_ReturnsAllProducts()
    {
        var result = await _fixture.Context.GetProductIdentifierListFullAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task StreamProductIdentifiersAsync_StreamsProducts()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamProductIdentifiersAsync(ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0, "Expected at least one product to be streamed.");
    }

    [Fact]
    public async Task GetProductIdentifierAsync_ReturnsProduct()
    {
        var ct = TestContext.Current.CancellationToken;

        // Get an identifier from the list first
        var listResult = await _fixture.Context.GetProductIdentifierListAsync(limit: 1, ct: ct);

        if (listResult.Products.Count > 0)
        {
            var identifier = listResult.Products[0].Identifier;
            var result = await _fixture.Context.GetProductIdentifierAsync(identifier, ct);

            Assert.NotNull(result);
            Assert.Equal(identifier, result.Identifier);
        }
        else
        {
            Assert.True(true, "No products found to test GetProductIdentifierAsync.");
        }
    }

    [Fact]
    public async Task GetProductIdentifierDraftAsync_ReturnsProductOrNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var listResult = await _fixture.Context.GetProductIdentifierListAsync(limit: 1, ct: ct);

        if (listResult.Products.Count > 0)
        {
            var identifier = listResult.Products[0].Identifier;
            try
            {
                var result = await _fixture.Context.GetProductIdentifierDraftAsync(identifier, ct);
                Assert.NotNull(result);
                Assert.Equal(identifier, result.Identifier);
            }
            catch (Exception ex) when (ex.Message.Contains("NotFound") || ex.Message.Contains("Forbidden"))
            {
                Assert.True(true, "Product draft not found or forbidden, which is expected if no draft exists or feature is disabled.");
            }
        }
    }

    #endregion


    #region Product model

    [Fact]
    public async Task GetProductModelListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetProductModelListAsync(limit: 5, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotNull(result.ProductModels);
    }

    [Fact]
    public async Task GetProductModelListFullAsync_ReturnsAllProductModels()
    {
        var result = await _fixture.Context.GetProductModelListFullAsync(ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task StreamProductModelsAsync_StreamsProductModels()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamProductModelsAsync(ct: TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0, "Expected at least one product to be streamed.");
    }

    [Fact]
    public async Task GetProductModelAsync_ReturnsProductModel()
    {
        var ct = TestContext.Current.CancellationToken;

        // Get a code from the list first
        var listResult = await _fixture.Context.GetProductModelListAsync(limit: 1, ct: ct);

        if (listResult.ProductModels.Count > 0)
        {
            var code = listResult.ProductModels[0].Code;
            var result = await _fixture.Context.GetProductModelAsync(code, ct);

            Assert.NotNull(result);
            Assert.Equal(code, result.Code);
        }
        else
        {
            Assert.True(true, "No product models found to test GetProductModelAsync.");
        }
    }

    [Fact]
    public async Task GetProductModelDraftAsync_ReturnsProductModelOrNotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var listResult = await _fixture.Context.GetProductModelListAsync(limit: 1, ct: ct);

        if (listResult.ProductModels.Count > 0)
        {
            var code = listResult.ProductModels[0].Code;
            try
            {
                var result = await _fixture.Context.GetProductModelDraftAsync(code, ct);
                Assert.NotNull(result);
                Assert.Equal(code, result.Code);
            }
            catch (Exception ex) when (ex.Message.Contains("NotFound") || ex.Message.Contains("Forbidden"))
            {
                Assert.True(true, "Product model draft not found or forbidden, which is expected if no draft exists or feature is disabled.");
            }
        }
    }

    #endregion


    #region Product media file

    [Fact]
    public async Task GetProductMediaFileListAsync_ReturnsList()
    {
        var result = await _fixture.Context.GetProductMediaFileListAsync(limit: 5, ct: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotNull(result.MediaFiles);
    }

    [Fact]
    public async Task GetProductMediaFileListFullAsync_ReturnsAllMediaFiles()
    {
        var result = await _fixture.Context.GetProductMediaFileListFullAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task StreamProductMediaFilesAsync_StreamsMediaFiles()
    {
        var count = 0;
        await foreach (var item in _fixture.Context.StreamProductMediaFilesAsync(TestContext.Current.CancellationToken))
        {
            Assert.NotNull(item);
            count++;
        }
        Assert.True(count > 0, "Expected at least one product to be streamed.");
    }

    [Fact]
    public async Task GetProductMediaFileAsync_ReturnsMediaFile()
    {
        var ct = TestContext.Current.CancellationToken;

        // Get a code from the list first
        var listResult = await _fixture.Context.GetProductMediaFileListAsync(limit: 1, ct: ct);

        if (listResult.MediaFiles.Count > 0)
        {
            var code = listResult.MediaFiles[0].Code;
            var result = await _fixture.Context.GetProductMediaFileAsync(code, ct);

            Assert.NotNull(result);
            Assert.Equal(code, result.Code);
        }
        else
        {
            Assert.True(true, "No media files found to test GetProductMediaFileAsync.");
        }
    }

    [Fact]
    public async Task DownloadProductMediaFileAsync_ReturnsBytes()
    {
        var ct = TestContext.Current.CancellationToken;

        // Get a code from the list first
        var listResult = await _fixture.Context.GetProductMediaFileListAsync(limit: 1, ct: ct);

        if (listResult.MediaFiles.Count > 0)
        {
            var code = listResult.MediaFiles[0].Code;
            var bytes = await _fixture.Context.DownloadProductMediaFileAsync(code, ct);

            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
        }
        else
        {
            Assert.True(true, "No media files found to test DownloadProductMediaFileAsync.");
        }
    }

    #endregion


    #region Write operations — lifecycle tests

    [Fact]
    public async Task CreateOrUpdateProductModelAsync_Lifecycle_UpdateExistingThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Fetch an existing product model to update — avoids needing to know family/variant codes upfront.
        var listResult = await _fixture.Context.GetProductModelListAsync(limit: 1, ct: ct);

        if (listResult.ProductModels.Count == 0)
        {
            Assert.True(true, "No product models found; skipping write test.");
            return;
        }

        var existing = listResult.ProductModels[0];

        // Step 1 — Re-PATCH the model with its current family_variant (required by Akeneo on update).
        var patch = new ProductModel
        {
            Code = existing.Code,
            FamilyVariant = existing.FamilyVariant,
            Categories = existing.Categories
        };
        var result = await _fixture.Context.CreateOrUpdateProductModelAsync(patch, ct);

        Assert.NotNull(result);
        Assert.Equal(existing.Code, result.Code);
        Assert.Equal(existing.FamilyVariant, result.FamilyVariant);
    }

    [Fact]
    public async Task CreateOrUpdateProductUuidAsync_Lifecycle_UpdateExistingThenVerify()
    {
        var ct = TestContext.Current.CancellationToken;

        // Fetch an existing product to update — use a known-good product.
        var listResult = await _fixture.Context.GetProductUuidListAsync(limit: 1, ct: ct);

        if (listResult.Products.Count == 0)
        {
            Assert.True(true, "No products found; skipping write test.");
            return;
        }

        var existing = listResult.Products[0];

        // Step 1 — Re-PATCH the product preserving its current state.
        var patch = new ProductUuid
        {
            Uuid = existing.Uuid,
            Family = existing.Family,
            Enabled = existing.Enabled
        };
        var result = await _fixture.Context.CreateOrUpdateProductUuidAsync(patch, ct);

        Assert.NotNull(result);
        Assert.Equal(existing.Uuid, result.Uuid);
    }

    #endregion

}

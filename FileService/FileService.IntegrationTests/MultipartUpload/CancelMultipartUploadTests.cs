using FileService.Contracts.Dto;
using FileService.Core.Features.CancelMultipartUpload;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.MultipartUpload;

public class CancelMultipartUploadTests : FileServiceFeatureBaseTests
{
    public CancelMultipartUploadTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CancelMultipartUpload_WithValidAsset_ShouldRemoveAssetFromDb()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var asset = await CreateMediaAssetInDb(ct);
        var request = new CancelMultipartUploadRequest(asset.Id, "fake-upload-id");

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Success);

        await ExecuteInDb(async dbContext =>
        {
            var exists = await dbContext.MediaAssets.AnyAsync(a => a.Id == asset.Id, ct);
            Assert.False(exists);
        });
    }

    [Fact]
    public async Task CancelMultipartUpload_WithNonExistentAsset_ShouldFail()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new CancelMultipartUploadRequest(Guid.NewGuid(), "fake-upload-id");

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<CancelMultipartUploadHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelMultipartUploadHandler>();
        return await action(handler);
    }
}

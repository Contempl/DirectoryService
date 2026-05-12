using FileService.Contracts.Dto;
using FileService.Core.Features.AbordMultipartUpload;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.MultipartUpload;

public class AbortMultipartUploadTests : FileServiceFeatureBaseTests
{
    public AbortMultipartUploadTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task AbortMultipartUpload_WithValidAsset_ShouldMarkAssetFailed()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var asset = await CreateMediaAssetInDb(ct);
        var request = new AbortMultipartUploadRequest(asset.Id, "fake-upload-id");

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var updated = await dbContext.MediaAssets.FirstAsync(a => a.Id == asset.Id, ct);
            Assert.Equal(MediaStatus.FAILED, updated.Status);
        });
    }

    [Fact]
    public async Task AbortMultipartUpload_WithNonExistentAsset_ShouldFail()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new AbortMultipartUploadRequest(Guid.NewGuid(), "fake-upload-id");

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<AbortMultipartUploadHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<AbortMultipartUploadHandler>();
        return await action(handler);
    }
}

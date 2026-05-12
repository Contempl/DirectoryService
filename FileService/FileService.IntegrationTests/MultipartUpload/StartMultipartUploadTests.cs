using FileService.Contracts;
using FileService.Core.Features;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.MultipartUpload;

public class StartMultipartUploadTests : FileServiceFeatureBaseTests
{
    public StartMultipartUploadTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task StartMultipartUpload_WithValidData_ShouldCreateAssetAndReturnUrls()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new StartMultipartUploadRequest("video.mp4", "video", "video/mp4", 1024, "lesson", Guid.NewGuid());

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.MediaAssetId);
        Assert.Equal("fake-upload-id", result.Value.UploadId);
        Assert.NotEmpty(result.Value.ChunkUrls);

        await ExecuteInDb(async dbContext =>
        {
            var asset = await dbContext.MediaAssets.FirstAsync(a => a.Id == result.Value.MediaAssetId, ct);
            Assert.NotNull(asset);
            Assert.Equal(MediaStatus.UPLOADING, asset.Status);
            Assert.Equal(AssetType.VIDEO, asset.AssetType);
        });
    }

    [Fact]
    public async Task StartMultipartUpload_WithInvalidFileName_ShouldFail()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new StartMultipartUploadRequest("no-extension", "video", "video/mp4", 1024, "lesson", Guid.NewGuid());

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task StartMultipartUpload_WithInvalidContext_ShouldFail()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new StartMultipartUploadRequest("video.mp4", "video", "video/mp4", 1024, "invalid-context", Guid.NewGuid());

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task StartMultipartUpload_WithZeroSize_ShouldFail()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new StartMultipartUploadRequest("video.mp4", "video", "video/mp4", 0, "lesson", Guid.NewGuid());

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<StartMultipartUploadHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<StartMultipartUploadHandler>();
        return await action(handler);
    }
}

using FileService.Contracts.Dto;
using FileService.Core.Features.CompleteMultipartUpload;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.MultipartUpload;

public class CompleteMultipartUploadTests : FileServiceFeatureBaseTests
{
    public CompleteMultipartUploadTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CompleteMultipartUpload_WithValidData_ShouldMarkAssetUploaded()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var asset = await CreateMediaAssetInDb(ct);
        var request = new CompleteMultipartUploadRequest(
            asset.Id,
            "fake-upload-id",
            [new PartETagDto(1, "etag-1")]);

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(asset.Id, result.Value.MediaAssetId);

        await ExecuteInDb(async dbContext =>
        {
            var updated = await dbContext.MediaAssets.FirstAsync(a => a.Id == asset.Id, ct);
            Assert.Equal(MediaStatus.UPLOADED, updated.Status);
        });
    }

    [Fact]
    public async Task CompleteMultipartUpload_WithWrongPartCount_ShouldFail()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var asset = await CreateMediaAssetInDb(ct);
        var request = new CompleteMultipartUploadRequest(
            asset.Id,
            "fake-upload-id",
            [new PartETagDto(1, "etag-1"), new PartETagDto(2, "etag-2")]);

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CompleteMultipartUpload_WithNonExistentAsset_ShouldFail()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new CompleteMultipartUploadRequest(
            Guid.NewGuid(),
            "fake-upload-id",
            [new PartETagDto(1, "etag-1")]);

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<CompleteMultipartUploadHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<CompleteMultipartUploadHandler>();
        return await action(handler);
    }
}

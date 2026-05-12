using FileService.Core.Features.Delete;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Assets;

public class DeleteFileTests : FileServiceFeatureBaseTests
{
    public DeleteFileTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteFile_WithExistingAsset_ShouldMarkAsDeleted()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var asset = await CreateMediaAssetInDb(ct);

        // Act
        var result = await ExecuteHandler(h => h.Handle(asset.Id, ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(asset.Id, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var updated = await dbContext.MediaAssets.FirstAsync(a => a.Id == asset.Id, ct);
            Assert.Equal(MediaStatus.DELETED, updated.Status);
        });
    }

    [Fact]
    public async Task DeleteFile_WithAlreadyDeletedAsset_ShouldReturnNotFound()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var asset = await CreateMediaAssetInDb(ct, MediaStatus.DELETED);

        // Act
        var result = await ExecuteHandler(h => h.Handle(asset.Id, ct));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("value.not.found", result.Error.Code);
    }

    [Fact]
    public async Task DeleteFile_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;

        // Act
        var result = await ExecuteHandler(h => h.Handle(Guid.NewGuid(), ct));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("value.not.found", result.Error.Code);
    }

    private async Task<T> ExecuteHandler<T>(Func<DeleteFileHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<DeleteFileHandler>();
        return await action(handler);
    }
}

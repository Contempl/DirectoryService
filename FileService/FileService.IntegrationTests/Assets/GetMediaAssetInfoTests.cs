using FileService.Core.Features.GetMediaAssetInfo;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Assets;

public class GetMediaAssetInfoTests : FileServiceFeatureBaseTests
{
    public GetMediaAssetInfoTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMediaAssetInfo_WithExistingAsset_ShouldReturnInfo()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var asset = await CreateMediaAssetInDb(ct);

        // Act
        var result = await ExecuteHandler(h => h.Handle(asset.Id, ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(asset.Id, result.Value.Id);
        Assert.Equal("video", result.Value.AssetType);
        Assert.Equal("uploading", result.Value.Status);
        Assert.Equal("test-video.mp4", result.Value.FileInfo.FileName);
    }

    [Fact]
    public async Task GetMediaAssetInfo_WithDeletedAsset_ShouldReturnNotFound()
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
    public async Task GetMediaAssetInfo_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;

        // Act
        var result = await ExecuteHandler(h => h.Handle(Guid.NewGuid(), ct));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("value.not.found", result.Error.Code);
    }

    private async Task<T> ExecuteHandler<T>(Func<GetMediaAssetInfoHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetMediaAssetInfoHandler>();
        return await action(handler);
    }
}

using FileService.Contracts.Dto;
using FileService.Core.Features.GetMediaAssetsInfo;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Assets;

public class GetMediaAssetsInfoTests : FileServiceFeatureBaseTests
{
    public GetMediaAssetsInfoTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMediaAssetsInfo_WithMixedStatuses_ShouldExcludeDeleted()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var visible = await CreateMediaAssetsInDb(2, ct);
        var deleted = await CreateMediaAssetInDb(ct, MediaStatus.DELETED);
        var ids = visible.Select(a => a.Id).Append(deleted.Id).ToList();
        var request = new GetMediaAssetsInfoRequest(ids);

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.MediaAssets.Count);
        Assert.DoesNotContain(result.Value.MediaAssets, a => a.Id == deleted.Id);
    }

    [Fact]
    public async Task GetMediaAssetsInfo_WithEmptyList_ShouldFail()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new GetMediaAssetsInfoRequest([]);

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetMediaAssetsInfo_WithAllNonExistentIds_ShouldReturnEmptyList()
    {
        // Arrange
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        var request = new GetMediaAssetsInfoRequest([Guid.NewGuid(), Guid.NewGuid()]);

        // Act
        var result = await ExecuteHandler(h => h.Handle(request, ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.MediaAssets);
    }

    private async Task<T> ExecuteHandler<T>(Func<GetMediaAssetsInfoHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetMediaAssetsInfoHandler>();
        return await action(handler);
    }
}

using System.Net;
using System.Net.Http.Json;
using FileService.Contracts.Dto;

namespace FileService.IntegrationTests.MediaAssets;

public class GetMediaAssetsInfoTests : FileServiceBaseTests
{
    public GetMediaAssetsInfoTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task BatchQuery_ShouldReturnVisibleAssetsAndOmitDeletedAssets()
    {
        // Arrange
        var first = await UploadOnePartAssetAsync("first.mp4", "first");
        var second = await UploadOnePartAssetAsync("second.mp4", "second");
        var deleted = await UploadOnePartAssetAsync("deleted.mp4", "deleted");
        await ReadOkEnvelopeAsync<Guid>(await Client.DeleteAsync($"/api/files/{deleted}"));

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/files/batch",
            new GetMediaAssetsInfoRequest([first, second, deleted]));
        var result = await ReadOkEnvelopeAsync<GetMediaAssetsInfoResponse>(response);

        // Assert
        Assert.Contains(result.MediaAssets, a => a.Id == first && a.Status == "uploaded");
        Assert.Contains(result.MediaAssets, a => a.Id == second && a.Status == "uploaded");
        Assert.DoesNotContain(result.MediaAssets, a => a.Id == deleted);
    }

    [Fact]
    public async Task BatchQuery_WithEmptyIds_ShouldReturnBadRequest()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/files/batch",
            new GetMediaAssetsInfoRequest([]));

        await AssertErrorStatusAsync(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BatchQuery_ShouldReturnAssetWhileUploadIsInProgress()
    {
        var uploading = await StartMultipartUploadAsync(
            "uploading.mp4",
            "video",
            "video/mp4",
            10);

        var response = await Client.PostAsJsonAsync(
            "/api/files/batch",
            new GetMediaAssetsInfoRequest([uploading.MediaAssetId]));
        var result = await ReadOkEnvelopeAsync<GetMediaAssetsInfoResponse>(response);

        var asset = Assert.Single(result.MediaAssets);
        Assert.Equal(uploading.MediaAssetId, asset.Id);
        Assert.Equal("uploading", asset.Status);
        Assert.Null(asset.DownloadUrl);
    }
}

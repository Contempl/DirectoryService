using System.Net;
using System.Net.Http.Json;
using System.Text;
using FileService.Contracts.Dto;
using FileService.Domain.Enums;

namespace FileService.IntegrationTests.MediaAssets;

public class DeleteMediaAssetTests : FileServiceBaseTests
{
    public DeleteMediaAssetTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteUploadedAsset_ShouldMarkDeletedAndRemoveStoredObject()
    {
        // Arrange
        var payload = Encoding.UTF8.GetBytes("delete me");
        var started = await StartMultipartUploadAsync("video.mp4", "video", "video/mp4", payload.Length);
        var eTag = await PutPartAsync(started.ChunkUrls.Single().UploadUrl, payload);
        await CompleteMultipartUploadAsync(started.MediaAssetId, started.UploadId, [new PartETagDto(1, eTag)]);

        // Act
        var beforeDelete = await GetAssetAsync(started.MediaAssetId, q => q.Select(a => new
        {
            Bucket = a.RawKey.Location,
            Key = a.RawKey.Value
        }));
        Assert.NotNull(beforeDelete);

        var response = await Client.DeleteAsync($"/api/files/{started.MediaAssetId}");
        var deletedId = await ReadOkEnvelopeAsync<Guid>(response);

        // Assert
        Assert.Equal(started.MediaAssetId, deletedId);
        Assert.False(await ObjectExistsAsync(beforeDelete.Bucket, beforeDelete.Key));

        var status = await GetAssetAsync(started.MediaAssetId, q => q.Select(a => a.Status));
        Assert.Equal(MediaStatus.DELETED, status);
    }

    [Fact]
    public async Task DeleteMediaAsset_WithUnknownAsset_ShouldReturnNotFound()
    {
        // Arrange & Act
        var response = await Client.DeleteAsync($"/api/files/{Guid.NewGuid()}");

        // Assert
        await AssertErrorStatusAsync(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteMediaAsset_WithAlreadyDeletedAsset_ShouldReturnNotFound()
    {
        // Arrange
        var assetId = await UploadOnePartAssetAsync("deleted-twice.mp4", "delete twice");
        await ReadOkEnvelopeAsync<Guid>(await Client.DeleteAsync($"/api/files/{assetId}"));

        // Act
        var response = await Client.DeleteAsync($"/api/files/{assetId}");

        // Assert
        await AssertErrorStatusAsync(response, HttpStatusCode.NotFound);
    }
}

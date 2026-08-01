using System.Net;
using System.Net.Http.Json;
using FileService.Contracts;
using FileService.Domain.Enums;

namespace FileService.IntegrationTests.MultipartUpload;

public class StartMultipartUploadTests : FileServiceBaseTests
{
    public StartMultipartUploadTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task StartMultipartUpload_ShouldCreateUploadingAssetAndReturnPartUrl()
    {
        // Arrange & Act
        var started = await StartMultipartUploadAsync("video.mp4", "video", "video/mp4", 1024);

        Assert.NotEqual(Guid.Empty, started.MediaAssetId);
        Assert.False(string.IsNullOrWhiteSpace(started.UploadId));
        Assert.Single(started.ChunkUrls);
        Assert.Equal(1, started.ChunkUrls[0].PartNumber);
        Assert.StartsWith("http://", started.ChunkUrls[0].UploadUrl, StringComparison.Ordinal);

        var stored = await GetAssetAsync(started.MediaAssetId, q => q.Select(a => new
        {
            a.Status,
            a.AssetType,
            Bucket = a.RawKey.Location,
            Key = a.RawKey.Value,
            a.MediaData.Size
        }));

        // Assert
        Assert.NotNull(stored);
        Assert.Equal(MediaStatus.UPLOADING, stored.Status);
        Assert.Equal(AssetType.VIDEO, stored.AssetType);
        Assert.Equal(FileServiceTestWebFactory.VideoBucket, stored.Bucket);
        Assert.Equal(1024, stored.Size);
        Assert.Contains(started.MediaAssetId.ToString(), stored.Key, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("video", "video/mp4", 1024)]
    [InlineData("video.mp4", "video/mp4", 0)]
    [InlineData("video.mp4", "image/png", 1024)]
    public async Task StartMultipartUpload_WithInvalidRequest_ShouldReturnBadRequest(
        string fileName,
        string contentType,
        long size)
    {
        // Arrange
        var request = new StartMultipartUploadRequest(fileName, "video", contentType, size, "lesson", Guid.NewGuid());

        // Act
        var response = await Client.PostAsJsonAsync("/api/files/multipart/start", request);

        // Assert
        await AssertErrorStatusAsync(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StartMultipartUpload_WithInvalidContext_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new StartMultipartUploadRequest(
            "video.mp4",
            "video",
            "video/mp4",
            1024,
            "invalid-context",
            Guid.NewGuid());

        // Act
        var response = await Client.PostAsJsonAsync("/api/files/multipart/start", request);

        // Assert
        await AssertErrorStatusAsync(response, HttpStatusCode.BadRequest);
    }
}

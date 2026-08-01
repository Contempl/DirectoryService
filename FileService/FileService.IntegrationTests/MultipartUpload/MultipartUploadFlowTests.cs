using System.Text;
using FileService.Contracts.Dto;
using FileService.Domain.Enums;

namespace FileService.IntegrationTests.MultipartUpload;

public class MultipartUploadFlowTests : FileServiceBaseTests
{
    public MultipartUploadFlowTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task MultipartUpload_ShouldStoreObjectAndMetadata()
    {
        // Arrange
        var payload = Encoding.UTF8.GetBytes("Hello from integration test");
        var started = await StartMultipartUploadAsync("video.mp4", "video", "video/mp4", payload.Length);

        // Act
        var eTag = await PutPartAsync(started.ChunkUrls.Single().UploadUrl, payload);
        var completed = await CompleteMultipartUploadAsync(started.MediaAssetId, started.UploadId, [new PartETagDto(1, eTag)]);
        
        Assert.Equal(started.MediaAssetId, completed.MediaAssetId);

        var stored = await GetAssetAsync(started.MediaAssetId, q => q.Select(a => new
        {
            a.Status,
            Bucket = a.RawKey.Location,
            Key = a.RawKey.Value,
            a.MediaData.Size
        }));
        
        // Assert
        Assert.NotNull(stored);
        Assert.Equal(MediaStatus.UPLOADED, stored.Status);
        Assert.Equal(payload.Length, stored.Size);
        Assert.True(await ObjectExistsAsync(stored.Bucket, stored.Key));
        Assert.Equal(payload.Length, await GetObjectSizeAsync(stored.Bucket, stored.Key));
    }

    [Fact]
    public async Task MultipartUpload_WithMultipleParts_ShouldComplete()
    {
        var firstPart = Enumerable.Repeat((byte)'a', 5 * 1024 * 1024).ToArray();
        var secondPart = Encoding.UTF8.GetBytes("last part");
        var started = await StartMultipartUploadAsync(
            "large-video.mp4",
            "video",
            "video/mp4",
            firstPart.Length + secondPart.Length);

        Assert.Equal(2, started.ChunkUrls.Count);

        var firstETag = await PutPartAsync(started.ChunkUrls.Single(u => u.PartNumber == 1).UploadUrl, firstPart);
        var secondETag = await PutPartAsync(started.ChunkUrls.Single(u => u.PartNumber == 2).UploadUrl, secondPart);

        await CompleteMultipartUploadAsync(
            started.MediaAssetId,
            started.UploadId,
            [new PartETagDto(1, firstETag), new PartETagDto(2, secondETag)]);

        var stored = await GetAssetAsync(started.MediaAssetId, q => q.Select(a => new
        {
            a.Status,
            Bucket = a.RawKey.Location,
            Key = a.RawKey.Value
        }));

        Assert.NotNull(stored);
        Assert.Equal(MediaStatus.UPLOADED, stored.Status);
        Assert.Equal(firstPart.Length + secondPart.Length, await GetObjectSizeAsync(stored.Bucket, stored.Key));
    }
}

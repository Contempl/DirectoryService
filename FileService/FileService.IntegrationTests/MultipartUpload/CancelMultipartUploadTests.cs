using System.Net;
using System.Net.Http.Json;
using Amazon.S3.Model;
using FileService.Contracts.Dto;

namespace FileService.IntegrationTests.MultipartUpload;

public class CancelMultipartUploadTests : FileServiceBaseTests
{
    public CancelMultipartUploadTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CancelMultipartUpload_ShouldRemovePendingAssetAndLeaveNoObject()
    {
        // Arrange
        var started = await StartMultipartUploadAsync("video.mp4", "video", "video/mp4", 1024);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/files/multipart/cancel",
            new CancelMultipartUploadRequest(started.MediaAssetId, started.UploadId));
        var cancelled = await ReadOkEnvelopeAsync<CancelMultipartUploadResponse>(response);

        // Assert
        Assert.True(cancelled.Success);
        Assert.False(await AssetExistsAsync(started.MediaAssetId));

        var uploads = await S3Client.ListMultipartUploadsAsync(new ListMultipartUploadsRequest
        {
            BucketName = FileServiceTestWebFactory.VideoBucket
        });
        Assert.DoesNotContain(uploads.MultipartUploads ?? [], u => u.UploadId == started.UploadId);
    }

    [Fact]
    public async Task CancelMultipartUpload_WithUnknownAsset_ShouldReturnNotFound()
    {
        // Arrange & Act
        var response = await Client.PostAsJsonAsync(
            "/api/files/multipart/cancel",
            new CancelMultipartUploadRequest(Guid.NewGuid(), "missing-upload-id"));

        // Assert
        await AssertErrorStatusAsync(response, HttpStatusCode.NotFound);
    }
}

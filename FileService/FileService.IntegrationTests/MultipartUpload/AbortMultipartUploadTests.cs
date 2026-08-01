using System.Net;
using System.Net.Http.Json;
using Amazon.S3.Model;
using FileService.Domain.Enums;
using AbortUploadRequest = FileService.Contracts.Dto.AbortMultipartUploadRequest;

namespace FileService.IntegrationTests.MultipartUpload;

public class AbortMultipartUploadTests : FileServiceBaseTests
{
    public AbortMultipartUploadTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task AbortMultipartUpload_ShouldMarkAssetFailedAndRemoveUpload()
    {
        // Arrange
        var started = await StartMultipartUploadAsync("video.mp4", "video", "video/mp4", 1024);

        // Act & Assert
        var response = await Client.PostAsJsonAsync(
            "/api/files/multipart/abort",
            new AbortUploadRequest(started.MediaAssetId, started.UploadId));
        var aborted = await ReadOkEnvelopeAsync<bool>(response);

        Assert.True(aborted);

        var status = await GetAssetAsync(started.MediaAssetId, q => q.Select(a => a.Status));
        Assert.Equal(MediaStatus.FAILED, status);

        var uploads = await S3Client.ListMultipartUploadsAsync(new ListMultipartUploadsRequest
        {
            BucketName = FileServiceTestWebFactory.VideoBucket
        });
        Assert.DoesNotContain(uploads.MultipartUploads ?? [], u => u.UploadId == started.UploadId);
    }

    [Fact]
    public async Task AbortMultipartUpload_WithUnknownAsset_ShouldReturnNotFound()
    {
        // Arrange & Act
        var response = await Client.PostAsJsonAsync(
            "/api/files/multipart/abort",
            new AbortUploadRequest(Guid.NewGuid(), "missing-upload-id"));

        // Assert
        await AssertErrorStatusAsync(response, HttpStatusCode.NotFound);
    }
}

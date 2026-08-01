using System.Net;
using System.Net.Http.Json;
using FileService.Contracts.Dto;
using FileService.Domain.Enums;
using CompleteUploadRequest = FileService.Contracts.Dto.CompleteMultipartUploadRequest;

namespace FileService.IntegrationTests.MultipartUpload;

public class CompleteMultipartUploadTests : FileServiceBaseTests
{
    public CompleteMultipartUploadTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CompleteMultipartUpload_WithWrongPartSet_ShouldReturnBadRequest()
    {
        // Arrange
        var started = await StartMultipartUploadAsync("large-video.mp4", "video", "video/mp4", 5 * 1024 * 1024 + 10);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/files/multipart/complete",
            new CompleteUploadRequest(started.MediaAssetId, started.UploadId, [new PartETagDto(1, "missing-etag")]));

        await AssertErrorStatusAsync(response, HttpStatusCode.BadRequest);

        // Assert
        var status = await GetAssetAsync(started.MediaAssetId, q => q.Select(a => a.Status));
        Assert.Equal(MediaStatus.UPLOADING, status);
    }

    [Fact]
    public async Task CompleteMultipartUpload_WithUnknownAsset_ShouldReturnNotFound()
    {
        // Arrange & Act
        var response = await Client.PostAsJsonAsync(
            "/api/files/multipart/complete",
            new CompleteUploadRequest(Guid.NewGuid(), "missing-upload-id", [new PartETagDto(1, "missing-etag")]));

        // Assert
        await AssertErrorStatusAsync(response, HttpStatusCode.NotFound);
    }
}

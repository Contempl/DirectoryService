using System.Net;
using System.Net.Http.Json;
using System.Text;
using FileService.Contracts.Dto;

namespace FileService.IntegrationTests.MediaAssets;

public class GetDownloadUrlTests : FileServiceBaseTests
{
    public GetDownloadUrlTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetDownloadUrl_ForUploadedAsset_ShouldReturnPresignedUrl()
    {
        // Arrange
        var payload = Encoding.UTF8.GetBytes("download me");
        var started = await StartMultipartUploadAsync("video.mp4", "video", "video/mp4", payload.Length);
        var eTag = await PutPartAsync(started.ChunkUrls.Single().UploadUrl, payload);
        await CompleteMultipartUploadAsync(started.MediaAssetId, started.UploadId, [new PartETagDto(1, eTag)]);

        var stored = await GetAssetAsync(started.MediaAssetId, q => q.Select(a => new
        {
            Bucket = a.RawKey.Location,
            Key = a.RawKey.Value
        }));
        Assert.NotNull(stored);
        
        // Act
        var urlResponse = await Client.PostAsJsonAsync("/api/files/url", new GetDownloadUrlRequest(started.MediaAssetId));
        var download = await ReadOkEnvelopeAsync<GetDownloadUrlResponse>(urlResponse);

        // Assert
        Assert.Contains(stored.Key, download.DownloadUrl, StringComparison.Ordinal);
        Assert.StartsWith("http://", download.DownloadUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDownloadUrl_WithUnknownAsset_ShouldReturnNotFound()
    {
        // Arrange & Act
        var response = await Client.PostAsJsonAsync("/api/files/url", new GetDownloadUrlRequest(Guid.NewGuid()));

        // Assert
        await AssertErrorStatusAsync(response, HttpStatusCode.NotFound);
    }
}

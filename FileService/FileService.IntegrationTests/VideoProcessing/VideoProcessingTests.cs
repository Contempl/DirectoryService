using System.Diagnostics;
using System.Text;
using Amazon.S3.Model;
using FileService.Contracts.Dto;
using FileService.Domain.Assets;
using FileService.Domain.Enums;
using FileService.Domain.MediaProcessing;
using FileService.Infrastructure.Postgres;
using FileService.VideoProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileService.IntegrationTests.VideoProcessing;

public sealed class VideoProcessingTests : FileServiceBaseTests
{
    public VideoProcessingTests(FileServiceTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task ProcessVideoAsync_WithValidVideo_ShouldCreateHlsAndMarkAssetReady()
    {
        // Arrange
        byte[] video = await CreateSampleVideoAsync();
        Guid videoAssetId = await UploadVideoAsync("sample.mp4", video);

        // Act
        await using (var scope = Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<VideoProcessingService>();
            var result = await service.ProcessVideoAsync(videoAssetId);

            Assert.True(result.IsSuccess, result.IsFailure ? result.Error.Message : string.Empty);
        }

        // Assert
        await using var assertScope = Services.CreateAsyncScope();
        var dbContext = assertScope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        var asset = await dbContext.MediaAssets
            .OfType<VideoAsset>()
            .AsNoTracking()
            .SingleAsync(video => video.Id == videoAssetId);

        var processing = await dbContext.VideoProcesses
            .AsNoTracking()
            .SingleAsync(video => video.VideoAssetId == videoAssetId);

        Assert.Equal(MediaStatus.READY, asset.Status);
        Assert.NotNull(asset.Metadata);
        Assert.True(asset.Metadata.Duration > TimeSpan.Zero);
        Assert.True(asset.Metadata.Width > 0);
        Assert.True(asset.Metadata.Height > 0);
        Assert.Equal(ProcessingStatus.COMPLETED, processing.Status);
        Assert.Equal(VideoAsset.MASTER_PLAYLIST_NAME, asset.FinalKey.Key);

        var hlsObjects = await S3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = asset.FinalKey.Location,
            Prefix = asset.GetHlsRootKey().Value.Value
        });

        Assert.Contains(hlsObjects.S3Objects, item => item.Key == asset.FinalKey.Value);
        Assert.Contains(hlsObjects.S3Objects, item => item.Key.EndsWith(".ts", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ProcessVideoAsync_WithInvalidVideo_ShouldMarkProcessingAndAssetFailed()
    {
        // Arrange
        byte[] invalidVideo = Encoding.UTF8.GetBytes("this is not a valid video file");
        Guid videoAssetId = await UploadVideoAsync("invalid.mp4", invalidVideo);

        // Act
        await using (var scope = Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<VideoProcessingService>();
            var result = await service.ProcessVideoAsync(videoAssetId);

            Assert.True(result.IsFailure);
        }

        // Assert
        await using var assertScope = Services.CreateAsyncScope();
        var dbContext = assertScope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        var asset = await dbContext.MediaAssets
            .OfType<VideoAsset>()
            .AsNoTracking()
            .SingleAsync(video => video.Id == videoAssetId);

        var processing = await dbContext.VideoProcesses
            .AsNoTracking()
            .SingleAsync(video => video.VideoAssetId == videoAssetId);

        Assert.Equal(MediaStatus.FAILED, asset.Status);
        Assert.Equal(ProcessingStatus.FAILED, processing.Status);
        Assert.False(string.IsNullOrWhiteSpace(processing.ErrorMessage));
        Assert.True(await ObjectExistsAsync(asset.RawKey.Location, asset.RawKey.Value));
    }

    private async Task<Guid> UploadVideoAsync(string fileName, byte[] payload)
    {
        var started = await StartMultipartUploadAsync(
            fileName,
            "video",
            "video/mp4",
            payload.Length);

        string eTag = await PutPartAsync(started.ChunkUrls.Single().UploadUrl, payload);
        var completed = await CompleteMultipartUploadAsync(
            started.MediaAssetId,
            started.UploadId,
            [new PartETagDto(1, eTag)]);

        return completed.MediaAssetId;
    }

    private async Task<byte[]> CreateSampleVideoAsync()
    {
        string temporaryFile = Path.Combine(Path.GetTempPath(), $"fs-video-{Guid.NewGuid():N}.mp4");

        try
        {
            using var scope = Services.CreateScope();
            var options = scope.ServiceProvider
                .GetRequiredService<IOptions<VideoProcessingOptions>>()
                .Value;

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = options.FfmpegPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("-y");
            process.StartInfo.ArgumentList.Add("-f");
            process.StartInfo.ArgumentList.Add("lavfi");
            process.StartInfo.ArgumentList.Add("-i");
            process.StartInfo.ArgumentList.Add("testsrc=size=320x240:rate=25");
            process.StartInfo.ArgumentList.Add("-f");
            process.StartInfo.ArgumentList.Add("lavfi");
            process.StartInfo.ArgumentList.Add("-i");
            process.StartInfo.ArgumentList.Add("sine=frequency=1000:sample_rate=44100");
            process.StartInfo.ArgumentList.Add("-t");
            process.StartInfo.ArgumentList.Add("1");
            process.StartInfo.ArgumentList.Add("-c:v");
            process.StartInfo.ArgumentList.Add("libx264");
            process.StartInfo.ArgumentList.Add("-pix_fmt");
            process.StartInfo.ArgumentList.Add("yuv420p");
            process.StartInfo.ArgumentList.Add("-c:a");
            process.StartInfo.ArgumentList.Add("aac");
            process.StartInfo.ArgumentList.Add("-shortest");
            process.StartInfo.ArgumentList.Add(temporaryFile);

            process.Start();
            string standardError = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            Assert.True(
                process.ExitCode == 0,
                $"Failed to create sample video with FFmpeg: {standardError}");

            return await File.ReadAllBytesAsync(temporaryFile);
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
    }
}

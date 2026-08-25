using FileService.Domain.Assets;

using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline;

public sealed record ProcessingContext
{
    private const string HLS_SUBDIRECTORY = "hls";

    public required Domain.MediaProcessing.VideoProcessing VideoProcessing { get; init; }

    public required VideoAsset VideoAsset { get; init; }

    public string? WorkingDirectory { get; private set; }

    public string? HlsOutputDirectory { get; private set; }

    public string? MediaAssetUrl { get; private set; }

    public void SetMediaAssetUrl(string mediaAssetUrl)
    {
        MediaAssetUrl = mediaAssetUrl;
    }

    public UnitResult<Error> CreateWorkingDirectory()
    {
        try
        {
            WorkingDirectory = Directory
                .CreateTempSubdirectory("video-processing")
                .FullName;

            HlsOutputDirectory = Path.Combine(WorkingDirectory, HLS_SUBDIRECTORY);
            Directory.CreateDirectory(HlsOutputDirectory);
        }
        catch (Exception exception)
        {
            return Error.Failure(
                "working.directory.creation",
                $"Failed to create working directory: {exception.Message}");
        }

        return UnitResult.Success<Error>();
    }

    public void Cleanup()
    {
        WorkingDirectory = null;
        HlsOutputDirectory = null;
        MediaAssetUrl = null;
    }
}

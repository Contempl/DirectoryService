using FileService.Domain.Assets;

namespace FileService.VideoProcessing.Pipeline;

public sealed record ProcessingContext
{
    public required Domain.MediaProcessing.VideoProcessing VideoProcessing { get; init; }

    public required VideoAsset VideoAsset { get; init; }

    public string? WorkingDirectory { get; init; }

    public string? HlsOutputDirectory { get; set; }

    public string? MediaAssetUrl { get; private set; }
}
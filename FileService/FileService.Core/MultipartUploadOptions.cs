namespace FileService.Core;

public class MultipartUploadOptions
{
    public long RecommendedChunkSizeBytes { get; init; } = 100 * 1024 * 1024;
    public int MaxChunks { get; init; } = 100;
}

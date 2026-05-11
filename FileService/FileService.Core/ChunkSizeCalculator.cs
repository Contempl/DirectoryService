using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.Kernel;

namespace FileService.Core;

public static class ChunkSizeCalculator
{
    private const int S3AbsoluteMaxChunks = 10000;

    public static Result<(long ChunkSize, int TotalChunks), Error> Calculate(
        long fileSize,
        long recommendedChunkSize,
        int maxChunks)
    {
        if (fileSize <= recommendedChunkSize)
            return (fileSize, 1);

        var chunkSize = recommendedChunkSize;
        var totalChunks = (int)((fileSize + chunkSize - 1) / chunkSize);

        if (totalChunks > maxChunks)
        {
            chunkSize = (fileSize + maxChunks - 1) / maxChunks;
            totalChunks = (int)((fileSize + chunkSize - 1) / chunkSize);
        }

        if (totalChunks is < 1 or > S3AbsoluteMaxChunks)
            return FileErrors.InvalidChunkCount(totalChunks);

        return (chunkSize, totalChunks);
    }
}

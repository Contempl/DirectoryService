using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Domain.Enums;
using FileService.Domain.MediaProcessing;
using Shared.Kernel;

namespace FileService.Core.Features.GetVideoProcessingStatus;

public sealed class VideoProcessingStatusReader(
    IMediaAssetsRepository mediaAssetsRepository,
    IVideoProcessingRepository videoProcessingRepository)
{
    public async Task<Result<VideoProcessingStatusResponse, Error>> ReadAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken)
    {
        // FS-13: Reader проверяет, что запрошен существующий video asset.
        var assetResult = await mediaAssetsRepository.GetVideoBy(
            asset => asset.Id == videoAssetId,
            cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        var asset = assetResult.Value;
        var processingResult = await videoProcessingRepository.GetByAsync(
            processing => processing.VideoAssetId == videoAssetId,
            cancellationToken);

        // FS-13: До старта Quartz строки processing ещё нет; UPLOADED означает queued.
        if (processingResult.IsFailure)
        {
            var status = MapAssetStatus(asset.Status);
            return new VideoProcessingStatusResponse(
                asset.Id,
                status,
                null,
                0,
                null,
                null,
                null,
                IsTerminal(status));
        }

        var processing = processingResult.Value;
        var responseStatus = MapProcessingStatus(asset.Status, processing.Status);

        return new VideoProcessingStatusResponse(
            asset.Id,
            responseStatus,
            processing.CurrentStep?.Type.ToString().ToLowerInvariant(),
            processing.ProgressPercentage,
            processing.ErrorMessage,
            processing.StartedAt,
            processing.CompletedAt,
            IsTerminal(responseStatus));
    }

    private static string MapAssetStatus(MediaStatus status) => status switch
    {
        MediaStatus.UPLOADED => "queued",
        MediaStatus.PROCESSING => "processing",
        MediaStatus.READY => "ready",
        MediaStatus.FAILED => "failed",
        MediaStatus.DELETED => "deleted",
        _ => status.ToString().ToLowerInvariant()
    };

    private static string MapProcessingStatus(MediaStatus assetStatus, ProcessingStatus processingStatus)
    {
        if (assetStatus is MediaStatus.READY or MediaStatus.FAILED or MediaStatus.DELETED)
            return MapAssetStatus(assetStatus);

        return processingStatus switch
        {
            ProcessingStatus.PROCESSING => "processing",
            ProcessingStatus.COMPLETED => "ready",
            ProcessingStatus.FAILED => "failed",
            _ => processingStatus.ToString().ToLowerInvariant()
        };
    }

    private static bool IsTerminal(string status) => status is "ready" or "failed" or "deleted";
}

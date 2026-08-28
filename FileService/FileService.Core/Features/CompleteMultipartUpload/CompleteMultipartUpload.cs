using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Core.Processing;
using FileService.Domain;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.CompleteMultipartUpload;

public sealed class CompleteMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/complete", async Task<EndpointResult<CompleteMultipartUploadResponse>>(
            [FromBody] CompleteMultipartUploadRequest request,
            [FromServices] CompleteMultipartUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class CompleteMultipartUploadHandler
{
    private readonly ILogger<CompleteMultipartUploadHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly ProcessingJobScheduler _processingJobScheduler;
    private readonly ITransactionManager _transactionManager;

    public CompleteMultipartUploadHandler(
        ILogger<CompleteMultipartUploadHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository,
        ProcessingJobScheduler processingJobScheduler,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
        _processingJobScheduler = processingJobScheduler;
        _transactionManager = transactionManager;
    }

    public async Task<Result<CompleteMultipartUploadResponse, Error>> Handle(
        CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var assetResult = await _mediaAssetsRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        var mediaAsset = assetResult.Value;

        if (request.PartETags.Count != mediaAsset.MediaData.ExpectedChunksCount)
            return FileErrors.ValidationFailed();

        var completeResult = await _s3Provider.CompleteMultipartUploadAsync(
            mediaAsset.RawKey, request.UploadId, request.PartETags, cancellationToken);


        if (completeResult.IsFailure)
        {
            mediaAsset.MarkFailed();
            await _transactionManager.SaveChangesAsync(cancellationToken);
            return completeResult.Error;
        }

        try
        {
            // FS-12: Сначала фиксируем UPLOADED в Postgres, чтобы StartNow-job увидела актуальный статус.
            using (var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken))
            {
                mediaAsset.MarkUploaded(DateTime.UtcNow);

                if (!mediaAsset.RequiresProcessing())
                {
                    var markReadyResult = mediaAsset.MarkReady();
                    if (markReadyResult.IsFailure)
                        return markReadyResult.Error;
                }

                var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveChangesResult.IsFailure)
                    return saveChangesResult.Error;

                transaction.Commit();
            }

            // FS-12: Долгий pipeline запускается фоново уже после commit, а HTTP-запрос быстро завершается.
            if (mediaAsset.RequiresProcessing())
            {
                var scheduleResult = await _processingJobScheduler.ScheduleAsync(mediaAsset, cancellationToken);
                if (scheduleResult.IsFailure)
                    return scheduleResult.Error;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing multipart upload for MediaAssetId: {MediaAssetId}", mediaAsset.Id);
            return GeneralErrors.Failure();
        }

        return new CompleteMultipartUploadResponse(mediaAsset.Id);
    }
}

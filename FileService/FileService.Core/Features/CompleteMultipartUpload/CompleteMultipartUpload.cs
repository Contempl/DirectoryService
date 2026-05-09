using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
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

    public CompleteMultipartUploadHandler(
        ILogger<CompleteMultipartUploadHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
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
            return completeResult.Error;

        mediaAsset.MarkUploaded(DateTime.UtcNow);
        await _mediaAssetsRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed multipart upload {UploadId} for asset {AssetId}", request.UploadId, mediaAsset.Id);

        return new CompleteMultipartUploadResponse(mediaAsset.Id);
    }
}

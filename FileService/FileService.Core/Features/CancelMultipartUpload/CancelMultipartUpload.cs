using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Domain;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.CancelMultipartUpload;

public sealed class CancelMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/cancel", async Task<EndpointResult<CancelMultipartUploadResponse>>(
            [FromBody] CancelMultipartUploadRequest request,
            [FromServices] CancelMultipartUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class CancelMultipartUploadHandler
{
    private readonly ILogger<CancelMultipartUploadHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;

    public CancelMultipartUploadHandler(
        ILogger<CancelMultipartUploadHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
    }

    public async Task<Result<CancelMultipartUploadResponse, Error>> Handle(
        CancelMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var assetResult = await _mediaAssetsRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        var mediaAsset = assetResult.Value;

        var cancelResult = await _s3Provider.AbortMultipartUploadAsync(
            mediaAsset.RawKey, request.UploadId, cancellationToken);
        if (cancelResult.IsFailure)
            return cancelResult.Error;

        await _mediaAssetsRepository.RemoveAsync(mediaAsset, cancellationToken);

        _logger.LogInformation("Cancelled multipart upload {UploadId} for asset {AssetId}", request.UploadId, mediaAsset.Id);

        return new CancelMultipartUploadResponse(true);
    }
}

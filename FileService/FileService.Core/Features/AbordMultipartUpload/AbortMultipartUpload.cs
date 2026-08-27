using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Domain;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.AbordMultipartUpload;

public sealed class AbortMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/abort", async Task<EndpointResult<bool>>(
            [FromBody] AbortMultipartUploadRequest request,
            [FromServices] AbortMultipartUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class AbortMultipartUploadHandler
{
    private readonly ILogger<AbortMultipartUploadHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;

    public AbortMultipartUploadHandler(
        ILogger<AbortMultipartUploadHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
    }

    public async Task<Result<bool, Error>> Handle(
        AbortMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var assetResult = await _mediaAssetsRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        var mediaAsset = assetResult.Value;

        var abortResult = await _s3Provider.AbortMultipartUploadAsync(
            mediaAsset.RawKey, request.UploadId, cancellationToken);
        if (abortResult.IsFailure)
            return abortResult.Error;

        mediaAsset.MarkFailed();
        await _mediaAssetsRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Aborted multipart upload {UploadId} for asset {AssetId}", request.UploadId, mediaAsset.Id);

        return true;
    }
}

using Core.Extensions;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dto;
using FileService.Domain.Assets;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using Framework.Constants;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Kernel;

namespace FileService.Core.Features;

public sealed class StartMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/start", async Task<EndpointResult<StartMultipartUploadResponse>>(
            [FromBody] StartMultipartUploadRequest request,
            [FromServices] StartMultipartUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token))
                .RequirePermissions(Permissions.FILES_MANAGE);
    }
}

public sealed class StartMultipartUploadHandler
{
    private readonly ILogger<StartMultipartUploadHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly MultipartUploadOptions _options;

    public StartMultipartUploadHandler(
        ILogger<StartMultipartUploadHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository,
        IOptions<MultipartUploadOptions> options)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
        _options = options.Value;
    }

    public async Task<Result<StartMultipartUploadResponse, Error>> Handle(
        StartMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var fileNameResult = FileName.Create(request.FileName);
        if (fileNameResult.IsFailure)
            return fileNameResult.Error;

        var contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error;

        var chunkCalcResult = ChunkSizeCalculator.Calculate(
            request.Size,
            _options.RecommendedChunkSizeBytes,
            _options.MaxChunks);
        if (chunkCalcResult.IsFailure)
            return chunkCalcResult.Error;

        var (chunkSize, totalChunks) = chunkCalcResult.Value;

        var mediaDataResult = MediaData.Create(fileNameResult.Value, contentTypeResult.Value, request.Size, totalChunks);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error;

        var mediaData = mediaDataResult.Value;
        var assetType = request.AssetType.ToAssetType();

        var ownerResult = MediaOwner.Create(request.Context, request.ContextId);
        if (ownerResult.IsFailure)
            return ownerResult.Error;

        var mediaAssetResult = MediaAsset.CreateForUpload(mediaData, assetType, ownerResult.Value);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        var mediaAsset = mediaAssetResult.Value;

        var uploadIdResult = await _s3Provider.StartMultipartUploadAsync(mediaAsset.RawKey, mediaData, cancellationToken);
        if (uploadIdResult.IsFailure)
            return uploadIdResult.Error;

        var uploadId = uploadIdResult.Value;

        var addResult = _mediaAssetsRepository.Add(mediaAsset, cancellationToken);
        if (addResult.IsFailure)
        {
            await _s3Provider.AbortMultipartUploadAsync(mediaAsset.RawKey, uploadId, cancellationToken);
            return addResult.Error;
        }

        var chunkUrlsResult = await _s3Provider.GenerateAllChunkUploadUrlsAsync(
            mediaAsset.RawKey, uploadId, totalChunks, cancellationToken);
        if (chunkUrlsResult.IsFailure)
        {
            await _s3Provider.AbortMultipartUploadAsync(mediaAsset.RawKey, uploadId, cancellationToken);
            mediaAsset.MarkFailed();
            await _mediaAssetsRepository.SaveChangesAsync(cancellationToken);
            return chunkUrlsResult.Error;
        }

        await _mediaAssetsRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Started multipart upload {UploadId} for {FileName}", uploadId, request.FileName);

        return new StartMultipartUploadResponse(mediaAsset.Id, uploadId, chunkUrlsResult.Value, chunkSize);
    }
}

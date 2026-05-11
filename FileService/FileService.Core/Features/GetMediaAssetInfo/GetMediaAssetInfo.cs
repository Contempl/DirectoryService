using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Domain.Enums;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.GetMediaAssetInfo;

public sealed class GetMediaAssetInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{mediaAssetId:guid}", async Task<EndpointResult<MediaAssetInfoResponse>>(
            [FromRoute] Guid mediaAssetId,
            [FromServices] GetMediaAssetInfoHandler handler,
            CancellationToken token) => await handler.Handle(mediaAssetId, token));
    }
}

public sealed class GetMediaAssetInfoHandler
{
    private readonly ILogger<GetMediaAssetInfoHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;

    public GetMediaAssetInfoHandler(
        ILogger<GetMediaAssetInfoHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
    }

    public async Task<Result<MediaAssetInfoResponse, Error>> Handle(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        var assetResult = await _mediaAssetsRepository.GetByIdAsync(mediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        var asset = assetResult.Value;

        if (asset.Status == MediaStatus.DELETED)
            return GeneralErrors.NotFound(mediaAssetId);

        string? downloadUrl = null;
        if (asset.Status == MediaStatus.READY)
        {
            var urlResult = await _s3Provider.DownloadFileAsync(asset.RawKey, cancellationToken);
            if (urlResult.IsSuccess)
                downloadUrl = urlResult.Value;
        }

        _logger.LogInformation("Retrieved info for asset {AssetId}", mediaAssetId);

        return new MediaAssetInfoResponse(
            asset.Id,
            asset.Status.ToString().ToLower(),
            asset.AssetType.ToString().ToLower(),
            asset.CreatedAt,
            asset.UpdatedAt,
            new FileInfoDto(
                asset.MediaData.FileName.Value,
                asset.MediaData.ContentType.Value,
                asset.MediaData.Size),
            downloadUrl);
    }
}

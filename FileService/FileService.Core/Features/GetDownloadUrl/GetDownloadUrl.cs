using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Core.Caching;
using FileService.Domain.Enums;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.GetDownloadUrl;

public sealed class GetDownloadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/url", async Task<EndpointResult<GetDownloadUrlResponse>>(
            [FromBody] GetDownloadUrlRequest request,
            [FromServices] GetDownloadUrlHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class GetDownloadUrlHandler
{
    private readonly ILogger<GetDownloadUrlHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly HybridCache _cache;

    public GetDownloadUrlHandler(
        ILogger<GetDownloadUrlHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository,
        HybridCache cache)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
        _cache = cache;
    }

    public async Task<Result<GetDownloadUrlResponse, Error>> Handle(
        GetDownloadUrlRequest request,
        CancellationToken cancellationToken)
    {
        var assetResult = await _mediaAssetsRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        var asset = assetResult.Value;
        if (asset.Status != MediaStatus.READY)
            return GeneralErrors.NotFound(request.MediaAssetId);

        var urlResult = await _cache.GetDownloadUrlAsync(
            asset.Id,
            token => _s3Provider.DownloadFileAsync(asset.RawKey, token),
            cancellationToken);
        if (urlResult.IsFailure)
            return urlResult.Error;

        _logger.LogInformation("Generated download URL for asset {AssetId}", request.MediaAssetId);

        return new GetDownloadUrlResponse(urlResult.Value);
    }
}

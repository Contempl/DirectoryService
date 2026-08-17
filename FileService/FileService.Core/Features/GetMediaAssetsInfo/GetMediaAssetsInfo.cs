using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Core.Caching;
using FileService.Domain;
using FileService.Domain.Enums;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.GetMediaAssetsInfo;

public sealed class GetMediaAssetsInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/batch", async Task<EndpointResult<GetMediaAssetsInfoResponse>>(
            [FromBody] GetMediaAssetsInfoRequest request,
            [FromServices] GetMediaAssetsInfoHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class GetMediaAssetsInfoHandler
{
    private readonly ILogger<GetMediaAssetsInfoHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly HybridCache _cache;

    public GetMediaAssetsInfoHandler(
        ILogger<GetMediaAssetsInfoHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository,
        HybridCache cache)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
        _cache = cache;
    }

    public async Task<Result<GetMediaAssetsInfoResponse, Error>> Handle(
        GetMediaAssetsInfoRequest request,
        CancellationToken cancellationToken)
    {
        if (request.MediaAssetIds.Count == 0)
            return FileErrors.ValidationFailed();

        var assets = await _mediaAssetsRepository.GetByIdsAsync(request.MediaAssetIds, cancellationToken);

        var visible = assets.Where(a => a.Status != MediaStatus.DELETED).ToList();
        var ready = visible.Where(a => a.Status == MediaStatus.READY).ToList();
        var urlTasks = ready.Select(async asset =>
        {
            var urlResult = await _cache.GetDownloadUrlAsync(
                asset.Id,
                token => _s3Provider.DownloadFileAsync(asset.RawKey, token),
                cancellationToken);

            return (asset.Id, Url: urlResult.IsSuccess ? urlResult.Value : null);
        });

        var urlMap = (await Task.WhenAll(urlTasks)).ToDictionary(result => result.Id, result => result.Url);
        var dtos = visible
            .Select(asset => new MediaAssetBriefDto(
                asset.Id,
                asset.Status.ToString().ToLowerInvariant(),
                urlMap.GetValueOrDefault(asset.Id)))
            .ToList();

        return new GetMediaAssetsInfoResponse(dtos);
    }
}

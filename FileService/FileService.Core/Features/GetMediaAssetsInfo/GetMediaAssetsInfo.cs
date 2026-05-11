using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Domain;
using FileService.Domain.Enums;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

    public GetMediaAssetsInfoHandler(
        ILogger<GetMediaAssetsInfoHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
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

        var urlMap = new Dictionary<Guid, string>();
        if (ready.Count > 0)
        {
            var urlsResult = await _s3Provider.GenerateDownloadUrlsAsync(
                ready.Select(a => a.RawKey), cancellationToken);

            if (urlsResult.IsSuccess)
            {
                for (var i = 0; i < ready.Count; i++)
                    urlMap[ready[i].Id] = urlsResult.Value[i];
            }
        }

        var dtos = visible
            .Select(a => new MediaAssetBriefDto(
                a.Id,
                a.Status.ToString().ToLower(),
                urlMap.TryGetValue(a.Id, out var url) ? url : null))
            .ToList();

        return new GetMediaAssetsInfoResponse(dtos);
    }
}

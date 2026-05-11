using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using FileService.Domain;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.Core.Features.GetChunkUploadUrl;

public sealed class GetChunkUploadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/url", async Task<EndpointResult<GetChunkUploadUrlResponse>>(
            [FromBody] GetChunkUploadUrlRequest request,
            [FromServices] GetChunkUploadUrlHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class GetChunkUploadUrlHandler
{
    private readonly ILogger<GetChunkUploadUrlHandler> _logger;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;

    public GetChunkUploadUrlHandler(
        ILogger<GetChunkUploadUrlHandler> logger,
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository)
    {
        _logger = logger;
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
    }

    public async Task<Result<GetChunkUploadUrlResponse, Error>> Handle(
        GetChunkUploadUrlRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PartNumber <= 0)
            return FileErrors.ValidationFailed();

        var assetResult = await _mediaAssetsRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        var urlResult = await _s3Provider.GenerateChunkUploadUrl(
            assetResult.Value.RawKey, request.UploadId, request.PartNumber);
        if (urlResult.IsFailure)
            return urlResult.Error;

        _logger.LogInformation("Generated chunk URL for part {PartNumber} of upload {UploadId}", request.PartNumber, request.UploadId);

        return new GetChunkUploadUrlResponse(request.PartNumber, urlResult.Value);
    }
}

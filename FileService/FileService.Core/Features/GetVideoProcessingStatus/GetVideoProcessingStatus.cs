using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Kernel;

namespace FileService.Core.Features.GetVideoProcessingStatus;

public sealed class GetVideoProcessingStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/files/{videoAssetId:guid}/processing-status",
            async Task<EndpointResult<VideoProcessingStatusResponse>>(
                [FromRoute] Guid videoAssetId,
                [FromServices] GetVideoProcessingStatusHandler handler,
                CancellationToken cancellationToken) =>
                await handler.Handle(videoAssetId, cancellationToken));
    }
}

public sealed class GetVideoProcessingStatusHandler(VideoProcessingStatusReader statusReader)
{
    public Task<Result<VideoProcessingStatusResponse, Error>> Handle(
        Guid videoAssetId,
        CancellationToken cancellationToken)
    {
        // FS-13: GET и будущий SSE используют один reader и возвращают одинаковое состояние.
        return statusReader.ReadAsync(videoAssetId, cancellationToken);
    }
}

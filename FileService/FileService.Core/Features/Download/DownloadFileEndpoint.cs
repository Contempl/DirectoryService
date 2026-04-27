using Core.Extensions;
using Framework.Constants;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.Download;

public class DownloadFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{id:guid}", async Task<EndpointResult<string>>(
            [FromRoute] Guid id,
            [FromServices] DownloadFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var request = new DownloadFileRequest(id);
            return await handler.HandleAsync(request, cancellationToken);
        }).RequirePermissions(Permissions.FILES_MANAGE);
    }
}
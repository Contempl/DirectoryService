using Core.Extensions;
using Framework.Constants;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.Upload;

public class UploadFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/upload", async Task<EndpointResult<Guid>>(
            [FromForm] UploadFileRequest request,
            [FromServices] UploadFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UploadFileCommand(request);
            return await handler.HandleAsync(command, cancellationToken);
        })
            .RequirePermissions(Permissions.FILES_MANAGE)
            .DisableAntiforgery();
    }
}
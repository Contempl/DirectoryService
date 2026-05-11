using Core.Extensions;
using Framework.Constants;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features.Delete;

public class DeleteFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/files/{id:guid}", async Task<EndpointResult<Guid>>(
            [FromRoute] Guid id,
            [FromServices] DeleteFileHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(id, cancellationToken))
                .RequirePermissions(Permissions.FILES_MANAGE);
    }
}

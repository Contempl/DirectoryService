using AuthService.Application.Extensions;
using AuthService.Domain.Constants;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Account.Deactivate;

public class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("users/{userId}/deactivate", async Task<EndpointResult> (
            [FromRoute] Guid userId,
            [FromServices] DeleteUserHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.HandleAsync(userId, cancellationToken);
        }).RequirePermissions(Permissions.USERS_MANAGE);
    }
}
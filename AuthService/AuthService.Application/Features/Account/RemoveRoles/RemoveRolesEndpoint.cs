using AuthService.Application.Extensions;
using AuthService.Domain.Constants;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Account.RemoveRoles;

public class RemoveRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/{userId}/roles/{role}", async Task<EndpointResult> (
            [FromRoute] Guid userId,
            [FromRoute] string role,
            RemoveRolesHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.HandleAsync(userId, role, cancellationToken);
        }).RequirePermissions(Permissions.USERS_MANAGE);;
    }
}
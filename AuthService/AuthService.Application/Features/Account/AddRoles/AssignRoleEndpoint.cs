using AuthService.Application.Extensions;
using AuthService.Domain.Constants;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Account.AddRoles;

public class AssignRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{userId}/roles", async Task<EndpointResult> (
            [FromRoute] Guid userId,
            [FromServices] AssignRoleHandler handler,
            AssignRoleRequest request,
            CancellationToken cancellationToken) =>
        {
            return await handler.HandleAsync(userId, request, cancellationToken);
        }).RequirePermissions(Permissions.USERS_MANAGE);
    }
}
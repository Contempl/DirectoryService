using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Account.ChangePassword;

public class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/change-password", async Task<EndpointResult> (
            [FromBody] ChangePasswordRequest request,
            [FromServices] ChangePasswordHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.HandleAsync(request, cancellationToken);
        }).RequireAuthorization();
    }
}
using AuthService.Application.Extensions;
using AuthService.Contracts.Result;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.ResetPassword;

public class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/reset-password", async Task<EndpointResult<PasswordResetCompleted>> (
            [FromBody] ResetPasswordRequest request,
            [FromServices] ResetPasswordHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.HandleAsync(request, cancellationToken);
        }).AllowAnonymousEndpoint();
    }
}
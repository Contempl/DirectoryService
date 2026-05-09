using AuthService.Application.Extensions;
using AuthService.Contracts.Result;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.SendPassResetLink;

public class SendResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forgot-password", async Task<EndpointResult<PasswordResetCompleted>>(
            [FromBody] SendResetPasswordRequest request,
            [FromServices] SendResetPasswordHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.HandleAsync(request, cancellationToken);
        }).AllowAnonymousEndpoint();
    }
}
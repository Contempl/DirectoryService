using AuthService.Application.Extensions;
using AuthService.Application.Features.Login;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async Task<EndpointResult<LoginResponse>>(
            [FromBody] RefreshTokenRequest request,
            [FromServices] RefreshTokenHandler handler,
            CancellationToken cancellationToken)
                => await handler.HandleAsync(request, cancellationToken))
            .AllowAnonymousEndpoint();
    }
}
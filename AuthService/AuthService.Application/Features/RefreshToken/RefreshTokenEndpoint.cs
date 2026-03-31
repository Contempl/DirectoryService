using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/refresh", async Task<EndpointResult<Domain.Entities.RefreshToken>>(
            [FromBody] RefreshTokenRequest request,
            [FromServices] RefreshTokenHandler handler,
            CancellationToken cancellationToken)
                => await handler.HandleAsync(request, cancellationToken));
    }
}
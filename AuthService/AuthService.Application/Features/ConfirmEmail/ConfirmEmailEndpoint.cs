using AuthService.Application.Extensions;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.ConfirmEmail;

public class ConfirmEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/confirm-email", async Task<EndpointResult>(
                [FromQuery] Guid userId,
                [FromQuery] string token,
                [FromServices] ConfirmEmailQueryHandler handler,
                CancellationToken cancellationToken) =>
        {
            var query = new ConfirmEmailQuery(userId, token);
            return await handler.HandleAsync(query, cancellationToken);
        }).AllowAnonymousEndpoint();
    }
} 
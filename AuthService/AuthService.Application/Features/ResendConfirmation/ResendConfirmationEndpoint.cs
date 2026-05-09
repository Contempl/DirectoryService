using AuthService.Application.Extensions;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.ResendConfirmation;

public class ResendConfirmationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/resend-confirmation", async Task<EndpointResult>(
            [FromBody] ResendConfirmationRequest request,
            [FromServices] ResendConfirmationHandler handler,
            CancellationToken cancellationToken) =>
        {
            return await handler.HandleAsync(request, cancellationToken);
        }).AllowAnonymousEndpoint();
    }
}
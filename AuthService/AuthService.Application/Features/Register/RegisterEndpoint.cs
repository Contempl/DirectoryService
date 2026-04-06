using AuthService.Application.Extensions;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Register;

public class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async Task<EndpointResult<Guid>>(
            [FromBody] RegisterRequest request,
            [FromServices] RegisterHandler handler,
            CancellationToken cancellationToken) 
                => await handler.HandleAsync(request, cancellationToken))
            .AllowAnonymousEndpoint();
    }
}
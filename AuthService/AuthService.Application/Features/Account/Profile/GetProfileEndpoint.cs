using AuthService.Contracts.Dto;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Account.Profile;

public class GetProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/me", async Task<EndpointResult<UserProfileDto>> (
            [FromServices] GetProfileHandler handler, 
            CancellationToken cancellationToken) =>
        {
            return await handler.HandleAsync(cancellationToken);
        }).RequireAuthorization();
    }
}
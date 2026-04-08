using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Account.RemoveRoles;

public class RemoveRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{userId}/roles", async Task<EndpointResult> () =>
        {
            throw new NotImplementedException();
        });
    }
}
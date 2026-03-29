using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/refresh", async Task<EndpointResult<Guid>>(

            CancellationToken cancellationToken) =>
        {
            throw new NotImplementedException();
        });
    }
}
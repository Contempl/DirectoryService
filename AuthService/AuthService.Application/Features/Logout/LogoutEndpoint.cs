using AuthService.Contracts.Result;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Logout;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", async Task<EndpointResult<SuccessfulResult>>(
            HttpContext httpContext, 
            [FromServices] LogoutHandler handler,
            CancellationToken cancellationToken)  =>
        {
            var claims = httpContext.User.Claims.Select(c => $"{c.Type} = {c.Value}");
            Console.WriteLine(string.Join("\n", claims)); 
            return await handler.HandleAsync(cancellationToken);
        });
    }
}
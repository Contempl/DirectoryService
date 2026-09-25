using AuthService.Application.Extensions;
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
        app.MapPost("/auth/logout", async Task<IResult> (
            HttpContext httpContext,
            [FromServices] LogoutHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!httpContext.Request.Cookies.TryGetValue(
                    "refresh_token",
                    out var rawRefreshToken) ||
                string.IsNullOrWhiteSpace(rawRefreshToken))
            {
                httpContext.Response.Cookies.Delete(
                    "refresh_token",
                    new CookieOptions { Path = "/api/auth" });

                return Results.Ok(new SuccessfulResult());
            }

            var result = await handler.HandleAsync(
                rawRefreshToken,
                cancellationToken);

            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            httpContext.Response.Cookies.Delete(
                "refresh_token",
                new CookieOptions
                {
                    Path = "/api/auth"
                });
            
            return Results.Ok(new SuccessfulResult());
            
        }).AllowAnonymousEndpoint();
    }
}

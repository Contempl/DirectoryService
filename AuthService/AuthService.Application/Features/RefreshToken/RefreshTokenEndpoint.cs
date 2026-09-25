using AuthService.Application.Extensions;
using AuthService.Application.Features.Login;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async Task<IResult> (
            HttpContext httpContext,
            [FromServices] RefreshTokenHandler handler,
            [FromServices] IHostEnvironment env,
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

                return Results.Unauthorized();
            }

            var request = new RefreshTokenRequest(rawRefreshToken);

            var result = await handler.HandleAsync(
                request,
                cancellationToken);

            if (result.IsFailure)
                return Results.Unauthorized();
            var refresh = result.Value;

            httpContext.Response.Cookies.Append(
                "refresh_token",
                refresh.RawRefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = env.IsProduction() || httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/api/auth",
                    Expires = refresh.RefreshTokenExpiresAt,
                    IsEssential = true
                });
            
            return Results.Ok(
                new LoginResponse(
                    refresh.AccessToken,
                    refresh.ExpiresIn));
            
        }).AllowAnonymousEndpoint();
    }
}

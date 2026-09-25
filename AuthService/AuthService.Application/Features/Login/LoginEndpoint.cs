using AuthService.Application.Extensions;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace AuthService.Application.Features.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async Task<IResult> (
            [FromBody]LoginRequest request,
            [FromServices] LoginHandler handler,
            HttpContext httpContext,
            [FromServices] IHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            var result =  await handler.HandleAsync(request, cancellationToken);
            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            var login = result.Value;

            httpContext.Response.Cookies.Append(
                "refresh_token",
                login.RawRefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = environment.IsProduction() || httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/api/auth",
                    Expires = login.RefreshTokenExpiresAt,
                    IsEssential = true
                });

            return Results.Ok(
                new LoginResponse(
                    login.AccessToken,
                    login.ExpiresIn));
        }).AllowAnonymousEndpoint();
    }
}

using AuthService.Application.Extensions;
using AuthService.Contracts.Result;
using Framework.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AuthService.Application.Features.Logout;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", async Task<IResult> (
            [FromServices] SignInManager<ApplicationUser> signInManager,
            CancellationToken cancellationToken) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok(new SuccessfulResult());
            
        }).AllowAnonymousEndpoint();
    }
}

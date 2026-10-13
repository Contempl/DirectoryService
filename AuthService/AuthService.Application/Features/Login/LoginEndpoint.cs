using AuthService.Application.Extensions;
using AuthService.Contracts.Result;
using AuthService.Domain.Entities;
using Framework.Response;
using Microsoft.AspNetCore.Identity;
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
            [FromServices] SignInManager<ApplicationUser> signInManager,
            CancellationToken cancellationToken) =>
        {
            var result =  await handler.HandleAsync(request, cancellationToken);
            if (result.IsFailure)
                return Results.BadRequest(result.Error);

            await signInManager.SignInAsync(
                result.Value.User,
                isPersistent: false);

            return Results.Ok(new SuccessfulResult());
        }).AllowAnonymousEndpoint();
    }
}

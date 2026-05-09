using Microsoft.AspNetCore.Builder;

namespace AuthService.Application.Extensions;

public static class EndpointExtensions
{
    public static RouteHandlerBuilder RequirePermissions(
        this RouteHandlerBuilder builder,
        params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            builder.RequireAuthorization($"Permission:{permission}");
        }

        return builder;
    }

    public static RouteHandlerBuilder AllowAnonymousEndpoint(
        this RouteHandlerBuilder builder)
    {
        return builder.AllowAnonymous();
    }
}
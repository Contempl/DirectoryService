using System.Security.Claims;
using Core.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Framework.Middleware;

public class DevAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var devAuthEnabled = configuration.GetValue<bool>("DevAuth:Enabled");
        
        if (devAuthEnabled)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new(ClaimTypes.Email, "dev@localhost"),
                new(ClaimTypes.Role, SystemRoles.Admin),
            };

            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "DevAuth"));
        }

        await next(context);
    }
}

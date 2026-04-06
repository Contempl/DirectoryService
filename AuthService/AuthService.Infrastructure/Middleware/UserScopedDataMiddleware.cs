using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Application.Auth;
using Microsoft.AspNetCore.Http;

namespace AuthService.Core.Middleware;

public class UserScopedDataMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserScopedData userScopedData)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var subClaim = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
            if (subClaim is null || !Guid.TryParse(subClaim, out var userId))
            {
                await next(context);
                return;
            }

            var email = context.User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
            var name = context.User.FindFirstValue(JwtRegisteredClaimNames.Name) ?? string.Empty;
            
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var permissions = context.User.FindAll("permission").Select(c => c.Value);

            userScopedData.Authenticate(userId, email, name, roles, permissions);
        }

        await next(context);
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Auth;
using Microsoft.AspNetCore.Http;

namespace Framework.Middleware;

public class UserScopedDataMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserScopedData userScopedData, IRolePermissionResolver permissionResolver)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var subClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
            if (subClaim is null || !Guid.TryParse(subClaim, out var userId))
            {
                await next(context);
                return;
            }

            var email = context.User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
            var name = context.User.FindFirstValue(JwtRegisteredClaimNames.Name) ?? string.Empty;
            
            var roles = context.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
            var permissions = permissionResolver.Resolve(roles);

            userScopedData.Authenticate(userId, email, name, roles, permissions);
        }

        await next(context);
    }
}
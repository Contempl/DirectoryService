using AuthService.Application.Auth;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Core.Authorization;

public class PermissionRequirementHandler(UserScopedData userScopedData) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (userScopedData.IsAuthenticated && 
            userScopedData.Permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
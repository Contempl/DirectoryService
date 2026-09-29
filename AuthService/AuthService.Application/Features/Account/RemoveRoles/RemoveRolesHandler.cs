using AuthService.Domain.Entities;
using Core.Auth;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Account.RemoveRoles;

public class RemoveRolesHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RemoveRolesHandler> _logger;

    public RemoveRolesHandler(UserManager<ApplicationUser> userManager, ILogger<RemoveRolesHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> HandleAsync(Guid userId, string role, CancellationToken cancellationToken)
    {
        var roles = SystemRoles.All;
        if (!roles.Contains(role))
        {
            _logger.LogInformation("The role {Role} was not found.", role);
            return GeneralErrors.ValueIsInvalid(nameof(role));
        }
        
        var user =  await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            _logger.LogInformation("Failed to fetch user.");
            return GeneralErrors.NotFound(name: nameof(user));
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            return Error.Conflict(
                "user.role.not_assigned",
                $"The user does not have the {role} role.");
        }
        
        if (string.Equals(role, SystemRoles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            var admins = await _userManager.GetUsersInRoleAsync(
                SystemRoles.Admin);

            if (admins.Count == 1)
            {
                _logger.LogWarning(
                    "Attempt to remove the Admin role from the last administrator.");

                return Error.Conflict(
                    "admin.last_role_removal",
                    "The Admin role cannot be removed from the last administrator.");
            }
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
        {
            _logger.LogInformation("Failed to remove role.");
            return GeneralErrors.Failure();
        }

        return UnitResult.Success<Error>();
    } 
}

using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
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
        var roles = Roles.AllRoles;
        if (!roles.Contains(role))
        {
            _logger.LogInformation($"The role {role} was not found.");
            return GeneralErrors.ValueIsInvalid(nameof(role));
        }
        
        var user =  await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            _logger.LogInformation("Failed to fetch user.");
            return GeneralErrors.NotFound(name: nameof(user));
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
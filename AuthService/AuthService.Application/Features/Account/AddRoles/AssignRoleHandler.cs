using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Account.AddRoles;

public class AssignRoleHandler
{
    private readonly IValidator<AssignRoleRequest> _validator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<AssignRoleHandler> _logger;

    public AssignRoleHandler(
        IValidator<AssignRoleRequest> validator,
        UserManager<ApplicationUser> userManager,
        ILogger<AssignRoleHandler> logger, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _validator = validator;
        _userManager = userManager;
        _logger = logger;
        _roleManager = roleManager;
    }

    public async Task<UnitResult<Error>> HandleAsync(Guid userId, AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult =  await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Validation Failed.");
            return GeneralErrors.ValueIsInvalid(request.Role);
        }
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            _logger.LogInformation("User not found");
            return GeneralErrors.NotFound(name: nameof(user));
        }

        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            _logger.LogInformation("Role not found");
            return GeneralErrors.NotFound(name: nameof(request.Role));
        }
        
        if (await _userManager.IsInRoleAsync(user, request.Role))
        {
            return Error.Conflict(
                "user.role.already_assigned",
                $"The user already has the {request.Role} role.");
        }

        var result = await _userManager.AddToRoleAsync(user, request.Role);

        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to assign role {Role} to user {UserId}: {Errors}",
                request.Role,
                userId,
                string.Join(", ", result.Errors.Select(error => error.Description)));

            return GeneralErrors.Failure();
        }

        return UnitResult.Success<Error>();
    }
}
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Shared;
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
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AssignRoleHandler> _logger;

    public AssignRoleHandler(
        IValidator<AssignRoleRequest> validator,
        UserManager<ApplicationUser> userManager,
        ILogger<AssignRoleHandler> logger, 
        RoleManager<IdentityRole> roleManager)
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
            return GeneralErrors.NotFound(name: nameof(Roles));
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        
        return UnitResult.Success<Error>();
    }
}
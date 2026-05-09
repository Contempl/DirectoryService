using AuthService.Application.Auth;
using AuthService.Contracts.Dto;
using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Account.Profile;

public class GetProfileHandler
{
    private readonly UserScopedData _userScopedData;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetProfileHandler> _logger;

    public GetProfileHandler(UserManager<ApplicationUser> userManager,
        UserScopedData userScopedData,
        ILogger<GetProfileHandler> logger)
    {
        _userManager = userManager;
        _userScopedData = userScopedData;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto, Error>> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = _userScopedData.UserId.ToString();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogInformation("User not found.");
            return GeneralErrors.NotFound(name: nameof(user));
        }

        var emailConfirmed = user.EmailConfirmed;
        
        var roles = _userScopedData.Roles;
        
        var permissions = _userScopedData.Permissions;
        
        var dateOfCreation = user.CreatedAt;

        var result = new UserProfileDto(roles, permissions, emailConfirmed, dateOfCreation);

        return result;
    }
}
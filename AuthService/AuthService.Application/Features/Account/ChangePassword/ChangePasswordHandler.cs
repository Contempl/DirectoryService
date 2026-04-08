using AuthService.Application.Auth;
using AuthService.Application.Database;
using AuthService.Domain.Entities;
using Core.Validation;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Account.ChangePassword;

public class ChangePasswordHandler
{
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<ChangePasswordRequest> _validator;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserScopedData _userScopedData;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        ITransactionManager transactionManager,
        IRefreshTokensRepository refreshTokensRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<ChangePasswordHandler> logger,
        UserScopedData userScopedData, 
        IValidator<ChangePasswordRequest> validator)
    {
        _transactionManager = transactionManager;
        _refreshTokensRepository = refreshTokensRepository;
        _userManager = userManager;
        _userScopedData = userScopedData;
        _validator = validator;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> HandleAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Validation error.");
            return GeneralErrors.ValueIsInvalid(nameof(request));
        }
        
        var userId = _userScopedData.UserId.ToString();
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogInformation("User not found.");
            return GeneralErrors.NotFound(name: nameof(ApplicationUser));
        }
        
        var passwordCheckResult = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordCheckResult)
        {
            _logger.LogInformation("Password doesn't match.");
            return GeneralErrors.ValueIsInvalid(nameof(passwordCheckResult));
        }

        var beginTransaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (beginTransaction.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction.");
            return GeneralErrors.Failure();
        }

        var transactionScope = beginTransaction.Value;

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            _logger.LogInformation("Failed to change password.");
            transactionScope.Rollback();
            return GeneralErrors.Failure();
        }
        
        await _refreshTokensRepository.RevokeAllRefreshTokensFromUser(_userScopedData.UserId, cancellationToken);

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            _logger.LogInformation("Failed to commit changes.");
            transactionScope.Rollback();
            return GeneralErrors.Failure();
        }

        return UnitResult.Success<Error>();
    }
}
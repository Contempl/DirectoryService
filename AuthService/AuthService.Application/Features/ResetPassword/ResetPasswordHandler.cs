using AuthService.Application.Database;
using AuthService.Contracts.Result;
using AuthService.Domain.Entities;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.ResetPassword;

public class ResetPasswordHandler : ICommandHandler<PasswordResetCompleted, ResetPasswordRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<ResetPasswordRequest> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<ResetPasswordRequest> validator,
        ILogger<ResetPasswordHandler> logger, 
        IRefreshTokensRepository refreshTokensRepository,
        ITransactionManager transactionManager)
    {
        _userManager = userManager;
        _validator = validator;
        _logger = logger;
        _refreshTokensRepository = refreshTokensRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<PasswordResetCompleted, Errors>> HandleAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Validation Failed.");
            return validationResult.ToErrors();
        }

        var beginTransaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (beginTransaction.IsFailure)
        {
            _logger.LogInformation("Begin Transaction failed.");
            return beginTransaction.Error.ToErrors();
        }

        using var transactionScope = beginTransaction.Value;

        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
            {
                _logger.LogInformation("User not found.");
                return GeneralErrors.NotFound(name: nameof(ApplicationUser)).ToErrors();
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                _logger.LogInformation("Reset Password failed.");
                return result.Errors.ToErrors();
            }
        
            var revokeTokensResult = await _refreshTokensRepository.RevokeAllRefreshTokensFromUser(user.Id, cancellationToken);
            if (revokeTokensResult.IsFailure)
            {
                _logger.LogInformation("Revoke Tokens failed.");
                return revokeTokensResult.Error.ToErrors();
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                _logger.LogInformation("Failed to commit transaction.");
                return commitResult.Error.ToErrors();
            }
        
            return new PasswordResetCompleted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during password reset.");
            transactionScope.Rollback();

            return GeneralErrors.Failure()
                .ToErrors();
        }
    }
}
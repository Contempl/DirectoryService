using AuthService.Application.Database;
using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Account.Deactivate;

public class DeleteUserHandler
{
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        IRefreshTokensRepository refreshTokensRepository,
        ITransactionManager transactionManager,
        UserManager<ApplicationUser> userManager,
        ILogger<DeleteUserHandler> logger)
    {
        _refreshTokensRepository = refreshTokensRepository;
        _transactionManager = transactionManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var beginTransaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (beginTransaction.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction.");
            return beginTransaction.Error;
        }

        using var transactionScope = beginTransaction.Value;

        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                _logger.LogInformation("User not found.");
                return GeneralErrors.NotFound(name: nameof(ApplicationUser));
            }
        
            var deleteResult = user.Deactivate();
            if (deleteResult.IsFailure)
            {
                _logger.LogInformation("Failed to deactivate user.");
                return deleteResult.Error;
            }
            
            var revokeResult =
                await _refreshTokensRepository.RevokeAllRefreshTokensFromUser(
                    userId,
                    cancellationToken);

            if (revokeResult.IsFailure)
                return revokeResult.Error;
            
            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
                return saveResult.Error;

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                _logger.LogInformation("Failed to commit transaction.");
                return commitResult.Error;
            }

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failure while removing user.");
            
            transactionScope.Rollback();
            
            return GeneralErrors.Failure();
        }
    }
}
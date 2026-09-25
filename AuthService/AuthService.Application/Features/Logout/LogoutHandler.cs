using AuthService.Application.Database;
using AuthService.Contracts.Result;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Logout;

public class LogoutHandler
{
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        IRefreshTokensRepository refreshTokensRepository,
        ILogger<LogoutHandler> logger,
        ITokenProvider tokenProvider,
        ITransactionManager transactionManager)
    {
        _refreshTokensRepository = refreshTokensRepository;
        _logger = logger;
        _tokenProvider = tokenProvider;
        _transactionManager = transactionManager;
    }

    public async Task<Result<SuccessfulResult, Errors>> HandleAsync(string rawRefreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenProvider.HashRefreshToken(rawRefreshToken);

        var tokenResult = await _refreshTokensRepository.GetByHashAsync(
            tokenHash,
            cancellationToken);
        
        if (tokenResult.IsFailure)
        {
            _logger.LogInformation("Refresh session is already absent.");
            return new SuccessfulResult();
        }
        
        var family = await _refreshTokensRepository.GetByFamilyIdAsync(
            tokenResult.Value.FamilyId,
            cancellationToken);

        foreach (var token in family.Where(token => !token.IsRevoked))
            token.Revoke();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        _logger.LogInformation("Tokens revoked successfully.");
        
        return new SuccessfulResult();
    }
}
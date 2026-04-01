using System.IdentityModel.Tokens.Jwt;
using AuthService.Application.Database;
using AuthService.Domain.Authorization;
using AuthService.Domain.Entities;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenHandler : ICommandHandler<Domain.Entities.RefreshToken, RefreshTokenRequest>
{
    private readonly ITransactionManager _transactionManager;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IRefreshTokensRepository refreshTokensRepository,
        ILogger<RefreshTokenHandler> logger, 
        UserManager<ApplicationUser> userManager,
        ITokenProvider tokenProvider,
        ITransactionManager transactionManager)
    {
        _refreshTokensRepository = refreshTokensRepository;
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Domain.Entities.RefreshToken, Errors>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {

        var beginTransactionAsync = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (beginTransactionAsync.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction,");
            return GeneralErrors.Failure().ToErrors();
        }

        using var transactionScope = beginTransactionAsync.Value;

        try
        {
            var fetchToken = await _refreshTokensRepository
                .GetByTokenAsync(request.RefreshToken, cancellationToken);
        
            if (fetchToken.IsFailure)
            {
                _logger.LogInformation("Could not fetch refresh token");
                return fetchToken.Error.ToErrors();
            }
        
            var refreshToken = fetchToken.Value;
        
            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user is null)
            {
                _logger.LogInformation("Could not find user with id {UserId}", refreshToken.UserId.ToString());
                return GeneralErrors.NotFound(name: nameof(ApplicationUser)).ToErrors();
            }

            if (refreshToken.IsRevoked)
            {
                _logger.LogInformation("Refresh token was revoked. Removing all refresh tokens for this user.");
                await _refreshTokensRepository.RevokeAllRefreshTokensFromUser(user.Id);
                return GeneralErrors.Failure().ToErrors();
            }
            
            if (refreshToken.ExpiryDate < DateTime.UtcNow)
                return GeneralErrors.ValueIsInvalid(nameof(RefreshToken)).ToErrors();
            
            var roles =  await _userManager.GetRolesAsync(user);
            var permissions = RolePermissions.GetPermissions(roles);
            
            var newJwtToken = _tokenProvider.GenerateJwtToken(user, roles.ToList(), permissions);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(newJwtToken);
            var jti = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

            var newRefToken = _tokenProvider.GenerateRefreshToken(user.Id, jti);
            
            if (newRefToken.IsFailure)
                return newRefToken.Error.ToErrors();

            var newRefreshToken = newRefToken.Value;
        
            refreshToken.Revoke(newRefreshToken.Token);
        
            await _refreshTokensRepository.AddAsync(newRefreshToken, cancellationToken);

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                transactionScope.Rollback();
                _logger.LogInformation("Failed to commit  transaction.");
            
                return commitResult.Error.ToErrors();
            }
        
            return newRefreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occured during transaction: {ex}", ex);
            transactionScope.Rollback();
            return GeneralErrors.Failure().ToErrors();
        }
    }
}
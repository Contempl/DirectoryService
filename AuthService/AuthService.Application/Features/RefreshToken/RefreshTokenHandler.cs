using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Abstractions;
using AuthService.Application.Database;
using AuthService.Application.Factories;
using AuthService.Application.Features.Login;
using AuthService.Domain.Authorization;
using AuthService.Domain.Entities;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Kernel;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenHandler : ICommandHandler<LoginResponse, RefreshTokenRequest>
{
    private readonly ITransactionManager _transactionManager;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IRefreshTokensRepository refreshTokensRepository,
        ILogger<RefreshTokenHandler> logger, 
        UserManager<ApplicationUser> userManager,
        ITokenProvider tokenProvider,
        ITransactionManager transactionManager, 
        IJwtOptions jwtOptions)
    {
        _refreshTokensRepository = refreshTokensRepository;
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _transactionManager = transactionManager;
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    public async Task<Result<LoginResponse, Errors>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var beginTransactionAsync = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (beginTransactionAsync.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction,");
            return GeneralErrors.Failure().ToErrors();
        }
        
        var tokenValidationParameters = TokenValidationParametersFactory.Create(_jwtOptions, validateLifetime: false);

        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(request.AccessToken, tokenValidationParameters, out _);
        
        var subClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(subClaim) || !Guid.TryParse(subClaim, out var userId))
            return GeneralErrors.ValueIsRequired(nameof(userId)).ToErrors(); 

        using var transactionScope = beginTransactionAsync.Value;

        try
        {
            var fetchToken = await _refreshTokensRepository
                .GetByTokenAsync(request.RefreshToken, userId, cancellationToken);
        
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
                await _refreshTokensRepository.RevokeAllRefreshTokensFromUser(user.Id, cancellationToken);
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

            var response = new LoginResponse(
                newJwtToken, newRefreshToken.Token, _jwtOptions.AccessTokenLifetimeMinutes);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occured during transaction: {ex}", ex);
            transactionScope.Rollback();
            return GeneralErrors.Failure().ToErrors();
        }
    }
}
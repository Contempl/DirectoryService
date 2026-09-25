using System.IdentityModel.Tokens.Jwt;
using AuthService.Application.Abstractions;
using AuthService.Application.Database;
using AuthService.Domain.Authorization;
using AuthService.Domain.Entities;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenHandler : ICommandHandler<RefreshResult, RefreshTokenRequest>
{
    private readonly ITransactionManager _transactionManager;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RefreshTokenHandler> _logger;
    private readonly IJwtOptions _jwtOptions;

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

    public async Task<Result<RefreshResult, Errors>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var transactionResult =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;
        
        var tokenHash = _tokenProvider.HashRefreshToken(
            request.RawRefreshToken);

        var tokenResult = await _refreshTokensRepository.GetByHashAsync(
            tokenHash,
            cancellationToken);

        if (tokenResult.IsFailure)
        {
            _logger.LogInformation("Refresh token was not found.");
            return GeneralErrors.Failure().ToErrors();
        }

        var refreshToken = tokenResult.Value;
       
        if (refreshToken.IsRevoked)
        {
            var family = await _refreshTokensRepository.GetByFamilyIdAsync(
                refreshToken.FamilyId,
                cancellationToken);

            foreach (var familyToken in family.Where(token => !token.IsRevoked))
                familyToken.Revoke();

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
                return saveResult.Error.ToErrors();

            var commitResult = transaction.Commit();

            if (commitResult.IsFailure)
                return commitResult.Error.ToErrors();

            _logger.LogWarning(
                "Refresh token reuse detected for family {FamilyId}.",
                refreshToken.FamilyId);

            return GeneralErrors.Failure().ToErrors();
        }
        
        if (refreshToken.ExpiryDate <= DateTime.UtcNow)
            return GeneralErrors.ValueIsInvalid(
                nameof(RefreshToken)).ToErrors();
        
        var user = await _userManager.FindByIdAsync(
            refreshToken.UserId.ToString());

        if (user is null || !user.IsActive)
        {
            _logger.LogInformation(
                "User {UserId} for refresh session was not found or inactive.",
                refreshToken.UserId);

            return GeneralErrors.NotFound().ToErrors();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = RolePermissions.GetPermissions(roles);

        var newAccessToken = _tokenProvider.GenerateJwtToken(
            user,
            roles.ToList(),
            permissions);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(newAccessToken);

        var jti = jwt.Claims
            .First(claim => claim.Type == JwtRegisteredClaimNames.Jti)
            .Value;
        
        var rotatedResult =
            _tokenProvider.GenerateRotatedRefreshToken(
                user.Id,
                jti,
                refreshToken.FamilyId);

        if (rotatedResult.IsFailure)
            return rotatedResult.Error.ToErrors();

        var rotated = rotatedResult.Value;
        
        refreshToken.Revoke(rotated.Entity.TokenHash);

        var addResult = await _refreshTokensRepository.AddAsync(
            rotated.Entity,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        var rotationSaveResult = await _transactionManager.SaveChangesAsync(
            cancellationToken);

        if (rotationSaveResult.IsFailure)
            return rotationSaveResult.Error.ToErrors();

        var rotationCommitResult = transaction.Commit();

        if (rotationCommitResult.IsFailure)
            return rotationCommitResult.Error.ToErrors();

        return new RefreshResult(
            newAccessToken,
            _jwtOptions.AccessTokenLifetimeMinutes,
            rotated.RawToken,
            rotated.Entity.ExpiryDate);
    }
}
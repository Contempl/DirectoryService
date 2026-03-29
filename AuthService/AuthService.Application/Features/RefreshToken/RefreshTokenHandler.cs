using AuthService.Domain.Entities;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenHandler : ICommandHandler<Domain.Entities.RefreshToken, RefreshTokenRequest>
{
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        IRefreshTokensRepository refreshTokensRepository,
        ILogger<RefreshTokenHandler> logger, 
        UserManager<ApplicationUser> userManager, ITokenProvider tokenProvider)
    {
        _refreshTokensRepository = refreshTokensRepository;
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    public async Task<Result<Domain.Entities.RefreshToken, Errors>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
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
        
        var newRefToken = _tokenProvider.GenerateRefreshToken(user.Id, refreshToken.Id);
        if (newRefToken.IsFailure)
            return newRefToken.Error.ToErrors();

        var newRefreshToken = newRefToken.Value;
        
        refreshToken.Revoke(newRefreshToken.Token);
        
        await _refreshTokensRepository.AddAsync(newRefreshToken, cancellationToken);

        return newRefreshToken;
    }
}
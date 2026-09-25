using AuthService.Application;
using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Core.Repositories;

public class RefreshTokensRepository : IRefreshTokensRepository
{
    private readonly AuthDbContext _authDbContext;
    private readonly ILogger<RefreshTokensRepository> _logger;

    public RefreshTokensRepository(AuthDbContext authDbContext, ILogger<RefreshTokensRepository> logger)
    {
        _authDbContext = authDbContext;
        _logger = logger;
    }

    public async Task<Result<RefreshToken, Error>> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _authDbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (refreshToken is null)
        {
            _logger.LogInformation("Refresh token not found.");
            return GeneralErrors.NotFound();
        }

        return refreshToken;
    }

    public async Task<UnitResult<Error>> RevokeAllRefreshTokensFromUser(Guid userId, CancellationToken  cancellationToken = default)
    {
        var tokens = await _authDbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var token in tokens)
            token.Revoke();
        
        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _authDbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        return await _authDbContext.RefreshTokens
            .Where(token => token.FamilyId == familyId)
            .ToListAsync(cancellationToken);
    }
}
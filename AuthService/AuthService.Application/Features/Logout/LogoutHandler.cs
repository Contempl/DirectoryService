using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Contracts.Result;
using AuthService.Domain.Entities;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Logout;

public class LogoutHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(
        IRefreshTokensRepository refreshTokensRepository,
        ILogger<LogoutHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _refreshTokensRepository = refreshTokensRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<SuccessfulResult, Errors>> HandleAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(_httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _refreshTokensRepository.RevokeAllRefreshTokensFromUser(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to log out.");
            return GeneralErrors.Failure().ToErrors();
        }

        _logger.LogInformation("Tokens revoked successfully.");
        
        return new SuccessfulResult();
    }
}
using System.IdentityModel.Tokens.Jwt;
using AuthService.Application.Abstractions;
using AuthService.Domain.Authorization;
using AuthService.Domain.Entities;
using AuthService.Domain.Shared;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Login;

public class LoginHandler : ICommandHandler<LoginResponse, LoginRequest>
{
    private readonly IJwtOptions _jwtOptions;
    private readonly ITokenProvider _tokenProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        IRefreshTokensRepository refreshTokensRepository,
        ILogger<LoginHandler> logger,
        ITokenProvider tokenProvider,
        IJwtOptions jwtOptions)
    {
        _userManager = userManager;
        _refreshTokensRepository = refreshTokensRepository;
        _logger = logger;
        _tokenProvider = tokenProvider;
        _jwtOptions = jwtOptions;
    }

    public async Task<Result<LoginResponse, Errors>> HandleAsync(LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            _logger.LogInformation($"Неверный email или пароль");
            return AuthErrors.WrongEmailOrPassword().ToErrors();
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogInformation($"User {request.Email} is locked");
            return AuthErrors.UserIsLocked().ToErrors();
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogInformation("Неверный email или пароль");
            await _userManager.AccessFailedAsync(user);
            return AuthErrors.WrongEmailOrPassword().ToErrors();
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogInformation($"User {request.Email} has not been confirmed");
            return AuthErrors.EmailIsNotConfirmed().ToErrors();
        }
        
        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = RolePermissions.GetPermissions(roles);

        var jwtToken = _tokenProvider.GenerateJwtToken(user, roles.ToList(), permissions);
        
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(jwtToken);
        var jti = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        var refreshTokenResult = _tokenProvider.GenerateRefreshToken(user.Id, jti);
        if (refreshTokenResult.IsFailure)
        {
            _logger.LogInformation("failed to generate refresh token.");
            return refreshTokenResult.Error.ToErrors();
        }

        var refreshToken = refreshTokenResult.Value;
        
        await _refreshTokensRepository.AddAsync(refreshToken, cancellationToken);
        
        var response = new LoginResponse(jwtToken, refreshToken.Token, _jwtOptions.AccessTokenLifetimeMinutes);

        return response;
    }
}
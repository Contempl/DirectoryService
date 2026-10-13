using AuthService.Domain.Entities;
using AuthService.Domain.Shared;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Login;

public class LoginHandler : ICommandHandler<LoginResult, LoginRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<LoginResult, Errors>> HandleAsync(LoginRequest request,
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

        return new LoginResult(user);
    }
}

public record LoginResult(ApplicationUser User);

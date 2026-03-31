using AuthService.Domain.Entities;
using AuthService.Domain.Shared;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.ConfirmEmail;

public class ConfirmEmailQueryHandler : IQueryHandler<ConfirmEmailQuery, UnitResult<Error>>
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ConfirmEmailQueryHandler> _logger;

    public ConfirmEmailQueryHandler(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<ConfirmEmailQueryHandler> logger)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> HandleAsync(ConfirmEmailQuery query, CancellationToken cancellationToken)
    {
        var userResult = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            _logger.LogInformation("Couldn't fetch user.");
            return userResult.Error;
        }

        var user = userResult.Value;

        var confirmEmail = await _userManager.ConfirmEmailAsync(user, query.Token);
        
        if (confirmEmail.Succeeded)
        {
            _logger.LogInformation("Email confirmed.");
            return UnitResult.Success<Error>();
        }
        
        return AuthErrors.EmailConfirmationFailed();
    }
}
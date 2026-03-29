using AuthService.Domain.Entities;
using Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.RefreshToken;

public class RefreshTokenCommand : ICommandHandler<string ,RefreshTokenRequest>
{
    private readonly ILogger<RefreshTokenCommand> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public RefreshTokenCommand(ILogger<RefreshTokenCommand> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<Result<string, Errors>> HandleAsync(RefreshTokenRequest command, CancellationToken cancellationToken)
    {
        return string.Empty;
    }
}
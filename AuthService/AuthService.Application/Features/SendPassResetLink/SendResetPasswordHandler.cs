using AuthService.Contracts.Result;
using AuthService.Domain.Entities;
using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.SendPassResetLink;

public class SendResetPasswordHandler : ICommandHandler<PasswordResetCompleted, SendResetPasswordRequest>
{
    private readonly IValidator<SendResetPasswordRequest> _resetPasswordRequestValidator;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SendResetPasswordHandler> _logger;

    public SendResetPasswordHandler(
        IEmailSender emailSender,
        UserManager<ApplicationUser> userManager,
        ILogger<SendResetPasswordHandler> logger, 
        IValidator<SendResetPasswordRequest> resetPasswordRequestValidator)
    {
        _emailSender = emailSender;
        _userManager = userManager;
        _logger = logger;
        _resetPasswordRequestValidator = resetPasswordRequestValidator;
    }

    public async Task<Result<PasswordResetCompleted, Errors>> HandleAsync(SendResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _resetPasswordRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Validation Failed.");
            return validationResult.ToErrors();
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogInformation("User not found.");
            return new PasswordResetCompleted();
        }

        var resetLink = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _emailSender.SendPasswordResetAsync(request.Email, resetLink, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogInformation("Failed to send password reset token.");
            return result.Error.ToErrors();
        }

        return new PasswordResetCompleted();
    }
}
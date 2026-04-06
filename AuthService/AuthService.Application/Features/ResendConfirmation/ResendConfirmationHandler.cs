using AuthService.Domain.Entities;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.ResendConfirmation;

public class ResendConfirmationHandler 
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<ResendConfirmationRequest> _validator;
    private readonly ILogger<ResendConfirmationHandler> _logger;

    public ResendConfirmationHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<ResendConfirmationRequest> validator,
        ILogger<ResendConfirmationHandler> logger,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _validator = validator;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> HandleAsync(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Validation Failed.");
            return GeneralErrors.ValueIsInvalid(nameof(ResendConfirmationRequest));
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.EmailConfirmed)
        {
            _logger.LogInformation("User not found.");
            return UnitResult.Success<Error>();
        }
        
        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        var subject = "Resending confirmation email";
        var htmlBody = $"<h1>Confirm your email</h1><p>Click <a href='{emailConfirmationToken}'>here</a> to confirm.</p>";

        await _emailSender.SendAsync(request.Email, subject, htmlBody, cancellationToken);
        
        _logger.LogInformation("Sent another confirmation email.");

        return UnitResult.Success<Error>();
    }
}
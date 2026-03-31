using AuthService.Application.Database;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Application.Features.Register;

public class RegisterHandler : ICommandHandler<Guid, RegisterRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITransactionManager _transactionManager;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<RegisterRequest> _validator;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<RegisterRequest> validator,
        ILogger<RegisterHandler> logger, 
        IEmailSender emailSender,
        ITransactionManager transactionManager)
    {
        _userManager = userManager;
        _validator = validator;
        _logger = logger;
        _emailSender = emailSender;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Validation failed for registration.");
            return validationResult.ToErrors();
        }
        
        var beginTransaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (beginTransaction.IsFailure)
        {
            _logger.LogInformation("Begin transaction failed.");
            return GeneralErrors.Failure().ToErrors();
        }

        using var transaction = beginTransaction.Value;

        try
        {
            var userResult = ApplicationUser.Create(request.FirstName, request.LastName, request.Email);
            if (userResult.IsFailure)
            {
                _logger.LogInformation("Failed to create a user.");
                return GeneralErrors.Failure().ToErrors();
            }

            var user = userResult.Value;

            var userCreationResult = await _userManager.CreateAsync(userResult.Value, request.Password);
            if (!userCreationResult.Succeeded)
            {
                _logger.LogInformation("User creation failed.");
                
                if (userCreationResult.Errors.Any(e => e.Code == "DuplicateEmail"))
                    return AuthErrors.EmailAlreadyTaken().ToErrors();
            
                return userCreationResult.Errors.ToErrors();
            }
        
            await _userManager.AddToRoleAsync(user, Roles.Participant);

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _emailSender.SendEmailConfirmationAsync(request.Email, emailConfirmationToken, cancellationToken);

            var commitResult = transaction.Commit();
            if (commitResult.IsFailure)
            {
                _logger.LogInformation("Failed to commit  transaction.");
            
                return commitResult.Error.ToErrors();
            }
        
            return user.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occured in a transaction to register user: {ex}", ex);
            transaction.Rollback();
            return GeneralErrors.Failure().ToErrors();
        }
    }
}
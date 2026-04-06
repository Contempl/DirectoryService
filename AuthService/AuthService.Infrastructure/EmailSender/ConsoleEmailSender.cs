using AuthService.Application;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace AuthService.Core.EmailSender;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<UnitResult<Error>> SendEmailConfirmationAsync(string email, string confirmationLink,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "EMAIL CONFIRMATION | To: {Email} | Link: {Link}",
            email, confirmationLink);

        Console.WriteLine($"EMAIL CONFIRMATION | To: {email} | Link: {confirmationLink}");
        
        return Task.FromResult(UnitResult.Success<Error>());
    }

    public Task<UnitResult<Error>> SendPasswordResetAsync(string email, string resetLink, CancellationToken ct)
    {
        _logger.LogInformation(
            "PASSWORD RESET | To: {Email} | Link: {Link}",
            email, resetLink);
        
        Console.WriteLine($"PASSWORD RESET | To: {email} | Link: {resetLink}");

        return Task.FromResult(UnitResult.Success<Error>());
    }

    public Task<UnitResult<Error>> SendAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        _logger.LogInformation(
            "EMAIL CONFIRMATION | To: {Email} | Link: {Link}",
            to, htmlBody);

        Console.WriteLine($"EMAIL CONFIRMATION | To: {to} | Link: {htmlBody}");
        
        return Task.FromResult(UnitResult.Success<Error>());
    }
}
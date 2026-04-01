using CSharpFunctionalExtensions;

namespace AuthService.Application;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken ct);
    Task SendPasswordResetAsync(string email, string resetLink, CancellationToken ct);
}
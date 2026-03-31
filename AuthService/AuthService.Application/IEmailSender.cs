using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.Application;

public interface IEmailSender
{
    Task<UnitResult<Error>> SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken ct);
    Task<UnitResult<Error>> SendPasswordResetAsync(string email, string resetLink, CancellationToken ct);
}
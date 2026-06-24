using System.Collections.Concurrent;
using AuthService.Application;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace AuthService.IntegrationTests.Infrastructure;

public class FakeEmailSender : IEmailSender
{
    private readonly ConcurrentDictionary<string, string> _confirmationTokens = new();

    public string? GetConfirmationToken(string email) =>
        _confirmationTokens.TryGetValue(email, out var token) ? token : null;

    public Task<UnitResult<Error>> SendEmailConfirmationAsync(
        string email, string confirmationLink, CancellationToken ct)
    {
        _confirmationTokens[email] = confirmationLink;
        return Task.FromResult(UnitResult.Success<Error>());
    }

    public Task<UnitResult<Error>> SendPasswordResetAsync(
        string email, string resetLink, CancellationToken ct) =>
        Task.FromResult(UnitResult.Success<Error>());

    public Task<UnitResult<Error>> SendAsync(
        string to, string subject, string htmlBody, CancellationToken ct) =>
        Task.FromResult(UnitResult.Success<Error>());
}

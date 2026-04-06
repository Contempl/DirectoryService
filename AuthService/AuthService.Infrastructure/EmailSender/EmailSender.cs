using AuthService.Application;
using AuthService.Core.Options;
using CSharpFunctionalExtensions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Shared.Kernel;

namespace AuthService.Core.EmailSender;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<SmtpEmailSender> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Email confirmation";

        _logger.LogInformation("Sending email confirmation");
        
        var htmlContent = $"<h1>Confirm your email</h1><p>Click <a href='{confirmationLink}'>here</a> to confirm.</p>";
        var bodyBuilder = new BodyBuilder { HtmlBody = htmlContent };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, SecureSocketOptions.StartTls, ct);
            _logger.LogInformation("Connecting client.");
        
            await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password, ct);
            _logger.LogInformation("Authenticating client.");
        
            await client.SendAsync(message, ct);
            _logger.LogInformation("Email sent");
        
            await client.DisconnectAsync(true, ct);
            _logger.LogInformation("Disconnecting client.");

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to sent confirmation email. Erorr: {ex}", ex);
            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> SendPasswordResetAsync(string email, string resetLink, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Reset password";

        _logger.LogInformation("Sending reset password link");
        
        var htmlContent = $"<h1>Reset your password</h1><p>Click <a href='{resetLink}'>here</a> to confirm.</p>";
        var bodyBuilder = new BodyBuilder { HtmlBody = htmlContent };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, SecureSocketOptions.StartTls, ct);
            _logger.LogInformation("Connecting client.");
        
            await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password, ct);
            _logger.LogInformation("Authenticating client.");
        
            await client.SendAsync(message, ct);
            _logger.LogInformation("Email sent");
        
            await client.DisconnectAsync(true, ct);
            _logger.LogInformation("Disconnecting client.");

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to sent confirmation email. Error: {ex}", ex);
            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> SendAsync(string email, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
        message.To.Add(MailboxAddress.Parse(email));

        _logger.LogInformation("Started sending  email.");
        
        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        
        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, SecureSocketOptions.StartTls, ct);
            _logger.LogInformation("Connecting client.");
        
            await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password, ct);
            _logger.LogInformation("Authenticating client.");
        
            await client.SendAsync(message, ct);
            _logger.LogInformation("Email sent");
        
            await client.DisconnectAsync(true, ct);
            _logger.LogInformation("Disconnecting client.");

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to sent confirmation email. Error: {ex}", ex);
            return GeneralErrors.Failure();
        }
    }
}
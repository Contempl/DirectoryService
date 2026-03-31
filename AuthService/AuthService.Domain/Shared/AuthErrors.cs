using Shared.Kernel;

namespace AuthService.Domain.Shared;

public static class AuthErrors
{
    public static Error EmailAlreadyTaken() => 
        Error.Conflict("auth.email_taken", "Email is already taken.");
    
    public static Error EmailConfirmationFailed() =>
        Error.Conflict("auth.email_confirmation_failed", "Email confirmation failed.");
}
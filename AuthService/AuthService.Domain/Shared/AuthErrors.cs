using Shared.Kernel;

namespace AuthService.Domain.Shared;

public static class AuthErrors
{
    public static Error EmailAlreadyTaken() => 
        Error.Conflict("auth.email_taken", "Email is already taken.");
    
    public static Error EmailConfirmationFailed() =>
        Error.Conflict("auth.email_confirmation_failed", "Email confirmation failed.");
    
    public static Error UserIsLocked() =>
        Error.Conflict("user.is.locked", "User is locked.");
    
    public static Error WrongEmailOrPassword() =>
        Error.Conflict("wrong.password.or.email", "Wrongs email or password.");
    
    public static Error EmailIsNotConfirmed() =>
        Error.Conflict("email.is.not.confirmed", "Email is not confirmed.");
}
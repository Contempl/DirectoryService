using Shared.Kernel;

namespace AuthService.Domain.Shared;

public static class AuthErrors
{
    public static Error EmailAlreadyTaken() => 
        Error.Conflict("auth.email_taken", "Email is already taken");
}
using Microsoft.AspNetCore.Identity;
using Shared.Kernel;

namespace AuthService.Domain.Shared;

public static class IdentityResultExtensions
{
    public static Error ToError(this IdentityError error)
    {
        return Error.Validation(error.Code, error.Description);
    }
    
    public static Errors ToErrors(this IEnumerable<IdentityError> errors)
    {
        return new Errors(errors.Select(e => e.ToError()));
    }
}
using Core.Validation;
using FluentValidation;
using Shared.Kernel;

namespace AuthService.Application.Features.Register;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithError(Error.Validation("auth.email.invalid", "Email is invalid"));

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .WithError(Error.Validation("auth.password.too_short", "Password must be at least 8 characters"))
            .Must(p => p.Any(char.IsUpper))
            .WithError(Error.Validation("auth.password.no_upper", "Password must contain at least one uppercase letter"))
            .Must(p => p.Any(char.IsDigit))
            .WithError(Error.Validation("auth.password.no_digit", "Password must contain at least one digit"))
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c)))
            .WithError(Error.Validation("auth.password.no_special", "Password must contain at least one special character"));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithError(Error.Validation("auth.firstname.required", "First name is required"))
            .MinimumLength(2)
            .WithError(Error.Validation("auth.firstname.too_short", "First name must be at least 2 characters"))
            .MaximumLength(100)
            .WithError(Error.Validation("auth.firstname.too_long", "First name must be at most 100 characters"));

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithError(Error.Validation("auth.lastname.required", "Last name is required"))
            .MinimumLength(2)
            .WithError(Error.Validation("auth.lastname.too_short", "Last name must be at least 2 characters"))
            .MaximumLength(100)
            .WithError(Error.Validation("auth.lastname.too_long", "Last name must be at most 100 characters"));
    }
}
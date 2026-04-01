using FluentValidation;

namespace AuthService.Application.Features.Register;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .Must(p => p.Any(char.IsUpper))
                .WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Must(p => p.Any(char.IsDigit))
                .WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c)))
                .WithMessage("Пароль должен содержать хотя бы один специальный символ");

        RuleFor(x => x.FirstName)
            .NotNull()
            .MinimumLength(2)
            .MaximumLength(100);
        
        RuleFor(x => x.LastName)
            .NotNull()
            .MinimumLength(2)
            .MaximumLength(100);
    }
}
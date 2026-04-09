using FluentValidation;

namespace AuthService.Application.Features.Account.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .MinimumLength(8)
            .Must(p => p.Any(char.IsUpper))
                .WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Must(p => p.Any(char.IsDigit))
                .WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c)))
                .WithMessage("Пароль должен содержать хотя бы один специальный символ");
        
        RuleFor(x => x.NewPassword)
            .MinimumLength(8)
            .Must(p => p.Any(char.IsUpper))
                .WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Must(p => p.Any(char.IsDigit))
                .WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c)))
                .WithMessage("Пароль должен содержать хотя бы один специальный символ");
    }
}
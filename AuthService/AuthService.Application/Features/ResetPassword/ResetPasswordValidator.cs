using FluentValidation;

namespace AuthService.Application.Features.ResetPassword;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();

        RuleFor(request => request.Token)
            .NotEmpty();

        RuleFor(request => request.NewPassword)
            .MinimumLength(8)
            .Must(p => p.Any(char.IsUpper))
                .WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Must(p => p.Any(char.IsDigit))
                .WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c)))
                .WithMessage("Пароль должен содержать хотя бы один специальный символ");
    }
}
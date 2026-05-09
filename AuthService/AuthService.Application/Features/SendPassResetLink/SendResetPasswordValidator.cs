using FluentValidation;

namespace AuthService.Application.Features.SendPassResetLink;

public class SendResetPasswordValidator : AbstractValidator<SendResetPasswordRequest>
{
    public SendResetPasswordValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
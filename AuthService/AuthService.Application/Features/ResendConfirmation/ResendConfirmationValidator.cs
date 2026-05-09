using FluentValidation;

namespace AuthService.Application.Features.ResendConfirmation;

public class ResendConfirmationValidator : AbstractValidator<ResendConfirmationRequest>
{
    public ResendConfirmationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("Email is invalid.");
    }
}
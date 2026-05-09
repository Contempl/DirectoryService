using FluentValidation;

namespace AuthService.Application.Features.Account.AddRoles;

public class AssignRoleValidator : AbstractValidator<AssignRoleRequest>
{
    public AssignRoleValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
                .WithMessage("Role is required")
            .NotNull()
            .MaximumLength(30);
    }
}
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.ToggleActivity;

public class ToggleDepartmentActivityValidator : AbstractValidator<ToggleDepartmentActivityRequest>
{
    public ToggleDepartmentActivityValidator()
    {
        RuleFor(request => request.DepartmentId)
            .NotEmpty();
    }
}
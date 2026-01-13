using FluentValidation;

namespace DirectoryService.Application.Departments.Update;

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("Name cannot be empty");
    }
}
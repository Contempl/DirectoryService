using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.Create;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(x => x.LocationIds)
            .NotEmpty()
            .WithError(Error.Validation("department.locationIds.not.empty", "Список локаций не может быть пустым"))
            .Must(items => items.Distinct().Count() == items.Count)
            .WithError(Error.Validation("location.ids.must.be.distinct", "Локации не должны повторяться"));
    }
}
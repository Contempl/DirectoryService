using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions.Create;

public class CreatePositionValidator : AbstractValidator<CreatePositionRequest>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);
        
        RuleFor(x => x.Description)
            .MaximumLength(1000);
        
        RuleFor(x => x.DepartmentIds)
            .NotEmpty()
            .Must(items => items.Distinct().Count() == items.Count)
            .WithError(Error.Validation("duplicate.department.id", "дубликаты департаментов не должны попадать в коллекцию"));
    }
}
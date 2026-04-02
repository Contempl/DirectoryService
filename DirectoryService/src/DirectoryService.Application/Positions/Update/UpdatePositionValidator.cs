using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions.Update;

public class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(c => c.PositionId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdatePositionCommand.PositionId)));

        RuleFor(c => c.Request.Name)
            .MustBeValueObject(Name.Create);
    }
}
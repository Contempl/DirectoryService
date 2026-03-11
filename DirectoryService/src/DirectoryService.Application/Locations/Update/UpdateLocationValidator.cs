using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities.VO;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.Update;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationValidator()
    {
        RuleFor(x => x.LocationDto.Name)
            .NotEmpty()
            .MaximumLength(150)
            .WithError(GeneralErrors.ValueIsInvalid("Name must be not empty and between 150 characters"));

        RuleFor(x => x.LocationDto.City)
            .MaximumLength(128)
            .MinimumLength(2);

        RuleFor(x => x.LocationDto.Street)
            .MaximumLength(128)
            .MinimumLength(2);

        RuleFor(x => x.LocationDto.House)
            .MaximumLength(20);

        RuleFor(x => x.LocationDto.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}
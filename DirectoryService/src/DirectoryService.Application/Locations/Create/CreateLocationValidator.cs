using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities.VO;
using FluentValidation;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.CreateLocationDto.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(c => c.CreateLocationDto)
            .MustBeValueObject(
                a
                    => Address.Create(a.City, a.Street, a.House, a.Apartment));
        
        RuleFor(x => x.CreateLocationDto.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}
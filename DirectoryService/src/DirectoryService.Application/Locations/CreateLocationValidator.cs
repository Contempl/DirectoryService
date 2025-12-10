using System.Data;
using DirectoryService.Contracts.Locations;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("Name must be between 3 and 150 characters");
        
        RuleFor(x => x.Address.City)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("City must be between 3 and 150 characters");
        
        RuleFor(x => x.Address.Street)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("Street must be between 3 and 150 characters");
        
        RuleFor(x => x.Address.House)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("House must less than 50 characters");
        
        RuleFor(x => x.Address.Apartment)
            .MaximumLength(150)
            .WithMessage("Apartment must be between 3 and 150 characters");

        RuleFor(x => x.Timezone.Value)
            .NotEmpty()
            .WithMessage("Time zone must be valid IANA code");
    }
}
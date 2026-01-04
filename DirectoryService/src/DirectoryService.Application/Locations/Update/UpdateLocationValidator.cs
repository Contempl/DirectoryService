using DirectoryService.Application.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.Update;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationValidator()
    {
        RuleFor(x => x.LocationIds.ToList())
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("locationIds"))
            .Must(locationIds => locationIds != null 
                                 && locationIds.Count() == locationIds.Distinct().Count())
            .WithError(GeneralErrors.ValueIsRequired("locationIds"));
    }
}
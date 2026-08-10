using DirectoryService.Application.Validation;
using Shared.Kernel;
using FluentValidation;

namespace DirectoryService.Application.Locations.UpdateForDepartment;

public class UpdateLocationsValidator : AbstractValidator<UpdateLocationsRequest>
{
    public UpdateLocationsValidator()
    {
        RuleFor(x => x.LocationIds.ToList())
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("locationIds"))
            .Must(locationIds => locationIds != null 
                                 && locationIds.Count() == locationIds.Distinct().Count())
            .WithError(GeneralErrors.ValueIsRequired("locationIds"));
    }
}

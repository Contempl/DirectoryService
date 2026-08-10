using DirectoryService.Application.Validation;
using Shared.Kernel;
using FluentValidation;

namespace DirectoryService.Application.Locations.BulkActivityUpdate;

public class BulkUpdateLocationsActivityValidator : AbstractValidator<BulkUpdateLocationsActivityRequest>
{
    public BulkUpdateLocationsActivityValidator()
    {
        RuleFor(x => x.LocationIds)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithError(Error.Validation(
                "locations.bulk.ids.required",
                "Location ids are required."))
            .NotEmpty()
            .WithError(Error.Validation(
                "locations.bulk.ids.empty",
                "At least one location id is required."))
            .Must(ids => ids.Count <= 100)
            .WithError(Error.Validation(
                "locations.bulk.ids.limit",
                "No more than 100 locations can be processed at once."));

        RuleForEach(x => x.LocationIds)
            .NotEmpty()
            .WithError(Error.Validation(
                "locations.bulk.id.empty",
                "Location id cannot be empty."));
    }
}

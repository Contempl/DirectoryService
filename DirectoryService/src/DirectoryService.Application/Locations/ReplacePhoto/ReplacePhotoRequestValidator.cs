using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using Shared.Kernel;
using FluentValidation;

namespace DirectoryService.Application.Locations.ReplacePhoto;

public sealed class ReplacePhotoRequestValidator : AbstractValidator<ReplacePhotoRequest>
{
    public ReplacePhotoRequestValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithError(Error.Validation(
                "location.id.required",
                "Location id is required."));

        RuleFor(x => x.AssetId)
            .NotEmpty()
            .WithError(LocationPhotoErrors.AssetIdRequired());
    }
}

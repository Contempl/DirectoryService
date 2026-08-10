using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.AttachPhoto;

public sealed class AttachPhotoRequestValidator : AbstractValidator<AttachPhotoRequest>
{
    public AttachPhotoRequestValidator()
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

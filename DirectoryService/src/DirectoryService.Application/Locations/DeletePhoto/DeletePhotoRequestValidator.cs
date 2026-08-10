using DirectoryService.Application.Validation;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.DeletePhoto;

public sealed class DeletePhotoRequestValidator : AbstractValidator<DeletePhotoRequest>
{
    public DeletePhotoRequestValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithError(Error.Validation(
                "location.id.required",
                "Location id is required."));
    }
}

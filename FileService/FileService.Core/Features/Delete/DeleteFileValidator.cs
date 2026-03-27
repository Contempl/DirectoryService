using Core.Validation;
using FluentValidation;
using Shared.Kernel;

namespace FileService.Core.Features.Delete;

public class DeleteFileValidator : AbstractValidator<DeleteFileRequest>
{
    public DeleteFileValidator()
    {
        RuleFor(f => f.FileId)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(DeleteFileRequest.FileId)))
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired(nameof(DeleteFileRequest.FileId)));
    }
}
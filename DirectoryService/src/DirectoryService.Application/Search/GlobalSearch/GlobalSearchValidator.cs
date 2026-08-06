using FluentValidation;

namespace DirectoryService.Application.Search.GlobalSearch;

public class GlobalSearchValidator : AbstractValidator<SearchQuery>
{
    public GlobalSearchValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
    }
}
using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Search.GlobalSearch;

public sealed record SearchQuery(string Query) : IQuery;

public sealed record SearchResultDto(
    string Type,
    Guid Id,
    string Title,
    string Subtitle,
    string? Href);
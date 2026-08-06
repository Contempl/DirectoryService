using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Search.GlobalSearch;
using DirectoryService.Domain.Shared;
using DirectoryService.Presentation.Response;
using Framework.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[Route("api/search")]
[Authorize(Policy = $"Permission:{Permissions.CONTENT_VIEW}")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly IQueryHandler<SearchQuery, Result<List<SearchResultDto>, Errors>> _searchHandler;

    public SearchController(IQueryHandler<SearchQuery, Result<List<SearchResultDto>, Errors>> searchHandler)
    {
        _searchHandler = searchHandler;
    }

    [HttpGet]
    public async Task<EndpointResult<List<SearchResultDto>>> Search(
        [FromQuery(Name = "q")] string query,
        CancellationToken cancellationToken)
    {
        return await _searchHandler.HandleAsync(
            new SearchQuery(query),
            cancellationToken);
    }
}

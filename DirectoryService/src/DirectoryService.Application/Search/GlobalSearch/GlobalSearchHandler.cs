using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using Shared.Kernel;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Search.GlobalSearch;

public class GlobalSearchHandler : IQueryHandler<SearchQuery, Result<List<SearchResultDto>, Errors>>
{
    private readonly IValidator<SearchQuery> _validator;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GlobalSearchHandler> _logger;

    public GlobalSearchHandler(IValidator<SearchQuery> validator,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GlobalSearchHandler> logger)
    {
        _validator = validator;
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<List<SearchResultDto>, Errors>> HandleAsync(SearchQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Global search validation failed for query {Query}", query.Query);
            return validationResult.ToErrors();
        }

        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
                           (
                               SELECT
                                   'department' AS Type,
                                   d.id AS Id,
                                   d.name AS Title,
                                   d.path::text AS Subtitle,
                                   '/departments?focus=' || d.id::text AS Href
                               FROM departments d
                               WHERE d.deleted_at IS NULL
                                 AND d.name ILIKE '%' || @Query || '%'
                               LIMIT @Limit
                           )

                           UNION ALL

                           (
                               SELECT
                                   'location' AS Type,
                                   l.id AS Id,
                                   l.name AS Title,
                                   concat_ws(', ', l.city, l.street, l.house) AS Subtitle,
                                   '/locations?focus=' || l.id::text AS Href
                               FROM locations l
                               WHERE l.is_active = TRUE
                                 AND (
                                     l.name ILIKE '%' || @Query || '%'
                                     OR l.city ILIKE '%' || @Query || '%'
                                 )
                               LIMIT @Limit
                           )

                           UNION ALL

                           (
                               SELECT
                                   'position' AS Type,
                                   p.id AS Id,
                                   p.name AS Title,
                                   COALESCE(string_agg(DISTINCT d.name, ', '), '') AS Subtitle,
                                   '/positions?focus=' || p.id::text AS Href
                               FROM positions p
                               LEFT JOIN department_positions dp
                                   ON dp.position_id = p.id
                               LEFT JOIN departments d
                                   ON d.id = dp.department_id AND d.deleted_at IS NULL
                               WHERE p.is_active = TRUE
                                 AND p.name ILIKE '%' || @Query || '%'
                               GROUP BY p.id, p.name
                               LIMIT @Limit
                           );
                           """;

        var command = new CommandDefinition(
            sql,
            new
            {
                Query = query.Query.Trim(),
                Limit = 10
            },
            cancellationToken: cancellationToken);

        var results =
            await connection.QueryAsync<SearchResultDto>(command);

        return results.ToList();
    }
}

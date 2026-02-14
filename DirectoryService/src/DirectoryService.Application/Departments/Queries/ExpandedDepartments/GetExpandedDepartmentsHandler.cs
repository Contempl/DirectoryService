using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.Queries.ExpandedDepartments;

public class GetExpandedDepartmentsHandler : IQueryHandler<ExtendedDepartmentsQuery, List<DepartmentsWithChildrenDto>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly HybridCache _cache;

    public GetExpandedDepartmentsHandler(IDbConnectionFactory dbConnectionFactory, HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cache = cache;
    }

    public async Task<List<DepartmentsWithChildrenDto>> HandleAsync(ExtendedDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var cacheKey = string.Concat(
            $"{Constants.DEPARTMENT_CACHE_KEY}",
            $"{Constants.EXPANDED_DEPARTMENTS_TAG}",
            "page", query.Page,
            "size", query.Size);

        var parameters = new DynamicParameters(new
        {
            Offset = (query.Page - 1) * query.Size, query.Size, query.Prefetch
        });

        var departmentsWithChildren = await _cache.GetOrCreateAsync<List<DepartmentsWithChildrenDto>>(
            cacheKey,
            factory: async _ =>
            {
                var sqlQuery = """
                               WITH roots AS (SELECT 
                               d.id, 
                               d.name, 
                               d.identifier,
                               d."ParentId", 
                               d.path, 
                               d.depth, 
                               d.is_active AS IsActive,
                               d.created_at,
                               d.updated_at
                               FROM departments d  
                               WHERE d."ParentId" IS NULL
                               ORDER BY d.created_at
                               LIMIT @Size OFFSET @Offset)
                               SELECT *, (EXISTS(SELECT 1 FROM departments d where d."ParentId" = roots.id OFFSET @Prefetch LIMIT 1)) AS HasMoreChildren FROM roots
                               UNION ALL 
                               SELECT c.*, (EXISTS(SELECT 1 FROM departments d WHERE d."ParentId" = c.id)) AS HasMoreChildren FROM roots r
                               CROSS JOIN LATERAL(
                               SELECT 
                               d.id, 
                               d.name, 
                               d.identifier,
                               d."ParentId", 
                               d.path, 
                               d.depth, 
                               d.is_active AS IsActive,
                               d.created_at,
                               d.updated_at
                               FROM departments d
                               WHERE d."ParentId" = r.id AND d.is_active = true
                               ORDER BY d.created_at
                               LIMIT @Prefetch) c
                               """;

                using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

                var departments = await connection
                    .QueryAsync<DepartmentsWithChildrenDto>(sqlQuery, parameters);

                var materializedDepartments = departments.ToArray();

                var departmentsDict = materializedDepartments.ToDictionary(x => x.Id);

                var rootDepartments = new List<DepartmentsWithChildrenDto>();

                foreach (var row in materializedDepartments)
                {
                    if (row.ParentId.HasValue && departmentsDict.TryGetValue(row.ParentId.Value, out var parent))
                        parent.Children.Add(departmentsDict[row.Id]);
                    else
                        rootDepartments.Add(departmentsDict[row.Id]);
                }

                return rootDepartments;
            },
            tags: [Constants.DEPARTMENT_CACHE_KEY],
            cancellationToken: cancellationToken);

        return departmentsWithChildren;
    }
}
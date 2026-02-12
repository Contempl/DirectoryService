using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Constants;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.Queries.GetChildrenDepartments;

public class GetChildrenHandler : IQueryHandler<GetChildrenQuery, List<DepartmentsWithChildrenDto>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly HybridCache _cache;

    public GetChildrenHandler(IDbConnectionFactory dbConnectionFactory, HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cache = cache;
    }

    public async Task<List<DepartmentsWithChildrenDto>> HandleAsync(GetChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var cacheKey = string.Concat(
            $"{Constants.DEPARTMENT_CACHE_KEY}",
            $"{Constants.CHILDREN_DEPARTMENTS_TAG}",
            "page", query.Request.Page,
            "size", query.Request.Size);


        var parameters = new DynamicParameters(new
        {
            query.ParentId, Offset = (query.Request.Page - 1) * query.Request.Size, query.Request.Size,
        });

        var departments = await _cache.GetOrCreateAsync<List<DepartmentsWithChildrenDto>>(
            key: cacheKey,
            factory: async _ =>
            {
                var sqlQuery = """
                               SELECT
                               d.id, 
                               d.name, 
                               d.identifier,
                               d."ParentId", 
                               d.path, 
                               d.depth, 
                               d.is_active,
                               d.created_at,
                               d.updated_at,
                               d."ChildrenCount", 
                               (EXISTS (SELECT 1 from departments WHERE "ParentId" = d.id)) 
                                                      AS HasMoreChildren
                               FROM departments d
                               WHERE d."ParentId" = @parentId AND d.is_active = true
                               ORDER BY created_at ASC
                               OFFSET @offset LIMIT @size
                               """;
                using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);


                var departmentsQueryResult =
                    await connection.QueryAsync<DepartmentsWithChildrenDto>(sqlQuery, parameters);

                var childrenDepartments = departmentsQueryResult.ToList();

                return childrenDepartments;
            },
            tags: [Constants.DEPARTMENT_CACHE_KEY],
            cancellationToken: cancellationToken);


        return departments;
    }
}
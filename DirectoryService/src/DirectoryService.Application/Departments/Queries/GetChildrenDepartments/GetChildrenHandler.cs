using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Queries.GetChildrenDepartments;

public class GetChildrenHandler : IQueryHandler<GetChildrenQuery, List<DepartmentsWithChildrenDto>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetChildrenHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<List<DepartmentsWithChildrenDto>> HandleAsync(GetChildrenQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters(new
        {
            query.ParentId, Offset = (query.Request.Page - 1) * query.Request.Size, query.Request.Size,
        });
        
        string sqlQuery = """
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

        var departmentsQueryResult = await connection.QueryAsync<DepartmentsWithChildrenDto>(sqlQuery, parameters);

        var childrenDepartments = departmentsQueryResult.ToList();

        return childrenDepartments;
    }
}
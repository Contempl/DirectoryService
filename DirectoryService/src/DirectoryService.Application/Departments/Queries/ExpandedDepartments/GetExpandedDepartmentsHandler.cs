using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Queries.ExpandedDepartments;

public class GetExpandedDepartmentsHandler : IQueryHandler<ExtendedDepartmentsQuery, List<DepartmentsWithChildrenDto>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetExpandedDepartmentsHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<List<DepartmentsWithChildrenDto>> HandleAsync(ExtendedDepartmentsQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters(new
        {
            Offset = (query.Page - 1) * query.Size, query.Size, query.Prefetch
        });
        
        string sqlQuery = """
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
    }
}
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Path = DirectoryService.Domain.Entities.VO.Path;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(ApplicationDbContext dbContext, ILogger<DepartmentRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> CreateAsync(Department department,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(department, cancellationToken);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Created department with id: {department.Id}");

            return Result.Success<Guid, Errors>(department.Id);
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid("invalid.department.values").ToErrors();
        }
    }

    public async Task<Result<Department, Errors>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Departments.FindAsync(id, cancellationToken);

        if (result is null)
        {
            return GeneralErrors.NotFound().ToErrors();
        }

        return result;
    }

    public async Task<Result<Department, Errors>> GetByIdAsNoTrackingAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return department is null
            ? GeneralErrors.NotFound(id).ToErrors()
            : department;
    }

    public async Task<Result<Department, Error>> GetByIdWithLocationsAsync(Guid id, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .Include(d => d.Locations)
            .Where(d => d.IsActive)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (department == null)
            return GeneralErrors.NotFound();

        return department;
    }

    public async Task<Result<Department, Errors>> GetByIdWithLock(Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        // Сделать лок на детей для этой фичи, но возвращать только одно подразделение
        var sql = @"
            SELECT * 
            FROM (
                SELECT 
                    d.id, 
                    d.name AS ""Name_Value"", 
                    d.path, 
                    d.depth, 
                    d.""ParentId"", 
                    d.""ChildrenCount"",
                    d.created_at, 
                    d.updated_at, 
                    d.deleted_at,
                    d.is_active,
                    d.identifier AS ""Identifier_Value""
                FROM departments AS d
                WHERE d.""path"" <@ (SELECT ""path"" FROM departments WHERE departments.id = {0})
                AND d.is_active = true
                FOR UPDATE
                OFFSET 0
            ) AS locked_tree
            WHERE locked_tree.id = {0}";

        var department = await _dbContext.Departments
            .FromSqlRaw(sql, departmentId)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound().ToErrors();

        return department;
    }

    public async Task<List<Guid>> GetDescendantIdsAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           WITH RECURSIVE descendants AS (
                               SELECT child.id, child.depth, child.path
                               FROM departments child
                               WHERE child."ParentId" = @departmentId
                                 AND child.is_active = true
                               UNION ALL
                               SELECT child.id, child.depth, child.path
                               FROM departments child
                               JOIN descendants parent ON child."ParentId" = parent.id
                               WHERE child.is_active = true
                           )
                           SELECT id
                           FROM descendants
                           ORDER BY depth, path
                           """;

        var connection = _dbContext.Database.GetDbConnection();
        var descendantIds = await connection.QueryAsync<Guid>(
            new CommandDefinition(sql, new { departmentId }, cancellationToken: cancellationToken));

        return descendantIds.AsList();
    }

    public async Task<bool> CheckIfDepartmentsExistAsync(List<Guid> departmentIds,
        CancellationToken cancellationToken = default)
    {
        var foundCount = await _dbContext.Departments
            .Where(l => departmentIds.Contains(l.Id) && l.IsActive)
            .CountAsync(cancellationToken);

        return foundCount == departmentIds.Count;
    }

    public async Task<UnitResult<Error>> MoveDepartment(
        Guid departmentId,
        Guid parentId,
        Path parentPath, Path departmentPath, CancellationToken cancellationToken = default)
    {
        string sql = """
                     WITH RECURSIVE subtree AS (
                         SELECT id FROM departments WHERE id = @departmentId
                         UNION ALL
                         SELECT child.id FROM departments child
                         JOIN subtree parent ON child."ParentId" = parent.id
                     )
                     UPDATE departments
                     SET path = @parentPath::ltree || subpath(path, nlevel(@departmentPath::ltree) - 1),
                         depth = nlevel(@parentPath::ltree || subpath(path, nlevel(@departmentPath::ltree) - 1)) - 1,
                         "ParentId" = CASE 
                                        WHEN id = @departmentId THEN @parentId 
                                        ELSE "ParentId" 
                                      END,
                         updated_at = NOW()
                     WHERE id IN (SELECT id FROM subtree)
                     """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            var sqlParams = new
            {
                departmentId,
                parentPath = parentPath.Value, departmentPath = departmentPath.Value, parentId = parentId,
            };

            await dbConnection.ExecuteAsync(sql, sqlParams);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid(ex.Message);
        }
    }

    public async Task<UnitResult<Error>> MoveDepartment(
        Guid departmentId, Path departmentPath, CancellationToken cancellationToken = default)
    {
        string sql = """
                     WITH RECURSIVE subtree AS (
                         SELECT id FROM departments WHERE id = @departmentId
                         UNION ALL
                         SELECT child.id FROM departments child
                         JOIN subtree parent ON child."ParentId" = parent.id
                     )
                     UPDATE departments
                     SET path = subpath(path, nlevel(@departmentPath::ltree) - 1),
                     depth = nlevel(subpath(path, nlevel(@departmentPath::ltree) - 1)) - 1,
                     "ParentId" = CASE
                                      WHEN id = @departmentId THEN null
                                      ELSE "ParentId"
                                  END,
                     updated_at = NOW()
                     WHERE id IN (SELECT id FROM subtree)
                     """;

        var dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            var sqlParams = new { departmentId, departmentPath = departmentPath.Value };

            await dbConnection.ExecuteAsync(sql, sqlParams);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid(ex.Message);
        }
    }

    public async Task<UnitResult<Error>> DeleteLocationsByDepAsync(Guid departmentId,
        CancellationToken cancellationToken)
    {
        var departmentLocationsResult = await _dbContext.DepartmentLocations
            .Where(dl => dl.DepartmentId == departmentId)
            .ExecuteDeleteAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> AddDepLocationsRelationsAsync(
        List<DepartmentLocation> departmentLocations,
        CancellationToken cancellationToken)
    {
        await _dbContext.DepartmentLocations.AddRangeAsync(departmentLocations, cancellationToken);
        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> UpdateTreePathsAsync(
        Path oldPath,
        Path newPath,
        CancellationToken cancellationToken)
    {
        string sql = """
                     UPDATE departments
                     SET path =
                     CASE
                         WHEN nlevel(path) = nlevel(@OldPath::ltree)
                         THEN @NewPath::ltree
                         ELSE
                             @NewPath::ltree
                             ||
                             subpath(path, nlevel(@OldPath::ltree))
                     END
                     WHERE path <@ @OldPath::ltree;
                     """;

        var parameters = new DynamicParameters(new
        {
            oldPath = oldPath.Value,
            newPath = newPath.Value
        });

        await _dbContext.Database.GetDbConnection()
            .ExecuteAsync(sql, parameters);
        
        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> DeactivateOrphanedLocationsAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        string sqlLocations = """
                                  UPDATE locations l
                              SET is_active = false,
                                  updated_at = NOW()
                              WHERE
                                  EXISTS (
                                      SELECT 1
                                      FROM department_locations dl
                                      WHERE dl.location_id = l.id
                                        AND dl.department_id = @deptId
                                  )
                              AND
                                  NOT EXISTS (
                                      SELECT 1
                                      FROM department_locations dl2
                                      JOIN departments d ON d.id = dl2.department_id
                                      WHERE dl2.location_id = l.id
                                        AND d.is_active = true
                                        AND dl2.department_id <> @deptId
                                  );
                              """;
       

        var connection = _dbContext.Database.GetDbConnection();

        var parameters = new DynamicParameters(new { deptId = departmentId });

        await connection.ExecuteAsync(sqlLocations, parameters);
        
        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> DeactivateOrphanedPositionsAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        string sqlPositions = """
                                 UPDATE positions p
                              SET is_active = false,
                                  updated_at = NOW()
                              WHERE
                                  EXISTS (
                                      SELECT 1
                                      FROM department_positions dp
                                      WHERE dp.position_id = p.id
                                        AND dp.department_id = @deptId
                                  )
                              AND
                                  NOT EXISTS (
                                      SELECT 1
                                      FROM department_positions dp2
                                      JOIN departments d ON d.id = dp2.department_id
                                      WHERE dp2.position_id = p.id
                                        AND d.is_active = true
                                        AND dp2.department_id <> @deptId
                                  );
                              """;
        
        var connection = _dbContext.Database.GetDbConnection();

        var parameters = new DynamicParameters(new { deptId = departmentId });
        
        await connection.ExecuteAsync(sqlPositions, parameters);
        
        return UnitResult.Success<Error>();
    }

    public async Task<Result<Department, Errors>> GetByIdForActivityAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId,
            cancellationToken: cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(departmentId, "department").ToErrors();

        return department;
    }
}

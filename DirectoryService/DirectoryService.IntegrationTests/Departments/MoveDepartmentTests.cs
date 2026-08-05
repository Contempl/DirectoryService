using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.Update;
using DirectoryService.IntegrationTests.Departments.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class MoveDepartmentTests : DepartmentsBaseTests
{
    public MoveDepartmentTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }


    [Fact]
    public async Task MoveDepartment_WithValidData_ShouldUpdateDepartment()
    {
        // Arrange
        CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        var departmentIds = await CreateDepartmentsHierarchy(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentRequest(departmentIds[5], departmentIds[4]);

            return sut.HandleAsync(command, ct);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentIds[5], ct);

            Assert.NotNull(department);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            Assert.Equal(5, department.Depth);
        });
    }

    [Fact]
    public async Task MoveDepartment_WithNullParent_ShouldHaveZeroDepth()
    {
        // Arrange
        CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        var departmentIds = await CreateDepartmentsHierarchy(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentRequest(departmentIds[5], null);

            return sut.HandleAsync(command, ct);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentIds[5], ct);

            Assert.NotNull(department);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            Assert.Equal(0, department.Depth);
        });
    }

    [Fact]
    public async Task MoveDepartment_WithNullParent_ShouldPreserveSubtreeHierarchy()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = source.Token;
        var departmentIds = await CreateDepartmentsHierarchy(ct);

        var result = await ExecuteHandler(sut => sut.HandleAsync(
            new UpdateDepartmentRequest(departmentIds[5], null),
            ct));

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentIds[5], ct);
            var child = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentIds[6], ct);

            Assert.True(result.IsSuccess);
            Assert.Null(movedDepartment.ParentId);
            Assert.Equal(0, movedDepartment.Depth);
            Assert.Equal(movedDepartment.Id, child.ParentId);
            Assert.Equal(1, child.Depth);
            Assert.StartsWith(movedDepartment.Path.Value + ".", child.Path.Value);
        });
    }

    [Fact]
    public async Task MoveDepartment_WithInvalidData_ShouldReturnError()
    {
        // Arrange
        CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        var departmentIds = await CreateDepartmentsHierarchy(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentRequest(departmentIds[3], departmentIds[3]);

            return sut.HandleAsync(command, ct);
        });

        // Assert
        Assert.NotNull(result.Error);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "department.move.cycle");
    }
    
    [Fact]
    public async Task MoveDepartment_MoveIntoChildDepartment_ShouldFallWithError()
    {
        // Arrange
        CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken ct = source.Token;
        var departmentIds = await CreateDepartmentsHierarchy(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentRequest(departmentIds[3], departmentIds[4]);

            return sut.HandleAsync(command, ct);
        });

        // Assert
        Assert.NotNull(result.Error);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "department.move.cycle");
    }

    [Fact]
    public async Task MoveDepartment_ToDeletedParent_ShouldReturnStableErrorCode()
    {
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = source.Token;
        var departmentIds = await CreateDepartmentsHierarchy(ct);

        await ExecuteInDb(async dbContext =>
        {
            var parent = await dbContext.Departments.FirstAsync(d => d.Id == departmentIds[1], ct);
            parent.SoftDelete();
            await dbContext.SaveChangesAsync(ct);
        });

        var result = await ExecuteHandler(sut => sut.HandleAsync(
            new UpdateDepartmentRequest(departmentIds[5], departmentIds[1]),
            ct));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "department.move.parent_deleted");
    }

    private async Task<T> ExecuteHandler<T>(Func<ICommandHandler<Guid, UpdateDepartmentRequest>, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<ICommandHandler<Guid, UpdateDepartmentRequest>>();

        return await action(sut);
    }
}

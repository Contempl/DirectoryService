using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Update;
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
        var departmentIds = await CreateDepartmentsHierarchy();

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentRequest(departmentIds[5], departmentIds[4]);

            return sut.HandleAsync(command, CancellationToken.None);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentIds[5], CancellationToken.None);

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
        var departmentIds = await CreateDepartmentsHierarchy();

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentRequest(departmentIds[5], null);

            return sut.HandleAsync(command, CancellationToken.None);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentIds[5], CancellationToken.None);

            Assert.NotNull(department);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            Assert.Equal(0, department.Depth);
        });
    }

    [Fact]
    public async Task MoveDepartment_WithInvalidData_ShouldReturnError()
    {
        // Arrange
        var departmentIds = await CreateDepartmentsHierarchy();

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentRequest(departmentIds[3], departmentIds[3]);

            return sut.HandleAsync(command, CancellationToken.None);
        });

        // Assert
        Assert.NotNull(result.Error);
        Assert.True(result.IsFailure);
    }
    
    [Fact]
    public async Task MoveDepartment_MoveIntoChildDepartment_ShouldFallWithError()
    {
        // Arrange
        var departmentIds = await CreateDepartmentsHierarchy();

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentRequest(departmentIds[3], departmentIds[4]);

            return sut.HandleAsync(command, CancellationToken.None);
        });

        // Assert
        Assert.NotNull(result.Error);
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<ICommandHandler<Guid, UpdateDepartmentRequest>, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<ICommandHandler<Guid, UpdateDepartmentRequest>>();

        return await action(sut);
    }
}
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Commands.ToggleActivity;
using DirectoryService.IntegrationTests.Departments.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class ToggleDepartmentActivityTests : DepartmentsBaseTests
{
    public ToggleDepartmentActivityTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ToggleActivity_ForActiveLeaf_ShouldDeactivateWithoutSoftDelete()
    {
        // Arrange
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = source.Token;
        var department = await CreateDepartmentAsync(ct);
        var previousUpdatedAt = department.UpdatedAt;

        var result = await ExecuteHandler(handler => handler.HandleAsync(
            new ToggleDepartmentActivityRequest(department.Id, false), ct));

        // Act & Assert
        await ExecuteInDb(async dbContext =>
        {
            var updated = await dbContext.Departments.FirstAsync(d => d.Id == department.Id, ct);

            Assert.True(result.IsSuccess);
            Assert.Equal(department.Id, result.Value);
            Assert.False(updated.IsActive);
            Assert.Null(updated.DeletedAt);
            Assert.True(updated.UpdatedAt > previousUpdatedAt);
        });
    }

    [Fact]
    public async Task ToggleActivity_ForInactiveDepartment_ShouldActivate()
    {
        // Arrange
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = source.Token;
        var department = await CreateDepartmentAsync(ct);

        // Act
        await ExecuteInDb(async dbContext =>
        {
            var tracked = await dbContext.Departments.FirstAsync(d => d.Id == department.Id, ct);
            tracked.Deactivate();
            await dbContext.SaveChangesAsync(ct);
        });

        var result = await ExecuteHandler(handler => handler.HandleAsync(
            new ToggleDepartmentActivityRequest(department.Id, true), ct));

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var updated = await dbContext.Departments.FirstAsync(d => d.Id == department.Id, ct);
            Assert.True(result.IsSuccess);
            Assert.True(updated.IsActive);
            Assert.Null(updated.DeletedAt);
        });
    }

    [Fact]
    public async Task ToggleActivity_ForAlreadyInactiveDepartment_ShouldBeIdempotent()
    {
        // Arrange
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = source.Token;
        var department = await CreateDepartmentAsync(ct);

        // Act
        await ExecuteHandler(handler => handler.HandleAsync(
            new ToggleDepartmentActivityRequest(department.Id, false), ct));

        var result = await ExecuteHandler(handler => handler.HandleAsync(
            new ToggleDepartmentActivityRequest(department.Id, false), ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(department.Id, result.Value);
    }

    [Fact]
    public async Task ToggleActivity_WithActiveDescendants_ShouldReturnConflict()
    {
        // Arrange
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = source.Token;
        var departmentIds = await CreateDepartmentsHierarchy(ct);

        // Act
        var result = await ExecuteHandler(handler => handler.HandleAsync(
            new ToggleDepartmentActivityRequest(departmentIds[0], false), ct));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error =>
            error.Code == "department.activity.active_descendants");

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstAsync(d => d.Id == departmentIds[0], ct);
            Assert.True(department.IsActive);
        });
    }

    [Fact]
    public async Task ToggleActivity_ForSoftDeletedDepartment_ShouldReturnConflict()
    {
        // Arrange
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = source.Token;
        var department = await CreateDepartmentAsync(ct);

        // Act
        await ExecuteInDb(async dbContext =>
        {
            var tracked = await dbContext.Departments.FirstAsync(d => d.Id == department.Id, ct);
            tracked.SoftDelete();
            await dbContext.SaveChangesAsync(ct);
        });

        var result = await ExecuteHandler(handler => handler.HandleAsync(
            new ToggleDepartmentActivityRequest(department.Id, true), ct));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.Code == "department.activity.deleted");
    }

    private async Task<T> ExecuteHandler<T>(
        Func<ICommandHandler<Guid, ToggleDepartmentActivityRequest>, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<
            ICommandHandler<Guid, ToggleDepartmentActivityRequest>>();

        return await action(handler);
    }
}

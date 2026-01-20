using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Update;
using DirectoryService.IntegrationTests.Departments.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateLocationTests : DepartmentsBaseTests
{
    public UpdateLocationTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateLocation_WithValidData_ShouldUpdateLocation()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationIds = await CreateLocationsAsync(ct);
        var departmentWithLocations = await CreateDepartmentWithLocationsAsync(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationRequest(departmentWithLocations.Id, locationIds);

            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentWithLocations.Id, CancellationToken.None);

            Assert.NotNull(department);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, department.Id);
        });
    }

    [Fact]
    public async Task UpdateLocation_WithOneLocation_ShouldUpdateSuccessfully()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocation(ct);
        var department = await CreateDepartmentAsync(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationRequest(department.Id, [locationId]);

            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.NotNull(department);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateLocation_WithTheSameLocation_ShouldFallWithError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var department = await CreateDepartmentAsync(ct);
        var locationId = await CreateLocation(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationRequest(department.Id, [locationId, locationId]);

            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.NotNull(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateLocation_WithInvalidData_ShouldReturnError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentWithLocations = await CreateDepartmentWithLocationsAsync(ct);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationRequest(departmentWithLocations.Id, []);

            return sut.Handle(command, CancellationToken.None);
        });

        // Assert
        Assert.NotNull(result.Error);
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<ICommandHandler<UpdateLocationRequest>, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<ICommandHandler<UpdateLocationRequest>>();

        return await action(sut);
    }
}
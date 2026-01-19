using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Create;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DepartmentsBaseTests
{
    
    public CreateDepartmentTests(DirectoryServiceTestWebFactory factory) : base(factory)
    { }
    
    [Fact]
    public async Task CreateDepartment_WithValidData_ShouldSucceed()
    {
        // Arrange
        var locationId = await CreateLocation();
        
        // Act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentRequest("Test Department", "TestDep", null, [locationId]);
            
            return sut.HandleAsync(command, CancellationToken.None);
        });
        
        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Id == result.Value, CancellationToken.None);

            Assert.NotNull(department);
            Assert.True(department.Id == result.Value);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_WithInvalidData_ShouldFail()
    {
        // Act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentRequest("Test Department", "TestDep", null, []);
            
            return sut.HandleAsync(command, CancellationToken.None);
        });
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task CreateDepartment_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        var locationId = await CreateLocation();

        // Act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentRequest("", "TestDep", null, [locationId]);
            
            return sut.HandleAsync(command, CancellationToken.None);
        });
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task CreateDepartment_WithIdentifier_ShouldReturnError()
    {
        // Arrange
        var locationId = await CreateLocation();

        // Act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentRequest("Test Department", "", null, [locationId]);
            
            return sut.HandleAsync(command, CancellationToken.None);
        });
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }



    private async Task<T> ExecuteHandler<T>(Func<ICommandHandler<Guid, CreateDepartmentRequest>, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<ICommandHandler<Guid, CreateDepartmentRequest>>();
        
        return await action(sut);
    }
}

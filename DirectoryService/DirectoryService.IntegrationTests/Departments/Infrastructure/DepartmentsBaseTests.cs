using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.VO;

namespace DirectoryService.IntegrationTests;

public class DepartmentsBaseTests : DirectoryBaseTests
{
    protected DepartmentsBaseTests(DirectoryServiceTestWebFactory factory) : base(factory)
    {
    }

    protected async Task<Guid> CreateLocation()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                Name.Create("Location").Value,
                new Address("Moscow", "street", "house", "apartment"),
                Timezone.Create("UTC").Value).Value;

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return location.Id;
        });
    }

    protected async Task<IEnumerable<Guid>> CreateLocationsAsync()
    {
        var result = await ExecuteInDb(async dbContext =>
        {
            var locations = new List<Location>();
            for (int i = 0; i < 7; i++)
            {
                var location = Location.Create(
                    Name.Create($"Location{i}").Value,
                    new Address($"Moscow{i}", $"{i}", $"house{i}", "apartment"),
                    Timezone.Create("UTC").Value).Value;

                dbContext.Locations.Add(location);
                locations.Add(location);
            }

            await dbContext.SaveChangesAsync();
            return locations;
        });
        return result.Select(l => l.Id);
    }


    protected async Task<Department> CreateDepartmentWithLocationsAsync()
    {
        var timezone = Timezone.Create("UTC").Value;

        var location1 = Location.Create(
            Name.Create("Москва-Сити офис").Value,
            Address.Create("Москва", "Пресненская наб.", "10", "20").Value,
            timezone).Value;

        var location2 = Location.Create(
            Name.Create("Санкт-Петербург офис").Value,
            Address.Create("Санкт-Петербург", "Невский пр.", "25", "5").Value,
            timezone).Value;

        var location3 = Location.Create(
            Name.Create("Казань офис").Value,
            Address.Create("Казань", "Кремлёвская", "7", "3").Value,
            timezone).Value;

        var deptName = Name.Create("Тестовый департамент").Value;
        var identifier = Identifier.Create("main").Value;

        var locations = new[] { location1, location2, location3 };

        var depId = Guid.NewGuid();

        var departmentLocations = locations.Select(location => DepartmentLocation.Create(depId, location.Id)).ToList();

        var department = Department.Create(deptName, identifier, null, null, departmentLocations).Value;


        return await ExecuteInDb<Department>(async (dbContext) =>
        {
            await dbContext.Locations.AddRangeAsync(location1, location2, location3);
            await dbContext.Departments.AddAsync(department, CancellationToken.None);

            await dbContext.SaveChangesAsync(CancellationToken.None);
            return department;
        });
    }

    protected async Task<Department> CreateDepartmentAsync()
    {
        var timezone = Timezone.Create("UTC").Value;

        var location = Location.Create(
            Name.Create("Tula Museum").Value,
            Address.Create("Tula", "Oktyabrskaya.", "10", null).Value,
            timezone).Value;

        var deptName = Name.Create("Museum").Value;
        var identifier = Identifier.Create("museo").Value;

        var locations = new[] { location };

        var depId = Guid.NewGuid();

        var departmentLocations = locations.Select(location => DepartmentLocation.Create(depId, location.Id)).ToList();

        var department = Department.Create(deptName, identifier, null, null, departmentLocations).Value;


        return await ExecuteInDb<Department>(async (dbContext) =>
        {
            await dbContext.Locations.AddAsync(location, CancellationToken.None);
            await dbContext.Departments.AddAsync(department, CancellationToken.None);

            await dbContext.SaveChangesAsync(CancellationToken.None);
            return department;
        });
    }

    protected async Task<List<Guid>> CreateDepartmentsHierarchy()
    {
        var locations = await CreateLocationsAsync();
        var locationIds = locations.ToArray();
        
        var main = Department.Create(
            Name.Create("main").Value,
            Identifier.Create("main").Value,
            null, null, [DepartmentLocation.Create(Guid.NewGuid(), locationIds[0])])
            .Value;

        var moscow = Department.Create(
            Name.Create("moscow").Value,
            Identifier.Create("moscow").Value,
            main.Id, main, [DepartmentLocation.Create(Guid.NewGuid(),locationIds[1] )])
            .Value;
    

        var marketing = Department.Create(
            Name.Create("marketing").Value,
            Identifier.Create("marketing").Value,
            moscow.Id, moscow, [DepartmentLocation.Create(Guid.NewGuid(), locationIds[2])])
            .Value;

        var engineers = Department.Create(
            Name.Create("Engineers").Value,
            Identifier.Create("engineers").Value,
            marketing.Id, marketing, [DepartmentLocation.Create(Guid.NewGuid(), locationIds[3])])
            .Value;

        var sales = Department.Create(
            Name.Create("sales").Value,
            Identifier.Create("sales").Value,
            engineers.Id, engineers, [DepartmentLocation.Create(Guid.NewGuid(), locationIds[4])])
            .Value;

        var shops = Department.Create(
            Name.Create("Shops").Value,
            Identifier.Create("shops").Value,
            sales.Id, sales, [DepartmentLocation.Create(Guid.NewGuid(), locationIds[5])])
            .Value;

        var callCenter = Department.Create(
            Name.Create("CallCenter").Value,
            Identifier.Create("callCenter").Value,
            shops.Id, shops, [DepartmentLocation.Create(Guid.NewGuid() ,locationIds[6])])
            .Value;

        return await ExecuteInDb<List<Guid>>(async dbContext =>
        {
            await dbContext.Departments.AddRangeAsync(
                main, moscow, marketing, engineers,
                sales, shops, callCenter);

            await dbContext.SaveChangesAsync(CancellationToken.None);

            return new List<Guid>
            {
                main.Id, moscow.Id, marketing.Id, engineers.Id,
                sales.Id, shops.Id, callCenter.Id
            };
        });
    }
}
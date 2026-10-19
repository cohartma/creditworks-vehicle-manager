using CreditWorks.Core.Models;
using CreditWorks.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace CreditWorks.IntegrationTests;

public class SqlServerIntegrationTests
{
    [Fact]
    public async Task SqlServer_migrates_seeds_and_persists_vehicle_with_manufacturer()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<CreditWorksDbContext>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(defaultConnectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings:DefaultConnection in the Web project's User Secrets.");
        }

        var testConnectionString = new SqlConnectionStringBuilder(defaultConnectionString)
        {
            InitialCatalog = $"CreditWorksIntegration_{Guid.NewGuid():N}"
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<CreditWorksDbContext>()
            .UseSqlServer(testConnectionString)
            .Options;

        var ownerName = $"Integration test {Guid.NewGuid():N}";

        try
        {
            await using (var dbContext = new CreditWorksDbContext(options))
            {
                await dbContext.Database.MigrateAsync();

                await DatabaseSeeder.SeedAsync(dbContext);
                await DatabaseSeeder.SeedAsync(dbContext);

                Assert.Equal(5, await dbContext.Manufacturers.CountAsync());
                Assert.Equal(3, await dbContext.VehicleCategories.CountAsync());

                var mazda = await dbContext.Manufacturers
                    .SingleAsync(manufacturer => manufacturer.Name == "Mazda");

                dbContext.Vehicles.Add(new Vehicle
                {
                    OwnerName = ownerName,
                    ManufacturerId = mazda.Id,
                    YearOfManufacture = 2020,
                    WeightKg = 1850.75m
                });

                await dbContext.SaveChangesAsync();
            }

            await using (var verificationContext = new CreditWorksDbContext(options))
            {
                var savedVehicle = await verificationContext.Vehicles
                    .AsNoTracking()
                    .Include(vehicle => vehicle.Manufacturer)
                    .SingleAsync(vehicle => vehicle.OwnerName == ownerName);

                Assert.Equal(2020, savedVehicle.YearOfManufacture);
                Assert.Equal(1850.75m, savedVehicle.WeightKg);
                Assert.Equal("Mazda", savedVehicle.Manufacturer.Name);
            }
        }
        finally
        {
            await using var cleanupContext = new CreditWorksDbContext(options);

            if (await cleanupContext.Database.CanConnectAsync())
            {
                await cleanupContext.Database.EnsureDeletedAsync();
            }
        }

    }
}

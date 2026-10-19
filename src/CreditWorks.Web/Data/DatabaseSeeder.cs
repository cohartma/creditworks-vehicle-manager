using CreditWorks.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Web.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CreditWorksDbContext dbContext)
    {
        if (!await dbContext.Manufacturers.AnyAsync())
        {
            dbContext.Manufacturers.AddRange(
                    new Manufacturer { Name = "Mazda" },
                    new Manufacturer { Name = "Mercedes" },
                    new Manufacturer { Name = "Honda" },
                    new Manufacturer { Name = "Ferrari" },
                    new Manufacturer { Name = "Toyota" });
        }

        if (!await dbContext.VehicleCategories.AnyAsync())
        {
            dbContext.VehicleCategories.AddRange(
                    new VehicleCategory
                    {
                        Name = "Light",
                        IconKey = "car-front",
                        MinimumWeightKg = 0.01m,
                        MaximumWeightKg = 500.01m
                    },
                    new VehicleCategory
                    {
                        Name = "Medium",
                        IconKey = "truck-front",
                        MinimumWeightKg = 500.01m,
                        MaximumWeightKg = 2500.01m
                    },
                    new VehicleCategory
                    {
                        Name = "Heavy",
                        IconKey = "truck",
                        MinimumWeightKg = 2500.01m,
                        MaximumWeightKg = null
                    });
        }
        await dbContext.SaveChangesAsync();
    }
}

using CreditWorks.Core.Models;
using CreditWorks.Core.Services;

namespace CreditWorks.Tests;

public class VehicleSortingTests
{
    [Theory]
    [InlineData("owner", false, "Alice", "Mike", "Zoe")]
    [InlineData("owner", true, "Zoe", "Mike", "Alice")]
    [InlineData("manufacturer", false, "Mike", "Alice", "Zoe")]
    [InlineData("manufacturer", true, "Zoe", "Alice", "Mike")]
    [InlineData("year", false, "Mike", "Zoe", "Alice")]
    [InlineData("year", true, "Alice", "Zoe", "Mike")]
    [InlineData("weight", false, "Alice", "Zoe", "Mike")]
    [InlineData("weight", true, "Mike", "Zoe", "Alice")]
    public void Apply_sorts_vehicles_by_requested_field(
            string sortBy,
            bool descending,
            string firstOwner,
            string secondOwner,
            string thirdOwner)
    {
        var sortedOwners = VehicleSorting
            .Apply(CreateVehicles().AsQueryable(), sortBy, descending)
            .Select(vehicle => vehicle.OwnerName)
            .ToArray();

        Assert.Equal(
                new[] { firstOwner, secondOwner, thirdOwner },
                sortedOwners);
    }

    private static List<Vehicle> CreateVehicles() =>
    [
        new()
        {
            OwnerName = "Zoe",
            Manufacturer = new Manufacturer { Name = "Toyota" },
            YearOfManufacture = 2019,
            WeightKg = 1800m
        },
        new()
        {
            OwnerName = "Alice",
            Manufacturer = new Manufacturer { Name = "Mazda" },
            YearOfManufacture = 2021,
            WeightKg = 1000m
        },
        new()
        {
            OwnerName = "Mike",
            Manufacturer = new Manufacturer { Name = "Honda" },
            YearOfManufacture = 2015,
            WeightKg = 2500m
        }
    ];
}


using CreditWorks.Core.Models;

namespace CreditWorks.Core.Services;

public static class VehicleSorting
{
    public static IQueryable<Vehicle> Apply(
            IQueryable<Vehicle> vehicles,
            string sortBy,
            bool descending)
    {
        return (sortBy, descending) switch
        {
            ("owner", false) => vehicles.OrderBy(vehicle => vehicle.OwnerName),
            ("owner", true) => vehicles.OrderByDescending(vehicle => vehicle.OwnerName),
            ("manufacturer", false) =>
                vehicles.OrderBy(vehicle => vehicle.Manufacturer.Name),
            ("manufacturer", true) =>
                vehicles.OrderByDescending(vehicle => vehicle.Manufacturer.Name),
            ("year", false) =>
                vehicles.OrderBy(vehicle => vehicle.YearOfManufacture),
            ("year", true) =>
                vehicles.OrderByDescending(vehicle => vehicle.YearOfManufacture),
            ("weight", false) => vehicles.OrderBy(vehicle => vehicle.WeightKg),
            ("weight", true) => vehicles.OrderByDescending(vehicle => vehicle.WeightKg),
            _ => descending
                ? vehicles.OrderByDescending(vehicle => vehicle.OwnerName)
                : vehicles.OrderBy(vehicle => vehicle.OwnerName)
        };
    }
}

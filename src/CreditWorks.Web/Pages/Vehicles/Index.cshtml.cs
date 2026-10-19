using Microsoft.AspNetCore.Mvc.RazorPages;

using CreditWorks.Core.Models;
using CreditWorks.Web.Data;
using Microsoft.EntityFrameworkCore;
using CreditWorks.Core.Services;

namespace CreditWorks.Web.Pages.Vehicles;

public class IndexModel : PageModel
{
    private readonly CreditWorksDbContext _dbContext;

    public IndexModel(CreditWorksDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public IReadOnlyList<VehicleListItem> Vehicles { get; private set; } = [];
    public string SortBy { get; private set; } = "owner";
    public bool Descending { get; private set; }

    public async Task OnGetAsync(
            string? sortBy = "owner",
            bool descending = false)
    {
        var allowedSorts = new[] { "owner", "manufacturer", "year", "weight" };
        SortBy = allowedSorts.Contains(sortBy) ? sortBy! : "owner";
        Descending = descending;

        var query = _dbContext.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.Manufacturer);

        var orderedQuery = VehicleSorting.Apply(query, SortBy, Descending);

        var vehicles = await orderedQuery.ToListAsync();
        var categories = await _dbContext.VehicleCategories
            .AsNoTracking()
            .ToListAsync();

        Vehicles = [.. vehicles
            .Select(vehicle => new VehicleListItem(
                        vehicle.OwnerName,
                        vehicle.Manufacturer.Name,
                        vehicle.YearOfManufacture,
                        vehicle.WeightKg,
                        VehicleCategoryResolver.Resolve(categories, vehicle.WeightKg)))];
    }
}

public record VehicleListItem(
    string OwnerName,
    string ManufacturerName,
    int YearOfManufacture,
    decimal WeightKg,
    VehicleCategory Category
);

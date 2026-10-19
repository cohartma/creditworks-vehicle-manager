using System.ComponentModel.DataAnnotations;
using CreditWorks.Core.Models;
using CreditWorks.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CreditWorks.Core.Services;

namespace CreditWorks.Web.Pages.Vehicles;

public class CreateModel : PageModel
{
    private readonly CreditWorksDbContext _dbContext;

    public CreateModel(CreditWorksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public VehicleInput Input { get; set; } = new();

    public List<SelectListItem> Manufacturers { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await LoadManufacturersAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!VehicleValidationRules.IsValidYearOfManufacture(
                    Input.YearOfManufacture,
                    DateTime.UtcNow.Year))
        {
            ModelState.AddModelError(
                "Input.YearOfManufacture",
                "Enter a sensible year of manufacture.");
        }

        if (!VehicleValidationRules.IsValidWeightKg(Input.WeightKg))
        {
            ModelState.AddModelError(
                "Input.WeightKg",
                "Weight must be positive and have no more than two decimal places.");
        }

        var manufacturerExists = await _dbContext.Manufacturers
            .AnyAsync(manufacturer => manufacturer.Id == Input.ManufacturerId);

        if (!manufacturerExists)
        {
            ModelState.AddModelError(
                "Input.ManufacturerId",
                "Select a manufacturer from the list.");
        }

        if (!ModelState.IsValid)
        {
            await LoadManufacturersAsync();
            return Page();
        }

        var vehicle = new Vehicle
        {
            OwnerName = Input.OwnerName.Trim(),
            ManufacturerId = Input.ManufacturerId,
            YearOfManufacture = Input.YearOfManufacture,
            WeightKg = Input.WeightKg
        };

        _dbContext.Vehicles.Add(vehicle);
        await _dbContext.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task LoadManufacturersAsync()
    {
        Manufacturers = await _dbContext.Manufacturers
            .AsNoTracking()
            .OrderBy(manufacturer => manufacturer.Name)
            .Select(manufacturer => new SelectListItem
            {
                Value = manufacturer.Id.ToString(),
                Text = manufacturer.Name
            })
            .ToListAsync();
    }

    public class VehicleInput
    {
        [Required]
        [StringLength(200)]
        public string OwnerName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ManufacturerId { get; set; }

        public int YearOfManufacture { get; set; }

        [Range(typeof(decimal), "0.01", "9999999.99")]
        public decimal WeightKg { get; set; }
    }
}

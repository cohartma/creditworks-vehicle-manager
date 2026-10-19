using CreditWorks.Core.Models;
using CreditWorks.Core.Services;
using CreditWorks.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace CreditWorks.Web.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly CreditWorksDbContext _dbContext;

    public IndexModel(CreditWorksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<VehicleCategory> Categories { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var categories = await _dbContext.VehicleCategories
            .OrderBy(category => category.MinimumWeightKg)
            .ToListAsync();

        var categoryToDelete = categories
            .SingleOrDefault(category => category.Id == id);

        if (categoryToDelete is null)
        {
            ModelState.AddModelError(
                    string.Empty,
                    "That category could not be found.");
            await LoadCategoriesAsync();
            return Page();
        }

        var errors = VehicleCategoryDeletionService
            .DeleteAndMerge(categories, id);

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            await LoadCategoriesAsync();
            return Page();
        }

        _dbContext.VehicleCategories.Remove(categoryToDelete);
        await _dbContext.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task LoadCategoriesAsync()
    {
        Categories = await _dbContext.VehicleCategories
            .AsNoTracking()
            .OrderBy(category => category.MinimumWeightKg)
            .ToListAsync();
    }
}

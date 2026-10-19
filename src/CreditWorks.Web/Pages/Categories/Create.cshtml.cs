using System.ComponentModel.DataAnnotations;
using CreditWorks.Core.Models;
using CreditWorks.Core.Services;
using CreditWorks.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CreditWorks.Web.Display;

namespace CreditWorks.Web.Pages.Categories;

public class CreateModel : PageModel
{
    private readonly CreditWorksDbContext _dbContext;

    public CreateModel(CreditWorksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public CategoryInput Input { get; set; } = new();

    public List<SelectListItem> CategoriesToSplit { get; private set; } = [];
    public List<SelectListItem> IconOptions => CategoryIconCatalog.GetOptions();

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var categories = await _dbContext.VehicleCategories
            .OrderBy(category => category.MinimumWeightKg)
            .ToListAsync();

        var categoryToSplit = categories
            .SingleOrDefault(category => category.Id == Input.CategoryToSplitId);

        if (categoryToSplit is null)
        {
            ModelState.AddModelError(
                    string.Empty,
                    "Select a category to split.");
        }
        else
        {
            if (Input.SplitAtKg != decimal.Round(Input.SplitAtKg, 2))
            {
                ModelState.AddModelError(
                        string.Empty,
                        "The split can have no more than two decimal places.");
            }

            if (Input.SplitAtKg <= categoryToSplit.MinimumWeightKg ||
                    (categoryToSplit.MaximumWeightKg is decimal maximum &&
                     Input.SplitAtKg >= maximum))
            {
                ModelState.AddModelError(
                        string.Empty,
                        "The split must be inside the selected category's range.");
            }
        }


        if (!CategoryIconCatalog.IsSupported(Input.IconKey))
        {
            ModelState.AddModelError(
                    "Input.IconKey",
                    "Choose an icon from the list.");
        }

        if (categoryToSplit is null)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        var newCategory = new VehicleCategory
        {
            Name = Input.Name?.Trim() ?? string.Empty,
            IconKey = Input.IconKey?.Trim() ?? string.Empty,
            MinimumWeightKg = Input.SplitAtKg,
            MaximumWeightKg = categoryToSplit.MaximumWeightKg
        };


        var proposedCategories = categories
            .Select(category => new VehicleCategory
            {
                Id = category.Id,
                Name = category.Name,
                IconKey = category.IconKey,
                MinimumWeightKg = category.MinimumWeightKg,
                MaximumWeightKg = category.Id == categoryToSplit.Id
                        ? Input.SplitAtKg
                        : category.MaximumWeightKg
            })
        .Append(newCategory)
            .ToList();

        var configurationErrors =
            VehicleCategoryConfigurationValidator.Validate(proposedCategories);

        if (configurationErrors.Count > 0)
        {
            foreach (var error in configurationErrors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            await LoadCategoriesAsync();
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        categoryToSplit.MaximumWeightKg = Input.SplitAtKg;
        _dbContext.VehicleCategories.Add(newCategory);

        await _dbContext.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _dbContext.VehicleCategories
            .AsNoTracking()
            .OrderBy(category => category.MinimumWeightKg)
            .ToListAsync();

        CategoriesToSplit = [.. categories
            .Select(category => new SelectListItem
                    {
                        Value = category.Id.ToString(),
                        Text = category.MaximumWeightKg is decimal maximum
                            ? $"{category.Name} ({category.MinimumWeightKg:N2} to {maximum:N2} kg)"
                            : $"{category.Name} ({category.MinimumWeightKg:N2} kg and above)"
                    })];
    }

    public class CategoryInput
    {
        [Range(1, int.MaxValue)]
        public int CategoryToSplitId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string IconKey { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.02", "9999999.99")]
        public decimal SplitAtKg { get; set; }

    }
}

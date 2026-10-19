using System.ComponentModel.DataAnnotations;
using CreditWorks.Core.Models;
using CreditWorks.Core.Services;
using CreditWorks.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CreditWorks.Web.Display;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CreditWorks.Web.Pages.Categories;

public class EditModel : PageModel
{
    private readonly CreditWorksDbContext _dbContext;

    public EditModel(CreditWorksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public List<CategoryInput> Categories { get; set; } = [];
    public List<SelectListItem> IconOptions => CategoryIconCatalog.GetOptions();

    public async Task<IActionResult> OnGetAsync()
    {
        var categories = await _dbContext.VehicleCategories
            .AsNoTracking()
            .OrderBy(category => category.MinimumWeightKg)
            .ToListAsync();

        Categories = categories
            .Select(category => new CategoryInput
            {
                Id = category.Id,
                Name = category.Name,
                IconKey = category.IconKey,
                MinimumWeightKg = category.MinimumWeightKg,
                MaximumWeightKg = category.MaximumWeightKg
            })
                .ToList();
        return Page();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        var savedCategories = await _dbContext.VehicleCategories
            .ToListAsync();

        var savedIds = savedCategories
            .Select(category => category.Id)
            .Order()
            .ToList();

        var submittedIds = Categories
            .Select(category => category.Id)
            .Order()
            .ToList();

        if (!savedIds.SequenceEqual(submittedIds))
        {
            ModelState.AddModelError(
                    string.Empty,
                    "The category list changed while you were editing. Reload the page and try again.");
        }

        var proposedCategories = Categories
            .Select(input => new VehicleCategory
            {
                Id = input.Id,
                Name = input.Name,
                IconKey = input.IconKey,
                MinimumWeightKg = input.MinimumWeightKg,
                MaximumWeightKg = input.MaximumWeightKg
            })
            .ToList();

        foreach (var error in VehicleCategoryConfigurationValidator
                .Validate(proposedCategories))
        {
            ModelState.AddModelError(string.Empty, error);
        }


        for (var index = 0; index < Categories.Count; index++)
        {
            if (!CategoryIconCatalog.IsSupported(Categories[index].IconKey))
            {
                ModelState.AddModelError(
                        $"Categories[{index}].IconKey",
                        "Choose an icon from the list.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var savedById = savedCategories.ToDictionary(category => category.Id);

        foreach (var input in Categories)
        {
            var category = savedById[input.Id];
            category.Name = input.Name.Trim();
            category.IconKey = input.IconKey.Trim();
            category.MinimumWeightKg = input.MinimumWeightKg;
            category.MaximumWeightKg = input.MaximumWeightKg;
        }

        await _dbContext.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    public class CategoryInput
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [Required]
        [StringLength(50)]
        public string IconKey { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999.99")]
        public decimal MinimumWeightKg { get; set; }

        [Range(typeof(decimal), "0.01", "9999999.99")]
        public decimal? MaximumWeightKg { get; set; }
    }
}



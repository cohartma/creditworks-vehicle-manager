using CreditWorks.Core.Models;

namespace CreditWorks.Core.Services;

public static class VehicleCategoryDeletionService
{
    public static IReadOnlyList<string> DeleteAndMerge(
            IList<VehicleCategory> categories,
            int categoryId)
    {
        var ordered = categories
            .OrderBy(category => category.MinimumWeightKg)
            .ToList();

        var categoryToDelete = ordered
            .SingleOrDefault(category => category.Id == categoryId);

        if (categoryToDelete is null)
        {
            return ["The category to delete could not be found."];
        }

        if (ordered.Count == 1)
        {
            return ["At least one category must remain."];
        }

        var deleteIndex = ordered.FindIndex(category => category.Id == categoryId);

        var previousCategory = deleteIndex > 0
            ? ordered[deleteIndex - 1]
            : null;

        var nextCategory = deleteIndex == 0
            ? ordered[1]
            : null;

        var proposedCategories = ordered
            .Where(category => category.Id != categoryId)
            .Select(category => new VehicleCategory
            {
                Id = category.Id,
                Name = category.Name,
                IconKey = category.IconKey,
                MinimumWeightKg = nextCategory?.Id == category.Id
                            ? 0.01m
                            : category.MinimumWeightKg,
                MaximumWeightKg = previousCategory?.Id == category.Id
                            ? categoryToDelete.MaximumWeightKg
                            : category.MaximumWeightKg
            })
        .ToList();

        var errors = VehicleCategoryConfigurationValidator
            .Validate(proposedCategories);

        if (errors.Count > 0)
        {
            return errors;
        }


        if (previousCategory is not null)
        {
            previousCategory.MaximumWeightKg =
                categoryToDelete.MaximumWeightKg;
        }
        else
        {
            nextCategory!.MinimumWeightKg = 0.01m;
        }

        categories.Remove(categoryToDelete);

        return [];
    }
}

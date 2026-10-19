using CreditWorks.Core.Models;

namespace CreditWorks.Core.Services;

public static class VehicleCategoryConfigurationValidator
{
    public static IReadOnlyList<string> Validate(
            IEnumerable<VehicleCategory> categories)
    {
        var errors = new List<string>();
        var ordered = categories
            .OrderBy(category => category.MinimumWeightKg)
            .ToList();

        if (ordered.Count == 0)
        {
            errors.Add("At least one category is required.");
            return errors;
        }

        foreach (var category in ordered)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                errors.Add("Every category must have a name.");
            }

            if (string.IsNullOrWhiteSpace(category.IconKey))
            {
                errors.Add($"Category '{category.Name}' must have an icon.");
            }

            if (category.MinimumWeightKg < 0.01m ||
                    decimal.Round(category.MinimumWeightKg, 2) != category.MinimumWeightKg)
            {
                errors.Add(
                        $"Category '{category.Name}' must have a minimum weight of at least 0.01 kg with at most two decimal places."
                    );
            }

            if (category.MaximumWeightKg is decimal maximum)
            {
                if (decimal.Round(maximum, 2) != maximum)
                {
                    errors.Add(
                            $"Category {category.Name}' maximum weight must have at most two decimal places.");
                }

                if (maximum <= category.MinimumWeightKg)
                {
                    errors.Add(
                            $"Category {category.Name}' maximum weight must be greater than its minimum.");
                }
            }
        }

        var duplicateNames = ordered
            .Where(category => !string.IsNullOrWhiteSpace(category.Name))
            .GroupBy(
                    category => category.Name.Trim(),
                    StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1);

        foreach (var group in duplicateNames)
        {
            errors.Add($"Category name '{group.Key}' is used more than once.");
        }

        if (ordered[0].MinimumWeightKg != 0.01m)
        {
            errors.Add("The first category must start at 0.01kg.");
        }

        for (var index = 0; index < ordered.Count - 1; index++)
        {
            var current = ordered[index];
            var next = ordered[index + 1];

            if (current.MaximumWeightKg is null)
            {
                errors.Add($"Only the final category can have no maximum weight.");
            }
            else if (current.MaximumWeightKg < next.MinimumWeightKg)
            {
                errors.Add($"There is a gap between '{current.Name}' and '{next.Name}'.");
            }
            else if (current.MaximumWeightKg > next.MinimumWeightKg)
            {
                errors.Add($"'{current.Name}' overlaps with '{next.Name}'.");
            }
        }

        if (ordered[^1].MaximumWeightKg is not null)
        {
            errors.Add("The final category must have no maximum weight.");
        }

        return errors;
    }
}

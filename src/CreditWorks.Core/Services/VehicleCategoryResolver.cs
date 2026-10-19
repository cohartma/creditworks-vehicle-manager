using CreditWorks.Core.Models;

namespace CreditWorks.Core.Services;

public static class VehicleCategoryResolver
{
    public static VehicleCategory Resolve(
            IEnumerable<VehicleCategory> categories,
            decimal weightKg)
    {
        if (weightKg <= 0 || decimal.Round(weightKg, 2) != weightKg)
        {
            throw new ArgumentOutOfRangeException(
                    nameof(weightKg),
                    "Weight must be positive and have no more than two decimal places.");
        }

        var matches = categories
            .Where(category =>
                    weightKg >= category.MinimumWeightKg &&
                    (category.MaximumWeightKg is null ||
                     weightKg < category.MaximumWeightKg.Value))
            .Take(2)
            .ToList();

        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                    "No category is configured for this vehicle weight.");
        }


        if (matches.Count > 1)
        {
            throw new InvalidOperationException(
                    "More than one category matches this vehicle weight.");
        }

        return matches[0];
    }
}


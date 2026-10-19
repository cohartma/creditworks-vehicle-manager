namespace CreditWorks.Core.Services;

public static class VehicleValidationRules
{
    public static bool IsValidYearOfManufacture(int year, int currentYear) =>
        year >= 1886 && year <= currentYear + 1;

    public static bool IsValidWeightKg(decimal weightKg) =>
        weightKg > 0 && weightKg <= 9_999_999.99m &&
        decimal.Round(weightKg, 2) == weightKg;
}

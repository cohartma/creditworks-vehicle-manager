using CreditWorks.Core.Services;

namespace CreditWorks.Tests;

public class VehicleValidationRulesTests
{
    [Theory]
    [InlineData(1885, false)]
    [InlineData(1886, true)]
    [InlineData(2027, true)]
    [InlineData(2028, false)]
    public void IsValidYearOfManufacture_checks_year_bounds(
        int year,
        bool expected)
    {
        const int currentYear = 2026;

        Assert.Equal(
                expected,
                VehicleValidationRules.IsValidYearOfManufacture(year, currentYear));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1.234, false)]
    [InlineData(0.01, true)]
    [InlineData(1850.75, true)]
    [InlineData(9999999.99, true)]
    [InlineData(10000000, false)]
    public void IsValidWeightKg_checks_positive_two_decimal_place_weights(
            decimal weightKg,
            bool expected)
    {
        Assert.Equal(expected, VehicleValidationRules.IsValidWeightKg(weightKg));
    }
}

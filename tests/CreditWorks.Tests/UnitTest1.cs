using CreditWorks.Core.Models;
using CreditWorks.Core.Services;


namespace CreditWorks.Tests;


public class VehicleCategoryResolverTests
{
    [Theory]
    [InlineData(500.00, "Light")]
    [InlineData(500.01, "Medium")]
    [InlineData(2500.00, "Medium")]
    [InlineData(2500.01, "Heavy")]
    public void Resolve_returns_category_at_boundry(
            decimal weightKg,
            string expectedCategory)
    {
        var category = VehicleCategoryResolver.Resolve(
                CreateCategories(),
                weightKg);

        Assert.Equal(expectedCategory, category.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.234)]
    public void Resolve_rejects_invalid_weight(decimal weightKg)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
                () => VehicleCategoryResolver.Resolve(
                                                      CreateCategories(),
                                                      weightKg));
    }

    [Fact]
    public void Resolve_uses_updated_ranges_for_an_existing_vehicle()
    {
        var categories = CreateCategories();
        const decimal vehicleWeightKg = 2200m;

        var originalCategory = VehicleCategoryResolver.Resolve(
                categories,
                vehicleWeightKg);

        Assert.Equal("Medium", originalCategory.Name);

        // Move medium / heavy boundary down from 2500.01 to 2000.01
        categories[1].MaximumWeightKg = 2000.01m;
        categories[2].MinimumWeightKg = 2000.01m;

        var updatedCategory = VehicleCategoryResolver.Resolve(
                categories,
                vehicleWeightKg);

        Assert.Equal("Heavy", updatedCategory.Name);
    }


    [Fact]
    public void Validate_accepts_complete_category_ranges()
    {
        var errors = VehicleCategoryConfigurationValidator.Validate(
                CreateCategories());

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_rejects_a_gap()
    {
        var categories = CreateCategories();
        categories[0].MaximumWeightKg = 500.00m;

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Contains(
                errors,
                error => error.Contains("gap", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_category_names_that_differ_only_by_case()
    {
        var categories = CreateCategories();
        categories[1].Name = "light";

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Contains(
                errors,
                error => error.Contains(
                    "used more than once",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_category_names_with_extra_spaces()
    {
        var categories = CreateCategories();
        categories[1].Name = " Light ";

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Contains(
                errors,
                error => error.Contains(
                    "used more than once",
                    StringComparison.OrdinalIgnoreCase));
    }


    [Fact]
    public void Validate_rejects_an_overlap()
    {
        var categories = CreateCategories();
        categories[0].MaximumWeightKg = 500.02m;

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Contains(
                errors,
                error => error.Contains("overlaps", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_a_non_final_category_without_a_maximum()
    {
        var categories = CreateCategories();
        categories[0].MaximumWeightKg = null;

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Single(errors);
        Assert.Contains(
                errors,
                error => error.Contains(
                    "Only the final category",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_a_final_category_with_a_maximum()
    {
        var categories = CreateCategories();
        categories[^1].MaximumWeightKg = 3000.01m;

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Single(errors);
        Assert.Contains(
                errors,
                error => error.Contains(
                    "final category must have no maximum",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_an_empty_category_list()
    {
        var errors = VehicleCategoryConfigurationValidator.Validate(
                []);

        Assert.Single(errors);
        Assert.Contains(
                errors,
                error => error.Contains(
                    "At least one category is required",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_a_first_category_that_does_not_start_at_point_zero_one()
    {
        var categories = CreateCategories();
        categories[0].MinimumWeightKg = 0.02m;

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Contains(
                errors,
                error => error.Contains(
                    "first category must start",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_a_minimum_weight_with_more_than_two_decimal_places()
    {
        var categories = CreateCategories();
        categories[1].MinimumWeightKg = 500.011m;

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Contains(
                errors,
                error => error.Contains(
                    "at most two decimal places",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_rejects_a_maximum_weight_with_more_than_two_decimal_places()
    {
        var categories = CreateCategories();
        categories[0].MaximumWeightKg = 500.011m;

        var errors = VehicleCategoryConfigurationValidator.Validate(categories);

        Assert.Contains(
                errors,
                error => error.Contains(
                    "maximum weight must have at most two decimal places",
                    StringComparison.OrdinalIgnoreCase));

    }

    private static VehicleCategory[] CreateCategories()
    {
        return
            [
            new VehicleCategory
            {
                Name = "Light",
                IconKey = "light",
                MinimumWeightKg = 0.01m,
                MaximumWeightKg = 500.01m
            },
            new VehicleCategory
            {
                Name = "Medium",
                IconKey = "medium",
                MinimumWeightKg = 500.01m,
                MaximumWeightKg = 2500.01m
            },
            new VehicleCategory
            {
                Name = "Heavy",
                IconKey = "heavy",
                MinimumWeightKg = 2500.01m,
                MaximumWeightKg = null
            }
            ];
    }
}

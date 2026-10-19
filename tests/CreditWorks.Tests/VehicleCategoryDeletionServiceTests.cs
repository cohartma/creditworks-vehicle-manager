using CreditWorks.Core.Models;
using CreditWorks.Core.Services;

namespace CreditWorks.Tests;


public class VehicleCategoryDeletionServiceTests
{

    [Fact]
    public void DedleteAndMerge_DeletingFirstCategory_MovesNextMinimumToPoiintZeroOne()
    {
        var categories = CreateCategories();

        var errors = VehicleCategoryDeletionService.DeleteAndMerge(categories, categoryId: 1);

        Assert.Empty(errors);
        Assert.Equal(2, categories.Count);
        Assert.Equal(0.01m, categories[0].MinimumWeightKg);
        Assert.Equal(5000m, categories[0].MaximumWeightKg);
        Assert.Equal(5000m, categories[1].MinimumWeightKg);
        Assert.Null(categories[1].MaximumWeightKg);
    }

    [Fact]
    public void DeleteAndMerge_DeletingMiddleCategory_ExtendsPreviousMaximum()
    {
        var categories = CreateCategories();

        var errors = VehicleCategoryDeletionService.DeleteAndMerge(categories, categoryId: 2);

        Assert.Empty(errors);
        Assert.Equal(2, categories.Count);
        Assert.Equal(0.01m, categories[0].MinimumWeightKg);
        Assert.Equal(5000m, categories[0].MaximumWeightKg);
        Assert.Equal(5000m, categories[1].MinimumWeightKg);
        Assert.Null(categories[1].MaximumWeightKg);
    }


    [Fact]
    public void DeleteAndMerge_DeletingLastCategory_MakesPreviousRangeOpenEnded()
    {
        var categories = CreateCategories();

        var errors = VehicleCategoryDeletionService.DeleteAndMerge(categories, categoryId: 3);

        Assert.Empty(errors);
        Assert.Equal(2, categories.Count);
        Assert.Equal(0.01m, categories[0].MinimumWeightKg);
        Assert.Equal(2500m, categories[0].MaximumWeightKg);
        Assert.Equal(2500m, categories[1].MinimumWeightKg);
        Assert.Null(categories[1].MaximumWeightKg);
    }

    [Fact]
    public void DeleteAndMerge_DeletingOnlyCategory_ReturnsErrorAndKeepsCategory()
    {
        var categories = new List<VehicleCategory>
        {
            new()
            {
                Id = 1,
                Name = "Light",
                IconKey = "car-front",
                MinimumWeightKg = 0.01m,
                MaximumWeightKg = null
            }
        };

        var errors = VehicleCategoryDeletionService.DeleteAndMerge(categories, categoryId: 1);

        Assert.NotEmpty(errors);
        Assert.Single(categories);
        Assert.Equal("Light", categories[0].Name);
    }


    [Fact]
    public void DeleteAndMerge_UnknownCategoryId_ReturnsErrorAndKeepsCategories()
    {
        var categories = CreateCategories();

        var errors = VehicleCategoryDeletionService.DeleteAndMerge(categories, categoryId: 99);

        Assert.NotEmpty(errors);
        Assert.Equal(3, categories.Count);
        Assert.Equal(2500m, categories[0].MaximumWeightKg);
        Assert.Equal(5000m, categories[1].MaximumWeightKg);
    }

    private static List<VehicleCategory> CreateCategories() =>
        [
        new()
        {
            Id = 1,
            Name = "Light",
            IconKey = "car-front",
            MinimumWeightKg = 0.01m,
            MaximumWeightKg = 2500m
        },
        new()
        {
            Id = 2,
            Name = "Medium",
            IconKey = "truck",
            MinimumWeightKg = 2500m,
            MaximumWeightKg = 5000m
        },
        new()
        {
            Id = 3,
            Name = "Heavy",
            IconKey = "truck-front",
            MinimumWeightKg = 5000m,
            MaximumWeightKg = null
        }
    ];
}

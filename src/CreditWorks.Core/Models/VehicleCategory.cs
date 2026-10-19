namespace CreditWorks.Core.Models;

public class VehicleCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public decimal MinimumWeightKg { get; set; }
    public decimal? MaximumWeightKg { get; set; }
}

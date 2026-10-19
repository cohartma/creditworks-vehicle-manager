using Microsoft.AspNetCore.Mvc.Rendering;

namespace CreditWorks.Web.Display;

public static class CategoryIconCatalog
{
    private static readonly Dictionary<string, string> Icons = new()
    {

        ["car-front"] = "Car front",
        ["car-front-fill"] = "Car front filled",
        ["truck-front"] = "Truck front",
        ["truck"] = "Truck",
    };

    public static bool IsSupported(string? iconKey)
    {
        return iconKey is not null && Icons.ContainsKey(iconKey);
    }

    public static List<SelectListItem> GetOptions()
    {
        return Icons
            .Select(icon => new SelectListItem
            {
                Value = icon.Key,
                Text = icon.Value
            })
        .ToList();
    }
}

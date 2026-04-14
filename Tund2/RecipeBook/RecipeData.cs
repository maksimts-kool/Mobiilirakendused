namespace Tund2;

public class RecipeData
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string DishType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public List<string> Ingredients { get; set; } = new();

    public DateTime CookingDate { get; set; } = DateTime.Today;

    public int CookingTimeMinutes { get; set; } = 45;

    public int Portions { get; set; } = 2;

    public string Difficulty { get; set; } = "Keskmine";

    public bool IsVegetarian { get; set; }

    public bool IsSweet { get; set; }

    public string Instructions { get; set; } = string.Empty;
}

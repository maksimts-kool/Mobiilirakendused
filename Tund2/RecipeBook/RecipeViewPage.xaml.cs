namespace Tund2;

public partial class RecipeViewPage : ContentPage
{
    private readonly Guid recipeId;

    public RecipeViewPage(Guid recipeId)
    {
        InitializeComponent();
        this.recipeId = recipeId;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadRecipe();
    }

    private async void LoadRecipe()
    {
        var recipe = RecipeStore.GetById(recipeId);

        if (recipe is null)
        {
            await DisplayAlertAsync("Viga", "Retsepti ei leitud.", "OK");
            await Navigation.PopAsync();
            return;
        }

        RecipeNameLabel.Text = recipe.Name;
        RecipeTypeLabel.Text = string.IsNullOrWhiteSpace(recipe.DishType) ? "Tüüp puudub" : recipe.DishType;
        RecipeDescriptionLabel.Text = string.IsNullOrWhiteSpace(recipe.Description) ? "Kirjeldus puudub" : recipe.Description;

        AuthorLabel.Text = $"Autor: {GetTextOrDefault(recipe.Author, "Puudub")}";
        CookingDateLabel.Text = $"Kuupäev: {recipe.CookingDate:dd.MM.yyyy}";
        CookingTimeLabel.Text = $"Valmistusaeg: {recipe.CookingTimeMinutes} min";
        PortionsLabel.Text = recipe.Portions == 1
            ? "Portsjonid: 1 portsjon"
            : $"Portsjonid: {recipe.Portions} portsjonit";
        DifficultyLabel.Text = $"Raskusaste: {GetTextOrDefault(recipe.Difficulty, "Puudub")}";
        FlagsLabel.Text = $"Omadused: {BuildFlagsText(recipe)}";
        InstructionsLabel.Text = GetTextOrDefault(recipe.Instructions, "Juhend puudub");

        IngredientsLayout.Children.Clear();

        foreach (var ingredient in recipe.Ingredients.Where(text => !string.IsNullOrWhiteSpace(text)))
        {
            IngredientsLayout.Children.Add(new Label
            {
                Text = $"• {ingredient}",
                TextColor = Color.FromArgb("#431407")
            });
        }

        if (IngredientsLayout.Children.Count == 0)
        {
            IngredientsLayout.Children.Add(new Label
            {
                Text = "Koostisosad puuduvad",
                TextColor = Color.FromArgb("#78716C")
            });
        }
    }

    private static string GetTextOrDefault(string? text, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(text) ? defaultValue : text.Trim();
    }

    private static string BuildFlagsText(RecipeData recipe)
    {
        var flags = new List<string>();

        if (recipe.IsVegetarian)
        {
            flags.Add("taimetoit");
        }

        if (recipe.IsSweet)
        {
            flags.Add("magus");
        }

        return flags.Count == 0 ? "tavaline" : string.Join(", ", flags);
    }
}

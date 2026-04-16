using System.Collections.ObjectModel;

namespace Tund2;

public partial class RecipeBookPage : ContentPage
{
    private readonly List<Entry> ingredientEntries = new();
    private readonly ObservableCollection<RecipeData> recipes;

    public RecipeBookPage()
        : this(((App)Application.Current!).Recipes)
    {
    }

    public RecipeBookPage(ObservableCollection<RecipeData> recipes)
    {
        InitializeComponent();
        this.recipes = recipes;
        SetDefaultValues();
    }

    private void SetDefaultValues()
    {
        CookingDatePicker.Date = DateTime.Today;
        DifficultyPicker.SelectedIndex = 1;

        UpdateCookingTimeLabel(CookingTimeSlider.Value);
        UpdatePortionsLabel(PortionsStepper.Value);
        ResetIngredients();
    }

    private void OnCookingTimeChanged(object? sender, ValueChangedEventArgs e)
    {
        UpdateCookingTimeLabel(e.NewValue);
    }

    private void OnPortionsChanged(object? sender, ValueChangedEventArgs e)
    {
        UpdatePortionsLabel(e.NewValue);
    }

    private void OnAddIngredientClicked(object? sender, EventArgs e)
    {
        AddIngredientRow(canDelete: true);
    }

    private void UpdateCookingTimeLabel(double value)
    {
        CookingTimeValueLabel.Text = $"{Math.Round(value)} min";
    }

    private void UpdatePortionsLabel(double value)
    {
        var portions = (int)Math.Round(value);
        PortionsValueLabel.Text = portions == 1 ? "1 portsjon" : $"{portions} portsjonit";
    }

    private void ResetIngredients()
    {
        IngredientsContainer.Children.Clear();
        ingredientEntries.Clear();

        AddIngredientRow(canDelete: false);
        AddIngredientRow(canDelete: false);
    }

    private void AddIngredientRow(string text = "", bool canDelete = false)
    {
        var entry = new Entry
        {
            Placeholder = "Sisesta koostisosa",
            Text = text,
            BackgroundColor = Colors.White,
            TextColor = Color.FromArgb("#431407")
        };

        var deleteButton = new Button
        {
            Text = "X",
            WidthRequest = 44,
            CornerRadius = 10,
            BackgroundColor = Color.FromArgb("#DC2626"),
            TextColor = Colors.White,
            IsVisible = canDelete,
            BindingContext = entry
        };

        deleteButton.Clicked += OnDeleteIngredientClicked;

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };

        grid.Add(entry, 0, 0);
        grid.Add(deleteButton, 1, 0);

        ingredientEntries.Add(entry);
        IngredientsContainer.Children.Add(grid);
    }

    private void OnDeleteIngredientClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is not Entry entry)
        {
            return;
        }

        var rowToRemove = IngredientsContainer.Children
            .OfType<Grid>()
            .FirstOrDefault(grid => grid.Children.Contains(entry));

        if (rowToRemove is not null)
        {
            IngredientsContainer.Children.Remove(rowToRemove);
            ingredientEntries.Remove(entry);
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var recipe = new RecipeData
        {
            Id = Guid.NewGuid(),
            Name = GetRecipeName(),
            DishType = DishTypeEntryCell.Text?.Trim() ?? string.Empty,
            Description = DescriptionEntryCell.Text?.Trim() ?? string.Empty,
            Author = AuthorEntryCell.Text?.Trim() ?? string.Empty,
            Ingredients = ingredientEntries
                .Select(entry => entry.Text?.Trim() ?? string.Empty)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList(),
            CookingDate = CookingDatePicker.Date ?? DateTime.Today,
            CookingTimeMinutes = (int)Math.Round(CookingTimeSlider.Value),
            Portions = (int)Math.Round(PortionsStepper.Value),
            Difficulty = DifficultyPicker.SelectedIndex >= 0
                ? DifficultyPicker.Items[DifficultyPicker.SelectedIndex]
                : string.Empty,
            IsVegetarian = VegetarianSwitchCell.IsToggled,
            IsSweet = SweetDishSwitchCell.IsToggled,
            Instructions = InstructionEditor.Text?.Trim() ?? string.Empty
        };

        recipes.Insert(0, recipe);

        await DisplayAlertAsync(
            "Retsept salvestatud",
            $"Retsept \"{recipe.Name}\" on salvestatud.",
            "OK");

        await Navigation.PopAsync();
    }

    private async void OnClearClicked(object? sender, EventArgs e)
    {
        RecipeNameEntryCell.Text = string.Empty;
        DishTypeEntryCell.Text = string.Empty;
        DescriptionEntryCell.Text = string.Empty;
        AuthorEntryCell.Text = string.Empty;

        CookingDatePicker.Date = DateTime.Today;
        CookingTimeSlider.Value = 45;
        PortionsStepper.Value = 2;
        DifficultyPicker.SelectedIndex = 1;
        VegetarianSwitchCell.IsToggled = false;
        SweetDishSwitchCell.IsToggled = false;

        InstructionEditor.Text = string.Empty;
        ResetIngredients();

        await DisplayAlertAsync(
            "Vorm puhastatud",
            "Kõik retsepti väljad on tühjendatud.",
            "OK");
    }

    private string GetRecipeName()
    {
        return string.IsNullOrWhiteSpace(RecipeNameEntryCell.Text)
            ? "Minu retsept"
            : RecipeNameEntryCell.Text.Trim();
    }
}

using System.Collections.ObjectModel;

namespace Tund2;

public partial class RecipeBookMenuPage : ContentPage
{
    private readonly ObservableCollection<RecipeData> recipes;

    public RecipeBookMenuPage()
        : this(((App)Application.Current!).Recipes)
    {
    }

    private RecipeBookMenuPage(ObservableCollection<RecipeData> recipes)
    {
        InitializeComponent();
        this.recipes = recipes;
        RecipesCollectionView.ItemsSource = this.recipes;
    }

    private async void OnCreateClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RecipeBookPage(recipes));
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        if (GetRecipeFromSender(sender) is RecipeData recipe)
        {
            await Navigation.PushAsync(new RecipeViewPage(recipe));
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (GetRecipeFromSender(sender) is not RecipeData recipe)
        {
            return;
        }

        var shouldDelete = await DisplayAlertAsync(
            "Kustuta retsept",
            $"Kas soovid retsepti \"{recipe.Name}\" kustutada?",
            "Jah",
            "Ei");

        if (shouldDelete)
        {
            recipes.Remove(recipe);
        }
    }

    private RecipeData? GetRecipeFromSender(object? sender)
    {
        return (sender as Button)?.BindingContext as RecipeData;
    }
}

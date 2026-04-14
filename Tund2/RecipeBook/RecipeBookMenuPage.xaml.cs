namespace Tund2;

public partial class RecipeBookMenuPage : ContentPage
{
    public RecipeBookMenuPage()
    {
        InitializeComponent();
        RecipesCollectionView.ItemsSource = RecipeStore.Recipes;
    }

    private async void OnCreateClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RecipeBookPage());
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        if (GetRecipeFromSender(sender) is RecipeData recipe)
        {
            await Navigation.PushAsync(new RecipeViewPage(recipe.Id));
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
            RecipeStore.Delete(recipe.Id);
        }
    }

    private RecipeData? GetRecipeFromSender(object? sender)
    {
        return (sender as Button)?.BindingContext as RecipeData;
    }
}

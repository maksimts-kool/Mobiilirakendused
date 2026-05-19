using Tund2.CityExplorer.ViewModels;

namespace Tund2.CityExplorer.Views;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesViewModel viewModel;

    public FavoritesPage(FavoritesViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await viewModel.LoadFavoritesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(viewModel.Localizer["DatabaseError"], ex.Message, "OK");
        }
    }
}

using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.ViewModels;

namespace Tund2.CityExplorer.Views;

public partial class ExplorePage : ContentPage
{
    private readonly ExploreViewModel viewModel;

    public ExplorePage(ExploreViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await RefreshFavoriteStatesAsync();
    }

    public async Task RefreshFavoriteStatesAsync()
    {
        try
        {
            await viewModel.RefreshFavoriteStatesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(viewModel.Localizer["DatabaseError"], ex.Message, "OK");
        }
    }

    private async void OnPlaceTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Place place)
        {
            return;
        }

        await Navigation.PushAsync(new TourPage(place, viewModel));
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        if (sender is not ImageButton { CommandParameter: Place place } button)
        {
            return;
        }

        try
        {
            await viewModel.ToggleFavoriteAsync(place);
            await FavoriteIconAnimator.PopAsync(button);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(viewModel.Localizer["DatabaseError"], ex.Message, "OK");
        }
    }
}

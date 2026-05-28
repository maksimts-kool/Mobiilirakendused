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
            var isFavorite = await viewModel.ToggleFavoriteAsync(place);
            await AnimateFavoriteIconAsync(button, isFavorite);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(viewModel.Localizer["DatabaseError"], ex.Message, "OK");
        }
    }

    private static async Task AnimateFavoriteIconAsync(ImageButton button, bool isLiked)
    {
        await Task.WhenAll(
            button.ScaleToAsync(0.72, 90, Easing.CubicOut),
            button.RotateToAsync(-10, 90, Easing.CubicOut),
            button.FadeToAsync(0.65, 90, Easing.CubicOut));

        await Task.WhenAll(
            button.ScaleToAsync(1.12, 150, Easing.SpringOut),
            button.RotateToAsync(8, 150, Easing.CubicOut),
            button.FadeToAsync(1, 150, Easing.CubicOut));

        await Task.WhenAll(
            button.ScaleToAsync(1, 90, Easing.CubicOut),
            button.RotateToAsync(0, 90, Easing.CubicOut));
    }
}

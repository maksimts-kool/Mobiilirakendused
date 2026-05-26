using Microsoft.Maui.Graphics;
using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.Services;
using Tund2.CityExplorer.ViewModels;

namespace Tund2.CityExplorer.Views;

public partial class TourPage : ContentPage
{
    private readonly ExploreViewModel exploreViewModel;

    public TourPage(Place place, ExploreViewModel exploreViewModel)
    {
        Place = place;
        this.exploreViewModel = exploreViewModel;
        Localizer = LocalizationManager.Instance;

        InitializeComponent();
        BindingContext = this;

        Localizer.CultureChanged += OnCultureChanged;
    }

    public Place Place { get; }

    public LocalizationManager Localizer { get; }

    public string CategoryTitle => Place.CategoryKey switch
    {
        "history" => Localizer["CategoryHistory"],
        "parks" => Localizer["CategoryParks"],
        "food" => Localizer["CategoryFood"],
        _ => Localizer["PlacesLabel"]
    };

    public string CategoryIcon => Place.CategoryKey switch
    {
        "history" => "★",
        "parks" => "♣",
        "food" => "◆",
        _ => "★"
    };

    public Color CategoryColor => Place.CategoryKey switch
    {
        "history" => Color.FromArgb("#F1D38B"),
        "parks" => Color.FromArgb("#8BE3C3"),
        "food" => Color.FromArgb("#FFC78E"),
        _ => Color.FromArgb("#8BE3C3")
    };

    public string BuyButtonText => Localizer["BuyTour"];

    protected override void OnDisappearing()
    {
        Localizer.CultureChanged -= OnCultureChanged;
        base.OnDisappearing();
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        Place.RefreshLanguage();
        OnPropertyChanged(nameof(CategoryTitle));
        OnPropertyChanged(nameof(BuyButtonText));
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        try
        {
            var isFavorite = await exploreViewModel.ToggleFavoriteAsync(Place);
            if (sender is ImageButton button)
            {
                await AnimateFavoriteIconAsync(button, isFavorite);
            }

            var title = isFavorite ? Localizer["AddedFavoriteTitle"] : Localizer["RemovedFavoriteTitle"];
            var message = isFavorite ? Localizer["AddedFavoriteMessage"] : Localizer["RemovedFavoriteMessage"];

            await DisplayAlertAsync(title, message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Localizer["DatabaseError"], ex.Message, "OK");
        }
    }

    private async void OnReadMoreTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync(Place.Name, Place.Detail, Localizer["Close"]);
    }

    private async void OnBuyClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync(Localizer["TourBookedTitle"], Localizer["TourBookedMessage"], "OK");
    }

    private static async Task AnimateFavoriteIconAsync(ImageButton button, bool isLiked)
    {
        await Task.WhenAll(
            button.ScaleToAsync(0.72, 90, Easing.CubicOut),
            button.RotateToAsync(-10, 90, Easing.CubicOut),
            button.FadeToAsync(0.65, 90, Easing.CubicOut));

        button.Source = isLiked ? "liked.png" : "unliked.png";

        await Task.WhenAll(
            button.ScaleToAsync(1.12, 150, Easing.SpringOut),
            button.RotateToAsync(8, 150, Easing.CubicOut),
            button.FadeToAsync(1, 150, Easing.CubicOut));

        await Task.WhenAll(
            button.ScaleToAsync(1, 90, Easing.CubicOut),
            button.RotateToAsync(0, 90, Easing.CubicOut));
    }
}

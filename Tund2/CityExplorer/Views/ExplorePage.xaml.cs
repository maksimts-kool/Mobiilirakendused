using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.ViewModels;

namespace Tund2.CityExplorer.Views;

public partial class ExplorePage : ContentPage
{
    private readonly ExploreViewModel viewModel;
    private readonly IDispatcherTimer autoScrollTimer;

    public ExplorePage(ExploreViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        BindingContext = viewModel;

        autoScrollTimer = Dispatcher.CreateTimer();
        autoScrollTimer.Interval = TimeSpan.FromSeconds(4);
        autoScrollTimer.Tick += (_, _) =>
        {
            if (viewModel.Places.Count <= 1 || PlaceCarousel.IsDragging)
            {
                return;
            }

            var nextPosition = (PlaceCarousel.Position + 1) % viewModel.Places.Count;
            PlaceCarousel.ScrollTo(nextPosition, position: ScrollToPosition.Center, animate: true);
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        autoScrollTimer.Start();
    }

    protected override void OnDisappearing()
    {
        autoScrollTimer.Stop();
        base.OnDisappearing();
    }

    private async void OnPlaceTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Place place)
        {
            return;
        }

        var addFavorite = await DisplayAlertAsync(
            place.Name,
            place.Detail,
            viewModel.Localizer["AddFavorite"],
            viewModel.Localizer["Close"]);

        if (!addFavorite)
        {
            return;
        }

        try
        {
            var wasAdded = await viewModel.AddFavoriteAsync(place);
            var title = wasAdded ? viewModel.Localizer["AddedFavoriteTitle"] : viewModel.Localizer["AlreadyFavoriteTitle"];
            var message = wasAdded ? viewModel.Localizer["AddedFavoriteMessage"] : viewModel.Localizer["AlreadyFavoriteMessage"];

            await DisplayAlertAsync(title, message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(viewModel.Localizer["DatabaseError"], ex.Message, "OK");
        }
    }
}

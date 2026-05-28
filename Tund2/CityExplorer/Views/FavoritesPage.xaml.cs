using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.ViewModels;

namespace Tund2.CityExplorer.Views;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesViewModel viewModel;
    private readonly ExploreViewModel exploreViewModel;

    public FavoritesPage(FavoritesViewModel viewModel, ExploreViewModel exploreViewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        this.exploreViewModel = exploreViewModel;
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

    private async void OnGroupHeaderTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border header ||
            header.BindingContext is not FavoriteCategoryGroup group ||
            header.Parent is not VerticalStackLayout groupLayout)
        {
            return;
        }

        var contentPanel = groupLayout.Children.OfType<Border>().Skip(1).FirstOrDefault();
        if (contentPanel is null)
        {
            group.IsExpanded = !group.IsExpanded;
            return;
        }

        header.InputTransparent = true;

        try
        {
            if (group.IsExpanded)
            {
                await CollapseGroupAsync(group, contentPanel);
                return;
            }

            await ExpandGroupAsync(group, contentPanel);
        }
        finally
        {
            header.InputTransparent = false;
        }
    }

    private async void OnFavoriteBadgeTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border badge ||
            badge.BindingContext is not Place place)
        {
            return;
        }

        badge.InputTransparent = true;

        try
        {
            await AnimateFavoriteBadgeAsync(badge);

            if (viewModel.RemoveFavoriteCommand.CanExecute(place))
            {
                viewModel.RemoveFavoriteCommand.Execute(place);
            }
        }
        finally
        {
            badge.InputTransparent = false;
        }
    }

    private async void OnFavoritePlaceTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject { BindingContext: Place place })
        {
            return;
        }

        await Navigation.PushAsync(new TourPage(place, exploreViewModel));
    }

    private static async Task ExpandGroupAsync(FavoriteCategoryGroup group, Border contentPanel)
    {
        contentPanel.AbortAnimation("SlideHeight");
        group.IsExpanded = true;

        contentPanel.IsVisible = true;
        contentPanel.HeightRequest = -1;
        contentPanel.Opacity = 0;
        contentPanel.TranslationY = -12;

        var targetHeight = MeasurePanelHeight(contentPanel);

        contentPanel.HeightRequest = 0;

        await Task.WhenAll(
            AnimateHeightAsync(contentPanel, 0, targetHeight),
            contentPanel.FadeToAsync(1, 220, Easing.CubicOut),
            contentPanel.TranslateToAsync(0, 0, 220, Easing.CubicOut));

        contentPanel.HeightRequest = -1;
    }

    private static async Task CollapseGroupAsync(FavoriteCategoryGroup group, Border contentPanel)
    {
        contentPanel.AbortAnimation("SlideHeight");

        var startHeight = contentPanel.Height > 0
            ? contentPanel.Height
            : MeasurePanelHeight(contentPanel);

        contentPanel.HeightRequest = startHeight;

        await Task.WhenAll(
            AnimateHeightAsync(contentPanel, startHeight, 0),
            contentPanel.FadeToAsync(0, 180, Easing.CubicIn),
            contentPanel.TranslateToAsync(0, -12, 180, Easing.CubicIn));

        group.IsExpanded = false;
        contentPanel.HeightRequest = 0;
        contentPanel.IsVisible = false;
        contentPanel.Opacity = 0;
        contentPanel.TranslationY = -12;
    }

    private static double MeasurePanelHeight(VisualElement panel)
    {
        var width = panel.Width > 0 ? panel.Width : 320;
        var measuredSize = panel.Measure(width, double.PositiveInfinity);
        return Math.Max(1, measuredSize.Height);
    }

    private static Task AnimateHeightAsync(VisualElement element, double start, double end)
    {
        var completion = new TaskCompletionSource();

        element.Animate(
            "SlideHeight",
            value => element.HeightRequest = value,
            start,
            end,
            16,
            240,
            Easing.CubicOut,
            (_, _) => completion.TrySetResult());

        return completion.Task;
    }

    private static async Task AnimateFavoriteBadgeAsync(Border badge)
    {
        var icon = badge.Content as Image;

        await Task.WhenAll(
            badge.ScaleToAsync(0.78, 90, Easing.CubicOut),
            badge.RotateToAsync(-9, 90, Easing.CubicOut),
            badge.FadeToAsync(0.72, 90, Easing.CubicOut));

        if (icon is not null)
        {
            icon.Source = "unliked.png";
        }

        await Task.WhenAll(
            badge.ScaleToAsync(1.12, 150, Easing.SpringOut),
            badge.RotateToAsync(8, 150, Easing.CubicOut),
            badge.FadeToAsync(1, 150, Easing.CubicOut));

        await Task.WhenAll(
            badge.ScaleToAsync(0.92, 90, Easing.CubicIn),
            badge.FadeToAsync(0, 90, Easing.CubicIn));

        badge.Scale = 1;
        badge.Rotation = 0;
        badge.Opacity = 1;
    }
}

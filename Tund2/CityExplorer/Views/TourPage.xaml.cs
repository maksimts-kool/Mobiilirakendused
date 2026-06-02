using Microsoft.Maui.Graphics;
using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.Services;
using Tund2.CityExplorer.ViewModels;

namespace Tund2.CityExplorer.Views;

public partial class TourPage : ContentPage
{
    private const uint DescriptionAnimationLength = 240;
    private const uint ToggleFadeLength = 80;

    private readonly ExploreViewModel exploreViewModel;
    private bool isDescriptionExpanded;
    private bool isReadMoreAnimating;

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

    public string CategoryTitle => CityExplorerCatalog.GetTourCategoryTitle(Place.CategoryKey, Localizer);

    public string CategoryIcon => CityExplorerCatalog.GetTourCategoryIcon(Place.CategoryKey);

    public Color CategoryColor => CityExplorerCatalog.GetTourCategoryColor(Place.CategoryKey);

    public string BuyButtonText => Localizer["BuyTour"];

    public string ReadMoreButtonText => IsDescriptionExpanded ? Localizer["ShowLess"] : Localizer["ReadMore"];

    public double ReadMoreArrowRotation => IsDescriptionExpanded ? 180 : 0;

    public bool IsDescriptionExpanded
    {
        get => isDescriptionExpanded;
        private set
        {
            if (isDescriptionExpanded == value)
            {
                return;
            }

            isDescriptionExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ReadMoreButtonText));
            OnPropertyChanged(nameof(ReadMoreArrowRotation));
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await exploreViewModel.RefreshFavoriteStatesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Localizer["DatabaseError"], ex.Message, "OK");
        }
    }

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
        OnPropertyChanged(nameof(ReadMoreButtonText));
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        try
        {
            await exploreViewModel.ToggleFavoriteAsync(Place);
            if (sender is ImageButton button)
            {
                await FavoriteIconAnimator.PopAsync(button);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Localizer["DatabaseError"], ex.Message, "OK");
        }
    }

    private async void OnReadMoreTapped(object? sender, TappedEventArgs e)
    {
        if (isReadMoreAnimating)
        {
            return;
        }

        isReadMoreAnimating = true;

        try
        {
            if (IsDescriptionExpanded)
            {
                await CollapseDescriptionAsync();
            }
            else
            {
                await ExpandDescriptionAsync();
            }
        }
        finally
        {
            isReadMoreAnimating = false;
        }
    }

    private async void OnBuyClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync(Localizer["TourBookedTitle"], Localizer["TourBookedMessage"], "OK");
    }

    private async Task ExpandDescriptionAsync()
    {
        await FadeReadMoreToggleAsync(0);
        IsDescriptionExpanded = true;

        ExpandedDescriptionClip.IsVisible = true;
        ExpandedDescriptionLabel.Opacity = 0;
        ExpandedDescriptionClip.HeightRequest = 0;

        var targetHeight = MeasureExpandedDescriptionHeight();

        await Task.WhenAll(
            AnimateExpandedDescriptionHeightAsync(0, targetHeight),
            ExpandedDescriptionLabel.FadeToAsync(1, DescriptionAnimationLength, Easing.CubicOut),
            FadeReadMoreToggleAsync(1),
            InfoCardsGrid.FadeToAsync(0, DescriptionAnimationLength, Easing.CubicOut));

        ExpandedDescriptionClip.HeightRequest = -1;
        InfoCardsGrid.IsVisible = false;
    }

    private async Task CollapseDescriptionAsync()
    {
        await FadeReadMoreToggleAsync(0);
        IsDescriptionExpanded = false;

        InfoCardsGrid.Opacity = 0;
        InfoCardsGrid.IsVisible = true;

        var startHeight = ExpandedDescriptionClip.Height > 0
            ? ExpandedDescriptionClip.Height
            : MeasureExpandedDescriptionHeight();

        await Task.WhenAll(
            AnimateExpandedDescriptionHeightAsync(startHeight, 0),
            ExpandedDescriptionLabel.FadeToAsync(0, DescriptionAnimationLength, Easing.CubicIn),
            FadeReadMoreToggleAsync(1),
            InfoCardsGrid.FadeToAsync(1, DescriptionAnimationLength, Easing.CubicOut));

        ExpandedDescriptionClip.HeightRequest = 0;
        ExpandedDescriptionClip.IsVisible = false;
    }

    private double MeasureExpandedDescriptionHeight()
    {
        var availableWidth = DescriptionSection.Width;
        if (availableWidth <= 0)
        {
            availableWidth = Width - DescriptionSection.Margin.Left - DescriptionSection.Margin.Right;
        }

        availableWidth = Math.Max(0, availableWidth);

        var request = ExpandedDescriptionLabel.Measure(availableWidth, double.PositiveInfinity);
        return request.Height;
    }

    private Task FadeReadMoreToggleAsync(double opacity)
    {
        return Task.WhenAll(
            ReadMoreLabel.FadeToAsync(opacity, ToggleFadeLength, Easing.CubicOut),
            ReadMoreArrow.FadeToAsync(opacity, ToggleFadeLength, Easing.CubicOut));
    }

    private Task AnimateExpandedDescriptionHeightAsync(double startHeight, double endHeight)
    {
        var completionSource = new TaskCompletionSource();
        var animation = new Animation(
            value => ExpandedDescriptionClip.HeightRequest = value,
            startHeight,
            endHeight,
            Easing.CubicInOut);

        animation.Commit(
            this,
            "ExpandedDescriptionHeight",
            length: DescriptionAnimationLength,
            finished: (_, _) => completionSource.SetResult());

        return completionSource.Task;
    }
}

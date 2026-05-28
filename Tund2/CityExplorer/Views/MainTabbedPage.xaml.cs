using Tund2.CityExplorer.Services;

#if IOS
using UIKit;
#endif

namespace Tund2.CityExplorer.Views;

public partial class MainTabbedPage : TabbedPage
{
    private readonly ExplorePage explorePage;
    private readonly FavoritesPage favoritesPage;
    private readonly SettingsPage settingsPage;
    private readonly LocalizationManager localizer = LocalizationManager.Instance;

    public MainTabbedPage(ExplorePage explorePage, FavoritesPage favoritesPage, SettingsPage settingsPage)
    {
        InitializeComponent();

        NavigationPage.SetHasNavigationBar(this, false);

        this.explorePage = explorePage;
        this.favoritesPage = favoritesPage;
        this.settingsPage = settingsPage;

        explorePage.IconImageSource = "cityexplorer_tab_grid.png";
        favoritesPage.IconImageSource = "cityexplorer_tab_heart.png";
        settingsPage.IconImageSource = "cityexplorer_tab_settings.png";

        UpdateTitles();

        Children.Add(explorePage);
        Children.Add(favoritesPage);
        Children.Add(settingsPage);

        CurrentPageChanged += OnCurrentPageChanged;
        localizer.CultureChanged += (_, _) => UpdateTitles();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        FixIosTabBarLayout();
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(120), FixIosTabBarLayout);
    }

    private async void OnCurrentPageChanged(object? sender, EventArgs e)
    {
        if (CurrentPage == explorePage)
        {
            await explorePage.RefreshFavoriteStatesAsync();
        }
    }

    private void UpdateTitles()
    {
        Title = localizer["AppTitle"];
        explorePage.Title = localizer["ExploreTitle"];
        favoritesPage.Title = localizer["FavoritesTabTitle"];
        settingsPage.Title = localizer["SettingsTitle"];
    }

#if IOS
    private void FixIosTabBarLayout()
    {
        var tabBarController = FindTabBarController(Handler?.PlatformView as UIViewController);
        if (tabBarController?.TabBar.Items is null)
        {
            return;
        }

        foreach (var item in tabBarController.TabBar.Items)
        {
            item.TitlePositionAdjustment = new UIOffset(0, -5);
            item.ImageInsets = new UIEdgeInsets(-2, 0, 2, 0);
        }

        tabBarController.TabBar.SetNeedsLayout();
        tabBarController.TabBar.LayoutIfNeeded();
    }

    private static UITabBarController? FindTabBarController(UIViewController? controller)
    {
        if (controller is null)
        {
            return null;
        }

        if (controller is UITabBarController tabBarController)
        {
            return tabBarController;
        }

        foreach (var child in controller.ChildViewControllers)
        {
            var result = FindTabBarController(child);
            if (result is not null)
            {
                return result;
            }
        }

        return FindTabBarController(controller.PresentedViewController);
    }
#else
    private void FixIosTabBarLayout()
    {
    }
#endif
}

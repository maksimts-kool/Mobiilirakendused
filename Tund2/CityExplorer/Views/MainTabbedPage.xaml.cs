using Tund2.CityExplorer.Services;

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

        Children.Add(explorePage);
        Children.Add(favoritesPage);
        Children.Add(settingsPage);

        localizer.CultureChanged += (_, _) => UpdateTitles();
        UpdateTitles();
    }

    private void UpdateTitles()
    {
        Title = localizer["AppTitle"];
        explorePage.Title = localizer["ExploreTitle"];
        favoritesPage.Title = localizer["FavoritesTitle"];
        settingsPage.Title = localizer["SettingsTitle"];
    }
}

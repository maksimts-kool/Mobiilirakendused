using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.ViewModels;

public class ExploreViewModel : BaseViewModel
{
    private readonly DatabaseService databaseService;
    private readonly List<Place> allPlaces;
    private Category? selectedCategory;
    private Place? selectedPlace;
    private string searchText = string.Empty;

    public ExploreViewModel(DatabaseService databaseService)
    {
        this.databaseService = databaseService;
        Localizer = LocalizationManager.Instance;

        Categories = new ObservableCollection<Category>
        {
            new() { Key = "history", Emoji = "⭐", Icon = "★", TitleKey = "CategoryHistory", AccentColor = Color.FromArgb("#F1D38B"), SoftColor = Color.FromArgb("#F7F7F7") },
            new() { Key = "parks", Emoji = "🌳", Icon = "♣", TitleKey = "CategoryParks", AccentColor = Color.FromArgb("#8BE3C3"), SoftColor = Color.FromArgb("#DFF8EF") },
            new() { Key = "food", Emoji = "🍽", Icon = "◆", TitleKey = "CategoryFood", AccentColor = Color.FromArgb("#FFC78E"), SoftColor = Color.FromArgb("#FFF3E6") }
        };

        allPlaces =
        [
            new() { Id = 1, CategoryKey = "history", Image = "cityexplorer_toompea.jpg", NameKey = "PlaceToompeaName", ShortDescriptionKey = "PlaceToompeaShort", DetailKey = "PlaceToompeaDetail", Rating = "4,9", PriceTextKey = "TourPriceHistory", DistanceTextKey = "TourDistanceCenter", TagTextKey = "TourTagHistory" },
            new() { Id = 2, CategoryKey = "history", Image = "cityexplorer_oldtown.jpg", NameKey = "PlaceOldTownName", ShortDescriptionKey = "PlaceOldTownShort", DetailKey = "PlaceOldTownDetail", Rating = "4,8", PriceTextKey = "TourPriceHistory", DistanceTextKey = "TourDistanceCenter", TagTextKey = "TourTagHistory" },
            new() { Id = 3, CategoryKey = "parks", Image = "cityexplorer_kadriorg.jpg", NameKey = "PlaceKadriorgName", ShortDescriptionKey = "PlaceKadriorgShort", DetailKey = "PlaceKadriorgDetail", Rating = "5,0", PriceTextKey = "TourPriceNature", DistanceTextKey = "TourDistanceQuiet", TagTextKey = "TourTagNature" },
            new() { Id = 4, CategoryKey = "parks", Image = "cityexplorer_pirita.jpg", NameKey = "PlacePiritaName", ShortDescriptionKey = "PlacePiritaShort", DetailKey = "PlacePiritaDetail", Rating = "4,7", PriceTextKey = "TourPriceNature", DistanceTextKey = "TourDistanceSea", TagTextKey = "TourTagNature" },
            new() { Id = 5, CategoryKey = "food", Image = "cityexplorer_market.jpg", NameKey = "PlaceMarketName", ShortDescriptionKey = "PlaceMarketShort", DetailKey = "PlaceMarketDetail", Rating = "4,9", PriceTextKey = "TourPriceFood", DistanceTextKey = "TourDistanceCenter", TagTextKey = "TourTagFood" },
            new() { Id = 6, CategoryKey = "food", Image = "cityexplorer_telliskivi.jpg", NameKey = "PlaceTelliskiviName", ShortDescriptionKey = "PlaceTelliskiviShort", DetailKey = "PlaceTelliskiviDetail", Rating = "4,8", PriceTextKey = "TourPriceFood", DistanceTextKey = "TourDistanceTram", TagTextKey = "TourTagFood" }
        ];

        Places = new ObservableCollection<Place>();
        ChangeCategoryCommand = new Command<Category>(category => SelectedCategory = category);
        AddFavoriteCommand = new Command<Place>(async place => await AddFavoriteFromCommandAsync(place));

        Localizer.CultureChanged += (_, _) =>
        {
            foreach (var category in Categories)
            {
                category.RefreshLanguage();
            }

            foreach (var place in allPlaces)
            {
                place.RefreshLanguage();
            }

            OnPropertyChanged(nameof(SelectedCategoryTitle));
        };

        SelectedCategory = Categories.First();
    }

    public LocalizationManager Localizer { get; }

    public ObservableCollection<Category> Categories { get; }

    public ObservableCollection<Place> Places { get; }

    public ICommand ChangeCategoryCommand { get; }

    public ICommand AddFavoriteCommand { get; }

    public Category? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (!SetProperty(ref selectedCategory, value) || selectedCategory is null)
            {
                return;
            }

            RefreshPlaces();
            OnPropertyChanged(nameof(SelectedCategoryTitle));
        }
    }

    public Place? SelectedPlace
    {
        get => selectedPlace;
        set => SetProperty(ref selectedPlace, value);
    }

    public string SearchText
    {
        get => searchText;
        set
        {
            if (!SetProperty(ref searchText, value))
            {
                return;
            }

            RefreshPlaces();
        }
    }

    public string SelectedCategoryTitle => SelectedCategory?.Title ?? string.Empty;

    public async Task<bool> AddFavoriteAsync(Place place)
    {
        if (await databaseService.FavoriteExistsAsync(place.Id))
        {
            place.IsFavorite = true;
            return false;
        }

        await databaseService.SaveFavoriteAsync(place);
        place.IsFavorite = true;
        return true;
    }

    public async Task<bool> ToggleFavoriteAsync(Place place)
    {
        if (await databaseService.FavoriteExistsAsync(place.Id))
        {
            await databaseService.DeleteFavoriteAsync(place.Id);
            place.IsFavorite = false;
            return false;
        }

        await databaseService.SaveFavoriteAsync(place);
        place.IsFavorite = true;
        return true;
    }

    public async Task RefreshFavoriteStatesAsync()
    {
        foreach (var place in allPlaces)
        {
            place.IsFavorite = await databaseService.FavoriteExistsAsync(place.Id);
        }
    }

    private void RefreshPlaces()
    {
        Places.Clear();

        if (selectedCategory is null)
        {
            SelectedPlace = null;
            OnPropertyChanged(nameof(HasPlaces));
            return;
        }

        var normalizedSearch = searchText.Trim();
        var filteredPlaces = allPlaces
            .Where(place => place.CategoryKey == selectedCategory.Key)
            .Where(place => string.IsNullOrWhiteSpace(normalizedSearch)
                || place.Name.Contains(normalizedSearch, StringComparison.CurrentCultureIgnoreCase)
                || place.ShortDescription.Contains(normalizedSearch, StringComparison.CurrentCultureIgnoreCase));

        foreach (var place in filteredPlaces)
        {
            Places.Add(place);
        }

        SelectedPlace = Places.FirstOrDefault();
        OnPropertyChanged(nameof(HasPlaces));
    }

    private async Task AddFavoriteFromCommandAsync(Place? place)
    {
        if (place is null)
        {
            return;
        }

        await AddFavoriteAsync(place);
    }

    public bool HasPlaces => Places.Count > 0;
}

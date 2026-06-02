using System.Collections.ObjectModel;
using System.Windows.Input;
using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.ViewModels;

public class ExploreViewModel : BaseViewModel
{
    private readonly DatabaseService databaseService;
    private readonly IReadOnlyList<Place> allPlaces;
    private Category? selectedCategory;
    private Place? selectedPlace;
    private string searchText = string.Empty;

    public ExploreViewModel(DatabaseService databaseService)
    {
        this.databaseService = databaseService;
        Localizer = LocalizationManager.Instance;

        Categories = new ObservableCollection<Category>(CityExplorerCatalog.CreateCategories());
        allPlaces = CityExplorerCatalog.CreatePlaces();
        Places = new ObservableCollection<Place>();
        ChangeCategoryCommand = new Command<Category>(category => SelectedCategory = category);
        AddFavoriteCommand = new Command<Place>(async place => await AddFavoriteFromCommandAsync(place));

        Localizer.CultureChanged += OnCultureChanged;

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

    public bool HasPlaces => Places.Count > 0;

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
        var favoriteIds = await databaseService.GetFavoriteIdsAsync();

        foreach (var place in allPlaces)
        {
            place.IsFavorite = favoriteIds.Contains(place.Id);
        }
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Localizer));

        foreach (var category in Categories)
        {
            category.RefreshLanguage();
        }

        foreach (var place in allPlaces)
        {
            place.RefreshLanguage();
        }

        OnPropertyChanged(nameof(SelectedCategoryTitle));
    }

    private void RefreshPlaces()
    {
        Places.Clear();

        if (selectedCategory is null)
        {
            SelectedPlace = null;
            NotifyPlacesChanged();
            return;
        }

        foreach (var place in GetFilteredPlaces())
        {
            Places.Add(place);
        }

        SelectedPlace = Places.FirstOrDefault();
        NotifyPlacesChanged();
    }

    private IEnumerable<Place> GetFilteredPlaces()
    {
        var normalizedSearch = searchText.Trim();

        return allPlaces
            .Where(place => place.CategoryKey == selectedCategory?.Key)
            .Where(place => MatchesSearch(place, normalizedSearch));
    }

    private static bool MatchesSearch(Place place, string normalizedSearch)
    {
        return string.IsNullOrWhiteSpace(normalizedSearch)
            || place.Name.Contains(normalizedSearch, StringComparison.CurrentCultureIgnoreCase)
            || place.ShortDescription.Contains(normalizedSearch, StringComparison.CurrentCultureIgnoreCase);
    }

    private void NotifyPlacesChanged()
    {
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
}

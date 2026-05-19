using System.Collections.ObjectModel;
using System.Windows.Input;
using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.ViewModels;

public class ExploreViewModel : BaseViewModel
{
    private readonly DatabaseService databaseService;
    private readonly List<Place> allPlaces;
    private Category? selectedCategory;
    private Place? selectedPlace;

    public ExploreViewModel(DatabaseService databaseService)
    {
        this.databaseService = databaseService;
        Localizer = LocalizationManager.Instance;

        Categories = new ObservableCollection<Category>
        {
            new() { Key = "history", Emoji = "🏰", TitleKey = "CategoryHistory" },
            new() { Key = "parks", Emoji = "🌳", TitleKey = "CategoryParks" },
            new() { Key = "food", Emoji = "🍽️", TitleKey = "CategoryFood" }
        };

        allPlaces =
        [
            new() { Id = 1, CategoryKey = "history", Image = "cityexplorer_toompea.png", NameKey = "PlaceToompeaName", ShortDescriptionKey = "PlaceToompeaShort", DetailKey = "PlaceToompeaDetail" },
            new() { Id = 2, CategoryKey = "history", Image = "cityexplorer_oldtown.png", NameKey = "PlaceOldTownName", ShortDescriptionKey = "PlaceOldTownShort", DetailKey = "PlaceOldTownDetail" },
            new() { Id = 3, CategoryKey = "parks", Image = "cityexplorer_kadriorg.png", NameKey = "PlaceKadriorgName", ShortDescriptionKey = "PlaceKadriorgShort", DetailKey = "PlaceKadriorgDetail" },
            new() { Id = 4, CategoryKey = "parks", Image = "cityexplorer_pirita.png", NameKey = "PlacePiritaName", ShortDescriptionKey = "PlacePiritaShort", DetailKey = "PlacePiritaDetail" },
            new() { Id = 5, CategoryKey = "food", Image = "cityexplorer_market.png", NameKey = "PlaceMarketName", ShortDescriptionKey = "PlaceMarketShort", DetailKey = "PlaceMarketDetail" },
            new() { Id = 6, CategoryKey = "food", Image = "cityexplorer_telliskivi.png", NameKey = "PlaceTelliskiviName", ShortDescriptionKey = "PlaceTelliskiviShort", DetailKey = "PlaceTelliskiviDetail" }
        ];

        Places = new ObservableCollection<Place>();
        ChangeCategoryCommand = new Command<Category>(category => SelectedCategory = category);

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

    public Category? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (!SetProperty(ref selectedCategory, value) || selectedCategory is null)
            {
                return;
            }

            Places.Clear();
            foreach (var place in allPlaces.Where(place => place.CategoryKey == selectedCategory.Key))
            {
                Places.Add(place);
            }

            SelectedPlace = Places.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedCategoryTitle));
        }
    }

    public Place? SelectedPlace
    {
        get => selectedPlace;
        set => SetProperty(ref selectedPlace, value);
    }

    public string SelectedCategoryTitle => SelectedCategory?.Title ?? string.Empty;

    public async Task<bool> AddFavoriteAsync(Place place)
    {
        if (await databaseService.FavoriteExistsAsync(place.Id))
        {
            return false;
        }

        await databaseService.SaveFavoriteAsync(place);
        return true;
    }
}

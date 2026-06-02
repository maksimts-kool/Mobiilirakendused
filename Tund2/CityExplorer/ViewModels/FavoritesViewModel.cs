using System.Collections.ObjectModel;
using System.Windows.Input;
using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.ViewModels;

public class FavoritesViewModel : BaseViewModel
{
    private readonly DatabaseService databaseService;
    private bool isBusy;

    public FavoritesViewModel(DatabaseService databaseService)
    {
        this.databaseService = databaseService;
        Localizer = LocalizationManager.Instance;
        Favorites = new ObservableCollection<Place>();
        FavoriteGroups = new ObservableCollection<FavoriteCategoryGroup>();
        RemoveFavoriteCommand = new Command<Place>(async place => await RemoveFavoriteAsync(place));

        Localizer.CultureChanged += OnCultureChanged;
    }

    public LocalizationManager Localizer { get; }

    public ObservableCollection<Place> Favorites { get; }

    public ObservableCollection<FavoriteCategoryGroup> FavoriteGroups { get; }

    public ICommand RemoveFavoriteCommand { get; }

    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    public bool HasFavorites => Favorites.Count > 0;

    public bool HasNoFavorites => Favorites.Count == 0;

    public async Task LoadFavoritesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var expandedStates = FavoriteGroups.ToDictionary(group => group.CategoryKey, group => group.IsExpanded);
            var savedPlaces = await databaseService.GetFavoritesAsync();

            Favorites.Clear();
            foreach (var place in savedPlaces)
            {
                Favorites.Add(place);
            }

            RebuildFavoriteGroups(savedPlaces, expandedStates);
        }
        finally
        {
            IsBusy = false;
            NotifyFavoriteStateChanged();
        }
    }

    private async Task RemoveFavoriteAsync(Place? place)
    {
        if (place is null)
        {
            return;
        }

        await databaseService.DeleteFavoriteAsync(place.Id);
        Favorites.Remove(place);

        var group = FavoriteGroups.FirstOrDefault(item => item.CategoryKey == place.CategoryKey);
        if (group is not null)
        {
            group.Places.Remove(place);

            if (group.Places.Count == 0)
            {
                FavoriteGroups.Remove(group);
            }
        }

        NotifyFavoriteStateChanged();
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Localizer));

        foreach (var group in FavoriteGroups)
        {
            group.RefreshLanguage();
        }
    }

    private void RebuildFavoriteGroups(
        IEnumerable<Place> savedPlaces,
        IReadOnlyDictionary<string, bool> expandedStates)
    {
        FavoriteGroups.Clear();

        foreach (var group in savedPlaces.GroupBy(place => place.CategoryKey))
        {
            var favoriteGroup = CityExplorerCatalog.CreateFavoriteGroup(group.Key);
            favoriteGroup.IsExpanded = expandedStates.TryGetValue(group.Key, out var wasExpanded)
                ? wasExpanded
                : FavoriteGroups.Count == 0;

            foreach (var place in group)
            {
                favoriteGroup.Places.Add(place);
            }

            FavoriteGroups.Add(favoriteGroup);
        }
    }

    private void NotifyFavoriteStateChanged()
    {
        OnPropertiesChanged(
            nameof(HasFavorites),
            nameof(HasNoFavorites));
    }
}

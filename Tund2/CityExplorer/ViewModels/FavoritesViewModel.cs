using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Tund2.CityExplorer.Models;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.ViewModels;

public class FavoritesViewModel : BaseViewModel
{
    private readonly DatabaseService databaseService;
    private readonly CategoryDefinition[] categoryDefinitions =
    [
        new("history", "CategoryHistory", "★", Color.FromArgb("#F1D38B"), Color.FromArgb("#FFF5DF"), Color.FromArgb("#E4B752")),
        new("parks", "CategoryParks", "♣", Color.FromArgb("#8BE3C3"), Color.FromArgb("#E7FAF2"), Color.FromArgb("#69D8B1")),
        new("food", "CategoryFood", "◆", Color.FromArgb("#FFC78E"), Color.FromArgb("#FFF0E0"), Color.FromArgb("#F2A85E"))
    ];

    private bool isBusy;

    public FavoritesViewModel(DatabaseService databaseService)
    {
        this.databaseService = databaseService;
        Localizer = LocalizationManager.Instance;
        Favorites = new ObservableCollection<Place>();
        FavoriteGroups = new ObservableCollection<FavoriteCategoryGroup>();
        RemoveFavoriteCommand = new Command<Place>(async place => await RemoveFavoriteAsync(place));

        Localizer.CultureChanged += (_, _) =>
        {
            foreach (var place in Favorites)
            {
                place.RefreshLanguage();
            }

            foreach (var group in FavoriteGroups)
            {
                group.RefreshLanguage();
            }
        };
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

            Favorites.Clear();
            FavoriteGroups.Clear();
            var savedPlaces = await databaseService.GetFavoritesAsync();

            foreach (var place in savedPlaces)
            {
                Favorites.Add(place);
            }

            foreach (var group in savedPlaces.GroupBy(place => place.CategoryKey))
            {
                var definition = categoryDefinitions.FirstOrDefault(item => item.Key == group.Key)
                    ?? categoryDefinitions[0];
                var favoriteGroup = new FavoriteCategoryGroup
                {
                    CategoryKey = group.Key,
                    TitleKey = definition.TitleKey,
                    Icon = definition.Icon,
                    HeaderColor = definition.HeaderColor,
                    BodyColor = definition.BodyColor,
                    ItemBorderColor = definition.ItemBorderColor,
                    IsExpanded = FavoriteGroups.Count == 0
                };

                foreach (var place in group)
                {
                    favoriteGroup.Places.Add(place);
                }

                FavoriteGroups.Add(favoriteGroup);
            }
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasFavorites));
            OnPropertyChanged(nameof(HasNoFavorites));
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

        OnPropertyChanged(nameof(HasFavorites));
        OnPropertyChanged(nameof(HasNoFavorites));
    }

    private sealed record CategoryDefinition(
        string Key,
        string TitleKey,
        string Icon,
        Color HeaderColor,
        Color BodyColor,
        Color ItemBorderColor);
}

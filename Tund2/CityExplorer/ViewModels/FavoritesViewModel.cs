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
        RemoveFavoriteCommand = new Command<Place>(async place => await RemoveFavoriteAsync(place));

        Localizer.CultureChanged += (_, _) =>
        {
            foreach (var place in Favorites)
            {
                place.RefreshLanguage();
            }
        };
    }

    public LocalizationManager Localizer { get; }

    public ObservableCollection<Place> Favorites { get; }

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
            var savedPlaces = await databaseService.GetFavoritesAsync();

            foreach (var place in savedPlaces)
            {
                Favorites.Add(place);
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
        OnPropertyChanged(nameof(HasFavorites));
        OnPropertyChanged(nameof(HasNoFavorites));
    }
}

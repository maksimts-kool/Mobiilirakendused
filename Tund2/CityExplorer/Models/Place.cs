using Tund2.CityExplorer.Common;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.Models;

public class Place : ObservableObject
{
    private bool isFavorite;

    public int Id { get; set; }

    public string CategoryKey { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string NameKey { get; set; } = string.Empty;

    public string ShortDescriptionKey { get; set; } = string.Empty;

    public string DetailKey { get; set; } = string.Empty;

    public string Rating { get; set; } = "4,9";

    public string PriceTextKey { get; set; } = "TourPrice";

    public string DistanceTextKey { get; set; } = "TourDistance";

    public string TagTextKey { get; set; } = "TourTag";

    public string Name => LocalizationManager.Instance[NameKey];

    public string ShortDescription => LocalizationManager.Instance[ShortDescriptionKey];

    public string Detail => LocalizationManager.Instance[DetailKey];

    public string PriceText => LocalizationManager.Instance[PriceTextKey];

    public string DistanceText => LocalizationManager.Instance[DistanceTextKey];

    public string TagText => LocalizationManager.Instance[TagTextKey];

    public bool IsFavorite
    {
        get => isFavorite;
        set
        {
            if (!SetProperty(ref isFavorite, value))
            {
                return;
            }

            OnPropertyChanged(nameof(FavoriteIcon));
        }
    }

    public string FavoriteIcon => IsFavorite ? "liked.png" : "unliked.png";

    public void RefreshLanguage()
    {
        OnPropertiesChanged(
            nameof(Name),
            nameof(ShortDescription),
            nameof(Detail),
            nameof(PriceText),
            nameof(DistanceText),
            nameof(TagText));
    }
}

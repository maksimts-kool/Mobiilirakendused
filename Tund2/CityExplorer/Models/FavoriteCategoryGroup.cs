using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;
using Tund2.CityExplorer.Common;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.Models;

public class FavoriteCategoryGroup : ObservableObject
{
    private bool isExpanded = true;

    public string CategoryKey { get; set; } = string.Empty;

    public string TitleKey { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public Color HeaderColor { get; set; } = Color.FromArgb("#F1D38B");

    public Color BodyColor { get; set; } = Color.FromArgb("#FFF5DF");

    public Color ItemBorderColor { get; set; } = Color.FromArgb("#E4B752");

    public ObservableCollection<Place> Places { get; } = new();

    public string Title => LocalizationManager.Instance[TitleKey];

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (!SetProperty(ref isExpanded, value))
            {
                return;
            }

            OnPropertiesChanged(
                nameof(ArrowRotation),
                nameof(HeaderCornerRadius));
        }
    }

    public double ArrowRotation => IsExpanded ? 180 : 0;

    public CornerRadius HeaderCornerRadius => IsExpanded
        ? new CornerRadius(16, 16, 0, 0)
        : new CornerRadius(16);

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(Title));

        foreach (var place in Places)
        {
            place.RefreshLanguage();
        }
    }
}

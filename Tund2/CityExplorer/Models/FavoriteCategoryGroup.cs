using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.Models;

public class FavoriteCategoryGroup : INotifyPropertyChanged
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
            if (isExpanded == value)
            {
                return;
            }

            isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ArrowRotation));
            OnPropertyChanged(nameof(HeaderCornerRadius));
        }
    }

    public double ArrowRotation => IsExpanded ? 180 : 0;

    public CornerRadius HeaderCornerRadius => IsExpanded
        ? new CornerRadius(16, 16, 0, 0)
        : new CornerRadius(16);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(Title));

        foreach (var place in Places)
        {
            place.RefreshLanguage();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

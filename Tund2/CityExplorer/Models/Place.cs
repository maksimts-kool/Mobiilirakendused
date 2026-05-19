using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.Models;

public class Place : INotifyPropertyChanged
{
    public int Id { get; set; }

    public string CategoryKey { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string NameKey { get; set; } = string.Empty;

    public string ShortDescriptionKey { get; set; } = string.Empty;

    public string DetailKey { get; set; } = string.Empty;

    public string Name => LocalizationManager.Instance[NameKey];

    public string ShortDescription => LocalizationManager.Instance[ShortDescriptionKey];

    public string Detail => LocalizationManager.Instance[DetailKey];

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(ShortDescription));
        OnPropertyChanged(nameof(Detail));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

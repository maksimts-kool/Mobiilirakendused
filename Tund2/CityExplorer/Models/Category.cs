using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.Models;

public class Category : INotifyPropertyChanged
{
    public string Key { get; set; } = string.Empty;

    public string Emoji { get; set; } = string.Empty;

    public string TitleKey { get; set; } = string.Empty;

    public string Title => LocalizationManager.Instance[TitleKey];

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(Title));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

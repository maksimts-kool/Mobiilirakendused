using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.Models;

public class Category : INotifyPropertyChanged
{
    public string Key { get; set; } = string.Empty;

    public string Emoji { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public Color AccentColor { get; set; } = Color.FromArgb("#8CE6C4");

    public Color SoftColor { get; set; } = Color.FromArgb("#F5F5F5");

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

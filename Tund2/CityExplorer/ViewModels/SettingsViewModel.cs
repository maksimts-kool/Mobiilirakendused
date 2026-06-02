using System.Windows.Input;
using Tund2.CityExplorer.Services;

namespace Tund2.CityExplorer.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    public SettingsViewModel()
    {
        Localizer = LocalizationManager.Instance;
        ChangeLanguageCommand = new Command<string>(languageCode => Localizer.SetCulture(languageCode));

        Localizer.CultureChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Localizer));
            OnPropertiesChanged(
                nameof(CurrentLanguageText),
                nameof(CurrentLanguageCode));
        };
    }

    public LocalizationManager Localizer { get; }

    public ICommand ChangeLanguageCommand { get; }

    public string CurrentLanguageCode => Localizer.CurrentLanguageCode.ToUpperInvariant();

    public string CurrentLanguageText => $"{Localizer["CurrentLanguage"]}: {CurrentLanguageCode}";
}

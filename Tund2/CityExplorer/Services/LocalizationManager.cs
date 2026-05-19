using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Tund2.CityExplorer.Services;

public sealed class LocalizationManager : INotifyPropertyChanged
{
    private const string LanguagePreferenceKey = "CityExplorerLanguage";

    private static readonly ResourceManager ResourceManager =
        new("Tund2.CityExplorer.Resources.Strings.AppResources", typeof(LocalizationManager).Assembly);

    private static readonly HashSet<string> SupportedLanguages =
        new(StringComparer.OrdinalIgnoreCase) { "et", "en", "ru" };

    private CultureInfo currentCulture = ResolveInitialCulture();

    public static LocalizationManager Instance { get; } = new();

    private LocalizationManager()
    {
        SetApplicationCulture(currentCulture, false);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? CultureChanged;

    public string CurrentLanguageCode => currentCulture.TwoLetterISOLanguageName;

    public string this[string key] => ResourceManager.GetString(key, currentCulture) ?? key;

    public void SetCulture(string languageCode)
    {
        var normalizedCode = string.IsNullOrWhiteSpace(languageCode)
            ? "et"
            : languageCode.Trim().ToLowerInvariant();

        if (!SupportedLanguages.Contains(normalizedCode))
        {
            normalizedCode = "et";
        }

        if (normalizedCode == CurrentLanguageCode)
        {
            return;
        }

        SetApplicationCulture(new CultureInfo(normalizedCode), true);
    }

    private void SetApplicationCulture(CultureInfo culture, bool saveLanguage)
    {
        currentCulture = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        if (saveLanguage)
        {
            Preferences.Default.Set(LanguagePreferenceKey, culture.TwoLetterISOLanguageName);
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private static CultureInfo ResolveInitialCulture()
    {
        var savedLanguage = Preferences.Default.Get(LanguagePreferenceKey, "et");
        var languageCode = SupportedLanguages.Contains(savedLanguage) ? savedLanguage : "et";

        return new CultureInfo(languageCode);
    }
}

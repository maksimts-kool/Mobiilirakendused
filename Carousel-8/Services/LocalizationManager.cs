using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Carousel_8.Services;

public sealed class LocalizationManager : INotifyPropertyChanged
{
    private const string LanguagePreferenceKey = "selected-language";
    private static readonly ResourceManager ResourceManager =
        new("Carousel_8.Resources.Localization.AppText", typeof(LocalizationManager).Assembly);
    private static readonly HashSet<string> SupportedLanguages =
        new(StringComparer.OrdinalIgnoreCase) { "en", "et" };

    private CultureInfo currentCulture = ResolveInitialCulture();

    public static LocalizationManager Instance { get; } = new();

    private LocalizationManager()
    {
        ApplyCulture(currentCulture, persistPreference: false);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? CultureChanged;

    public CultureInfo CurrentCulture => currentCulture;

    public string CurrentLanguageCode => currentCulture.TwoLetterISOLanguageName;

    public string this[string key] => GetString(key);

    public string GetString(string key)
    {
        return ResourceManager.GetString(key, currentCulture) ?? key;
    }

    public void SetCulture(string languageCode)
    {
        var normalizedCode = NormalizeLanguageCode(languageCode);

        if (normalizedCode == CurrentLanguageCode)
        {
            return;
        }

        ApplyCulture(new CultureInfo(normalizedCode), persistPreference: true);
    }

    private void ApplyCulture(CultureInfo culture, bool persistPreference)
    {
        currentCulture = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        if (persistPreference)
        {
            Preferences.Default.Set(LanguagePreferenceKey, culture.TwoLetterISOLanguageName);
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));

        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private static CultureInfo ResolveInitialCulture()
    {
        var savedLanguage = Preferences.Default.Get(LanguagePreferenceKey, string.Empty);
        var fallbackLanguage = string.IsNullOrWhiteSpace(savedLanguage)
            ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            : savedLanguage;

        return new CultureInfo(NormalizeLanguageCode(fallbackLanguage));
    }

    private static string NormalizeLanguageCode(string languageCode)
    {
        var normalizedCode = string.IsNullOrWhiteSpace(languageCode)
            ? "en"
            : languageCode.Trim().ToLowerInvariant();

        return SupportedLanguages.Contains(normalizedCode) ? normalizedCode : "en";
    }
}

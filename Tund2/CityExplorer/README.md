# CityExplorer: описание важных частей кода

`CityExplorer` - это часть MAUI-приложения, которая показывает места Таллинна по категориям, позволяет добавлять места в избранное и менять язык интерфейса.

## Запуск CityExplorer

Файл: `Tund2/StartPage.xaml.cs`

```csharp
new("CityExplorer", "Nutikas Tallinna giid karusselli, lemmikute ja keelevalikuga", () =>
{
    var databaseService = new DatabaseService();
    return new MainTabbedPage(
        new ExplorePage(new ExploreViewModel(databaseService)),
        new FavoritesPage(new FavoritesViewModel(databaseService)),
        new SettingsPage(new SettingsViewModel()));
})
```

Этот код добавляет CityExplorer в главное меню приложения. При открытии создается один `DatabaseService`, который используется страницами обзора и избранного.

## Модель места

Файл: `Tund2/CityExplorer/Models/Place.cs`

```csharp
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
}
```

`Place` хранит данные о месте: id, категорию, картинку и ключи для перевода. Название и описание берутся через `LocalizationManager`, поэтому текст может меняться при смене языка.

## Локализация

Файл: `Tund2/CityExplorer/Services/LocalizationManager.cs`

```csharp
private static readonly HashSet<string> SupportedLanguages =
    new(StringComparer.OrdinalIgnoreCase) { "et", "en", "ru" };

public string CurrentLanguageCode => currentCulture.TwoLetterISOLanguageName;

public string this[string key] =>
    ResourceManager.GetString(key, currentCulture) ?? key;
```

`LocalizationManager` отвечает за языки приложения. Он читает строки из `.resx` файлов и возвращает нужный текст по ключу.

```csharp
public void SetCulture(string languageCode)
{
    var normalizedCode = string.IsNullOrWhiteSpace(languageCode)
        ? "et"
        : languageCode.Trim().ToLowerInvariant();

    if (!SupportedLanguages.Contains(normalizedCode))
    {
        normalizedCode = "et";
    }

    SetApplicationCulture(new CultureInfo(normalizedCode), true);
}
```

Метод `SetCulture` меняет язык приложения. Если передан неподдерживаемый язык, приложение возвращается к эстонскому языку.

## База данных избранного

Файл: `Tund2/CityExplorer/Services/DatabaseService.cs`

```csharp
command.CommandText =
    """
    CREATE TABLE IF NOT EXISTS FavoritePlaces
    (
        Id INTEGER PRIMARY KEY,
        CategoryKey TEXT NOT NULL,
        Image TEXT NOT NULL,
        NameKey TEXT NOT NULL,
        ShortDescriptionKey TEXT NOT NULL,
        DetailKey TEXT NOT NULL
    );
    """;
```

Создается таблица `FavoritePlaces`, где хранятся избранные места.

```csharp
public async Task SaveFavoriteAsync(Place place)
{
    await InitializeAsync();

    await using var connection = new SqliteConnection($"Data Source={databasePath}");
    await connection.OpenAsync();

    await using var command = connection.CreateCommand();
    command.CommandText =
        """
        INSERT OR REPLACE INTO FavoritePlaces
            (Id, CategoryKey, Image, NameKey, ShortDescriptionKey, DetailKey)
        VALUES
            ($id, $categoryKey, $image, $nameKey, $shortDescriptionKey, $detailKey);
        """;
}
```

Метод сохраняет место в SQLite. Если место с таким `Id` уже существует, оно заменяется.

```csharp
public async Task DeleteFavoriteAsync(int placeId)
{
    await InitializeAsync();

    command.CommandText = "DELETE FROM FavoritePlaces WHERE Id = $id;";
    command.Parameters.AddWithValue("$id", placeId);

    await command.ExecuteNonQueryAsync();
}
```

Метод удаляет место из избранного по его `Id`.

## Главная логика обзора

Файл: `Tund2/CityExplorer/ViewModels/ExploreViewModel.cs`

```csharp
Categories = new ObservableCollection<Category>
{
    new() { Key = "history", Emoji = "🏰", TitleKey = "CategoryHistory" },
    new() { Key = "parks", Emoji = "🌳", TitleKey = "CategoryParks" },
    new() { Key = "food", Emoji = "🍽️", TitleKey = "CategoryFood" }
};
```

Создаются категории мест: история, парки и еда.

```csharp
allPlaces =
[
    new() { Id = 1, CategoryKey = "history", Image = "cityexplorer_toompea.png", NameKey = "PlaceToompeaName", ShortDescriptionKey = "PlaceToompeaShort", DetailKey = "PlaceToompeaDetail" },
    new() { Id = 2, CategoryKey = "history", Image = "cityexplorer_oldtown.png", NameKey = "PlaceOldTownName", ShortDescriptionKey = "PlaceOldTownShort", DetailKey = "PlaceOldTownDetail" },
    new() { Id = 3, CategoryKey = "parks", Image = "cityexplorer_kadriorg.png", NameKey = "PlaceKadriorgName", ShortDescriptionKey = "PlaceKadriorgShort", DetailKey = "PlaceKadriorgDetail" }
];
```

`allPlaces` содержит список мест. Каждое место связано с категорией и ключами локализации.

```csharp
public Category? SelectedCategory
{
    set
    {
        if (!SetProperty(ref selectedCategory, value) || selectedCategory is null)
        {
            return;
        }

        Places.Clear();
        foreach (var place in allPlaces.Where(place => place.CategoryKey == selectedCategory.Key))
        {
            Places.Add(place);
        }

        SelectedPlace = Places.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedCategoryTitle));
    }
}
```

Когда пользователь выбирает категорию, список `Places` очищается и заполняется только местами выбранной категории.

```csharp
public async Task<bool> AddFavoriteAsync(Place place)
{
    if (await databaseService.FavoriteExistsAsync(place.Id))
    {
        return false;
    }

    await databaseService.SaveFavoriteAsync(place);
    return true;
}
```

Метод добавляет место в избранное. Если оно уже сохранено, возвращается `false`.

## Страница обзора

Файл: `Tund2/CityExplorer/Views/ExplorePage.xaml`

```xml
<CarouselView x:Name="PlaceCarousel"
              ItemsSource="{Binding Places}"
              CurrentItem="{Binding SelectedPlace, Mode=TwoWay}"
              IndicatorView="PlaceIndicator"
              IsSwipeEnabled="True"
              Loop="True">
```

`CarouselView` показывает места в виде карусели. Данные берутся из коллекции `Places`.

Файл: `Tund2/CityExplorer/Views/ExplorePage.xaml.cs`

```csharp
autoScrollTimer.Interval = TimeSpan.FromSeconds(4);
autoScrollTimer.Tick += (_, _) =>
{
    if (viewModel.Places.Count <= 1 || PlaceCarousel.IsDragging)
    {
        return;
    }

    var nextPosition = (PlaceCarousel.Position + 1) % viewModel.Places.Count;
    PlaceCarousel.ScrollTo(nextPosition, position: ScrollToPosition.Center, animate: true);
};
```

Таймер автоматически прокручивает карусель каждые 4 секунды.

```csharp
private async void OnPlaceTapped(object? sender, TappedEventArgs e)
{
    if (e.Parameter is not Place place)
    {
        return;
    }

    var addFavorite = await DisplayAlertAsync(
        place.Name,
        place.Detail,
        viewModel.Localizer["AddFavorite"],
        viewModel.Localizer["Close"]);
}
```

При нажатии на карточку открывается окно с подробным описанием места и кнопкой добавления в избранное.

## Избранное

Файл: `Tund2/CityExplorer/ViewModels/FavoritesViewModel.cs`

```csharp
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
```

Метод загружает избранные места из SQLite и обновляет список на экране.

```csharp
private async Task RemoveFavoriteAsync(Place? place)
{
    if (place is null)
    {
        return;
    }

    await databaseService.DeleteFavoriteAsync(place.Id);
    Favorites.Remove(place);
}
```

Метод удаляет место из базы данных и из списка избранного.

## Настройки языка

Файл: `Tund2/CityExplorer/ViewModels/SettingsViewModel.cs`

```csharp
ChangeLanguageCommand = new Command<string>(
    languageCode => Localizer.SetCulture(languageCode));
```

Команда меняет язык приложения по коду языка: `et`, `en` или `ru`.

Файл: `Tund2/CityExplorer/Views/SettingsPage.xaml`

```xml
<Button Text="{Binding Localizer[Russian]}"
        Command="{Binding ChangeLanguageCommand}"
        CommandParameter="ru" />
```

Кнопка передает параметр `"ru"` и включает русский язык.

## Вкладки приложения

Файл: `Tund2/CityExplorer/Views/MainTabbedPage.xaml.cs`

```csharp
Children.Add(explorePage);
Children.Add(favoritesPage);
Children.Add(settingsPage);

localizer.CultureChanged += (_, _) => UpdateTitles();
UpdateTitles();
```

`MainTabbedPage` объединяет три вкладки: обзор, избранное и настройки. При смене языка названия вкладок обновляются автоматически.


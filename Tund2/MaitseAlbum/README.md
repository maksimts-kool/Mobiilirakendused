# MaitseAlbum: описание важных частей кода

`MaitseAlbum` - это часть MAUI-приложения для хранения рецептов. Пользователь может добавить название блюда, выбрать категорию, прикрепить картинку и потом посмотреть все рецепты, сгруппированные по категориям.

## Запуск MaitseAlbum

Файл: `Tund2/StartPage.xaml.cs`

```csharp
new("Maitsealbum", "Lihtne failiga retseptileht piltide ja kategooriatega",
    () => new MaitseAlbumPage(), "MA", "#ECFDF5", "#0F766E")
```

Этот код добавляет `MaitseAlbum` в главное меню приложения. При нажатии открывается страница `MaitseAlbumPage`.

## Главная страница с вкладками

Файл: `Tund2/MaitseAlbum/MaitseAlbumPage.xaml`

```xml
<TabbedPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
            xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
            xmlns:local="clr-namespace:Tund2"
            x:Class="Tund2.MaitseAlbumPage"
            Title="Maitsealbum"
            BarBackgroundColor="White"
            BarTextColor="#0F766E"
            SelectedTabColor="#0F766E"
            UnselectedTabColor="#334155">
```

`MaitseAlbumPage` - это `TabbedPage`, то есть страница с вкладками. В ней находятся две вкладки: добавление рецепта и список рецептов.

```xml
<local:UusRetseptPage>
    <local:UusRetseptPage.IconImageSource>
        <FontImageSource Glyph="+"
                         Size="28"
                         Color="#0F766E"/>
    </local:UusRetseptPage.IconImageSource>
</local:UusRetseptPage>

<local:RetseptideNimekiriPage>
    <local:RetseptideNimekiriPage.IconImageSource>
        <FontImageSource Glyph="&#x2261;"
                         Size="28"
                         Color="#0F766E"/>
    </local:RetseptideNimekiriPage.IconImageSource>
</local:RetseptideNimekiriPage>
```

Первая вкладка открывает форму добавления рецепта, вторая вкладка показывает сохраненные рецепты.

Файл: `Tund2/MaitseAlbum/MaitseAlbumPage.xaml.cs`

```csharp
public partial class MaitseAlbumPage : TabbedPage
{
    public MaitseAlbumPage()
    {
        SetAppleTabColors();
        InitializeComponent();
    }
}
```

В конструкторе настраиваются цвета вкладок для Apple-платформ и загружается XAML-интерфейс.

## Модель рецепта

Файл: `Tund2/MaitseAlbum/Retsept.cs`

```csharp
public class Retsept
{
    public string Nimi { get; set; } = string.Empty;

    public string Kategooria { get; set; } = string.Empty;

    public string PildiLink { get; set; } = string.Empty;
}
```

`Retsept` хранит основные данные одного рецепта:

- `Nimi` - название рецепта.
- `Kategooria` - категория блюда.
- `PildiLink` - путь к картинке рецепта.

## Группа рецептов

Файл: `Tund2/MaitseAlbum/RetseptiKategooria.cs`

```csharp
public class RetseptiKategooria : List<Retsept>
{
    public string Nimetus { get; set; }

    public RetseptiKategooria(string nimetus, IEnumerable<Retsept> retseptid)
        : base(retseptid)
    {
        Nimetus = nimetus;
    }
}
```

`RetseptiKategooria` нужна для группировки рецептов в списке. Она хранит название категории и рецепты, которые относятся к этой категории.

## Работа с файлом

Файл: `Tund2/MaitseAlbum/FailiHaldur.cs`

```csharp
private static readonly string FailiTee =
    Path.Combine(FileSystem.AppDataDirectory, "retseptid.txt");
```

Все рецепты сохраняются в текстовый файл `retseptid.txt` внутри папки приложения.

```csharp
public static List<Retsept> LoeRetseptid()
{
    var nimekiri = new List<Retsept>();

    if (!File.Exists(FailiTee))
    {
        return nimekiri;
    }

    string[] read = File.ReadAllLines(FailiTee);
```

Метод `LoeRetseptid` читает файл с рецептами. Если файла еще нет, возвращается пустой список.

```csharp
string[] osad = rida.Split(';');

if (osad.Length >= 3)
{
    nimekiri.Add(new Retsept
    {
        Nimi = osad[0].Trim(),
        Kategooria = osad[1].Trim(),
        PildiLink = osad[2].Trim()
    });
}
```

Каждая строка файла делится по символу `;`. Из трех частей создается объект `Retsept`.

```csharp
public static void SalvestaRetsept(Retsept retsept)
{
    string rida = $"{PuhastaTekst(retsept.Nimi)};{PuhastaTekst(retsept.Kategooria)};{PuhastaTekst(retsept.PildiLink)}";
    File.AppendAllText(FailiTee, rida + Environment.NewLine);
}
```

Метод добавляет новый рецепт в конец файла.

```csharp
public static void SalvestaKoikRetseptid(List<Retsept> retseptid)
{
    var read = retseptid.Select(retsept =>
        $"{PuhastaTekst(retsept.Nimi)};{PuhastaTekst(retsept.Kategooria)};{PuhastaTekst(retsept.PildiLink)}");

    File.WriteAllLines(FailiTee, read);
}
```

Метод полностью перезаписывает файл. Он используется, когда нужно удалить рецепт из списка.

```csharp
private static string PuhastaTekst(string tekst)
{
    return tekst.Replace(";", ",").Replace("\r", " ").Replace("\n", " ").Trim();
}
```

Метод очищает текст перед сохранением: убирает `;` и переносы строк, чтобы формат файла не ломался.

## Форма добавления рецепта

Файл: `Tund2/MaitseAlbum/UusRetseptPage.xaml`

```xml
<Entry x:Name="NimiEntry"
       Placeholder="Nt Pasta Carbonara"
       BackgroundColor="Transparent"
       TextColor="#0F172A"
       TextChanged="OnNimiChanged"/>
```

Поле для ввода названия рецепта.

```xml
<Picker x:Name="KategooriaPicker"
        Title="Vali kategooria"
        SelectedIndexChanged="OnKategooriaChanged">
    <Picker.Items>
        <x:String>Hommikusöögid</x:String>
        <x:String>Supid</x:String>
        <x:String>Pearoad</x:String>
        <x:String>Magustoidud</x:String>
        <x:String>Snäkid</x:String>
        <x:String>Joogid</x:String>
    </Picker.Items>
</Picker>
```

`Picker` дает выбрать категорию рецепта.

```xml
<Image x:Name="PildiEelvaade"
       Aspect="AspectFill"/>

<Button Text="Vali pilt"
        Clicked="OnValiPiltClicked"/>

<Button Text="Salvesta"
        Clicked="OnSalvestaClicked"/>
```

Здесь показывается выбранная картинка, а кнопки запускают выбор изображения и сохранение рецепта.

Файл: `Tund2/MaitseAlbum/UusRetseptPage.xaml.cs`

```csharp
private async void OnValiPiltClicked(object? sender, EventArgs e)
{
    IEnumerable<FileResult> pildid = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
    {
        Title = "Vali retsepti pilt"
    });

    FileResult? pilt = pildid.FirstOrDefault();
}
```

Метод открывает выбор изображения из устройства. Пользователь выбирает картинку для рецепта.

```csharp
string uusFailiNimi = $"retsept_{DateTime.Now:yyyyMMddHHmmssfff}{laiend}";
string uusPildiTee = Path.Combine(FileSystem.AppDataDirectory, uusFailiNimi);

using Stream vanaPilt = await pilt.OpenReadAsync();
using FileStream uusPilt = File.OpenWrite(uusPildiTee);
await vanaPilt.CopyToAsync(uusPilt);
```

Выбранная картинка копируется в папку приложения. Это нужно, чтобы приложение могло использовать изображение позже.

```csharp
string nimi = NimiEntry.Text?.Trim() ?? string.Empty;
string kategooria = KategooriaPicker.SelectedItem?.ToString() ?? string.Empty;

bool nimiPuudub = string.IsNullOrWhiteSpace(nimi);
bool kategooriaPuudub = string.IsNullOrWhiteSpace(kategooria);
bool piltPuudub = string.IsNullOrWhiteSpace(valitudPildiTee);
```

Перед сохранением проверяется, заполнены ли название, категория и картинка.

```csharp
FailiHaldur.SalvestaRetsept(new Retsept
{
    Nimi = nimi,
    Kategooria = kategooria,
    PildiLink = valitudPildiTee
});
```

Если все данные заполнены, создается новый `Retsept` и сохраняется в файл.

```csharp
private static void MuudaViga(Border border, Label label, bool naitaViga)
{
    border.Stroke = naitaViga ? VeaStroke : TavalineStroke;
    label.IsVisible = naitaViga;
}
```

Метод показывает или скрывает ошибку: меняет цвет рамки и видимость текста ошибки.

## Список рецептов

Файл: `Tund2/MaitseAlbum/RetseptideNimekiriPage.xaml`

```xml
<ListView x:Name="RetseptidListView"
          IsGroupingEnabled="True"
          HasUnevenRows="True"
          SeparatorVisibility="None">
```

`ListView` показывает рецепты. `IsGroupingEnabled="True"` включает группировку по категориям.

```xml
<ListView.GroupHeaderTemplate>
    <DataTemplate>
        <ViewCell>
            <Grid Padding="12,8"
                  BackgroundColor="#0F766E">
                <Label Text="{Binding Nimetus}"
                       FontSize="18"
                       FontAttributes="Bold"
                       TextColor="White"/>
            </Grid>
        </ViewCell>
    </DataTemplate>
</ListView.GroupHeaderTemplate>
```

Этот шаблон показывает заголовок группы, то есть название категории.

```xml
<Image Source="{Binding PildiLink}"
       WidthRequest="70"
       HeightRequest="70"
       Aspect="AspectFill"/>

<Label Text="{Binding Nimi}"
       FontSize="17"
       FontAttributes="Bold"/>

<Label Text="{Binding Kategooria}"
       FontSize="13"/>
```

В каждой строке списка показываются картинка, название и категория рецепта.

```xml
<MenuItem Text="Kustuta"
          IsDestructive="True"
          Clicked="OnKustutaClicked"
          CommandParameter="{Binding .}"/>
```

Контекстное действие для удаления рецепта.

Файл: `Tund2/MaitseAlbum/RetseptideNimekiriPage.xaml.cs`

```csharp
protected override void OnAppearing()
{
    base.OnAppearing();
    LaeRetseptid();
}
```

Каждый раз при открытии вкладки список рецептов загружается заново.

```csharp
private void LaeRetseptid()
{
    var retseptid = FailiHaldur.LoeRetseptid();

    var grupid = retseptid
        .GroupBy(retsept => retsept.Kategooria)
        .OrderBy(grupp => grupp.Key)
        .Select(grupp => new RetseptiKategooria(grupp.Key, grupp.OrderBy(retsept => retsept.Nimi)))
        .ToList();

    RetseptidListView.ItemsSource = grupid;
    TyhiTeade.IsVisible = grupid.Count == 0;
}
```

Метод читает рецепты из файла, группирует их по категориям и передает результат в `ListView`.

```csharp
private async void OnKustutaClicked(object? sender, EventArgs e)
{
    if ((sender as MenuItem)?.CommandParameter is not Retsept valitudRetsept)
    {
        return;
    }

    var retseptid = FailiHaldur.LoeRetseptid();
```

Метод получает выбранный рецепт из `CommandParameter` и загружает весь список рецептов из файла.

```csharp
retseptid.Remove(kustutatavRetsept);
FailiHaldur.SalvestaKoikRetseptid(retseptid);
```

После удаления рецепта файл полностью перезаписывается уже без выбранного рецепта.

```csharp
if (File.Exists(valitudRetsept.PildiLink) &&
    valitudRetsept.PildiLink.StartsWith(FileSystem.AppDataDirectory, StringComparison.Ordinal))
{
    File.Delete(valitudRetsept.PildiLink);
}
```

При удалении рецепта также удаляется его картинка, если она находится в папке приложения.


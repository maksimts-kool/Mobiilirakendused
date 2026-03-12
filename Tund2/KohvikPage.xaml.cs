using Microsoft.Maui.Storage;

namespace Tund2;

public partial class KohvikPage : ContentPage
{
    // Tellimuse andmed
    private string customerName = "";
    private string coffeeType = "";
    private string cupSize = "";
    private string sugarAmount = "";
    private bool hasSyrup = false;

    // Hinnad 
    private static readonly Dictionary<string, decimal> CoffeePrice = new()
    {
        { "Espresso",   1.80m },
        { "Americano",  2.10m },
        { "Cappuccino", 2.60m },
        { "Latte",      2.90m }
    };
    private static readonly Dictionary<string, decimal> SizeExtra = new()
    {
        { "S (Väike)",   0.00m },
        { "M (Keskmine)", 0.30m },
        { "L (Suur)",    0.60m }
    };
    private const decimal SyrupPrice = 0.50m;

    public KohvikPage()
    {
        InitializeComponent();
    }

    // Salvestatud tellimuse taastamine
    protected override void OnAppearing()
    {
        base.OnAppearing();

        customerName = Preferences.Default.Get("Kohvik_Nimi", "");
        coffeeType = Preferences.Default.Get("Kohvik_Jook", "");
        cupSize = Preferences.Default.Get("Kohvik_Suurus", "");
        sugarAmount = Preferences.Default.Get("Kohvik_Suhkur", "");
        hasSyrup = Preferences.Default.Get("Kohvik_Siirup", false);

        if (!string.IsNullOrEmpty(customerName) && !string.IsNullOrEmpty(coffeeType))
        {
            nameOnCupLabel.Text = customerName;
            nameOnCupLabel.IsVisible = true;

            ApplyCoffeeTheme(coffeeType);

            switch (cupSize)
            {
                case "S (Väike)":
                    cupBorder.WidthRequest = 100; cupBorder.HeightRequest = 130; cupEmojiLabel.FontSize = 55;
                    cupHandle.WidthRequest = 37; cupHandle.HeightRequest = 55;
                    cupWrapper.WidthRequest = 125; cupWrapper.HeightRequest = 130;
                    break;
                case "M (Keskmine)":
                    cupBorder.WidthRequest = 150; cupBorder.HeightRequest = 190; cupEmojiLabel.FontSize = 80;
                    cupHandle.WidthRequest = 55; cupHandle.HeightRequest = 85;
                    cupWrapper.WidthRequest = 185; cupWrapper.HeightRequest = 190;
                    break;
                case "L (Suur)":
                    cupBorder.WidthRequest = 195; cupBorder.HeightRequest = 240; cupEmojiLabel.FontSize = 100;
                    cupHandle.WidthRequest = 70; cupHandle.HeightRequest = 110;
                    cupWrapper.WidthRequest = 240; cupWrapper.HeightRequest = 240;
                    break;
            }

            infoLabel.Text = $"☕ {coffeeType} ({cupSize}) — {customerName} jaoks valmis!";
            UpdatePriceCard(CalculateTotal());
            manageOrderButton.IsEnabled = true;
            manageOrderButton.TextColor = Colors.White;
        }
    }

    // tellimuse loomine
    private async void NewOrder_Clicked(object? sender, EventArgs e)
    {
        newOrderButton.IsEnabled = false;

        // 1: Nimi
        string? name = await DisplayPromptAsync(
            "Tere tulemast!",
            "Kuidas teid kutsutakse?",
            accept: "Edasi",
            cancel: "Loobu",
            placeholder: "Teie nimi...",
            maxLength: 20);

        if (string.IsNullOrWhiteSpace(name))
        {
            newOrderButton.IsEnabled = true;
            return;
        }
        customerName = name.Trim();
        nameOnCupLabel.Text = customerName;
        nameOnCupLabel.IsVisible = true;

        // 2: Joogi valik
        string? coffee = await DisplayActionSheetAsync(
            "Valige jook",
            "Loobu",
            null,
            "Espresso", "Americano", "Cappuccino", "Latte");

        if (coffee == null || coffee == "Loobu")
        {
            newOrderButton.IsEnabled = true;
            return;
        }
        coffeeType = coffee;

        ApplyCoffeeTheme(coffeeType);
        infoLabel.Text = $"Valmistame {coffeeType}'t {customerName} jaoks...";
        UpdatePriceCard(CoffeePrice[coffeeType]);

        // 3: Suurus
        string? size = await DisplayActionSheetAsync(
            "Valige portsjni suurus",
            "Loobu",
            null,
            "S (Väike)", "M (Keskmine)", "L (Suur)");

        if (size == null || size == "Loobu")
        {
            newOrderButton.IsEnabled = true;
            return;
        }
        cupSize = size;

        switch (cupSize)
        {
            case "S (Väike)":
                cupBorder.WidthRequest = 100;
                cupBorder.HeightRequest = 130;
                cupEmojiLabel.FontSize = 55;
                cupHandle.WidthRequest = 37; cupHandle.HeightRequest = 55;
                cupWrapper.WidthRequest = 125; cupWrapper.HeightRequest = 130;
                break;
            case "M (Keskmine)":
                cupBorder.WidthRequest = 150;
                cupBorder.HeightRequest = 190;
                cupEmojiLabel.FontSize = 80;
                cupHandle.WidthRequest = 55; cupHandle.HeightRequest = 85;
                cupWrapper.WidthRequest = 185; cupWrapper.HeightRequest = 190;
                break;
            case "L (Suur)":
                cupBorder.WidthRequest = 195;
                cupBorder.HeightRequest = 240;
                cupEmojiLabel.FontSize = 100;
                cupHandle.WidthRequest = 70; cupHandle.HeightRequest = 110;
                cupWrapper.WidthRequest = 240; cupWrapper.HeightRequest = 240;
                break;
        }

        decimal runningTotal = CoffeePrice[coffeeType] + SizeExtra[cupSize];
        infoLabel.Text = $"{coffeeType} ({cupSize}) — {customerName}";
        UpdatePriceCard(runningTotal);

        // 4: Suhkur
        string? sugar = await DisplayPromptAsync(
            "Suhkur",
            "Mitu lusikatäit suhkrut lisada?",
            accept: "Lisa",
            cancel: "Ilma suhkruta",
            initialValue: "2",
            maxLength: 2,
            keyboard: Keyboard.Numeric);

        sugarAmount = string.IsNullOrEmpty(sugar) ? "0" : sugar;

        // 5: Karamellisiirup
        bool syrup = await DisplayAlertAsync(
            "Lisand",
            "Kas lisada karamellisiirup 0,50 € eest?",
            "Jah, palun!",
            "Ei, aitäh");
        hasSyrup = syrup;

        // 6: Kviitung
        await DisplayAlertAsync(
            "✅ Teie tellimus on valmis!",
            BuildReceipt(),
            "Aitäh!");

        infoLabel.Text = $"☕ {coffeeType} ({cupSize}) — {customerName} jaoks valmis!";
        UpdatePriceCard(CalculateTotal());
        manageOrderButton.IsEnabled = true;
        manageOrderButton.TextColor = Colors.White;
        newOrderButton.IsEnabled = true;

        Preferences.Default.Set("Kohvik_Nimi", customerName);
        Preferences.Default.Set("Kohvik_Jook", coffeeType);
        Preferences.Default.Set("Kohvik_Suurus", cupSize);
        Preferences.Default.Set("Kohvik_Suhkur", sugarAmount);
        Preferences.Default.Set("Kohvik_Siirup", hasSyrup);
    }

    // Tellimuse haldamine
    private async void ManageOrder_Clicked(object? sender, EventArgs e)
    {
        string? action = await DisplayActionSheetAsync(
            "Halda tellimust",
            "Loobu",
            "🗑️ Viska kohv ära",
            "🧾 Näita kviitungit");

        if (action == "🧾 Näita kviitungit")
        {
            await DisplayAlertAsync("🧾 Kviitung", BuildReceipt(), "Sulge");
        }
        else if (action == "🗑️ Viska kohv ära")
        {
            ResetOrder();
        }
    }

    // Taustavärvi rakendamine
    private void ApplyCoffeeTheme(string coffee)
    {
        (Color page, Color panel) = coffee switch
        {
            "Espresso" => (Color.FromArgb("#1A0A00"), Color.FromArgb("#3E1A06")),
            "Americano" => (Color.FromArgb("#2C1A0E"), Color.FromArgb("#4A2E1A")),
            "Cappuccino" => (Color.FromArgb("#5C3317"), Color.FromArgb("#7A4E2A")),
            "Latte" => (Color.FromArgb("#7A4E30"), Color.FromArgb("#9B6B45")),
            _ => (Color.FromArgb("#3E2A1A"), Color.FromArgb("#5C3317"))
        };
        BackgroundColor = page;
        infoBorder.BackgroundColor = panel;
    }

    // Hinna kaart
    private void UpdatePriceCard(decimal amount)
    {
        priceLabel.Text = $"{amount:0.00} €";
        priceBorder.IsVisible = true;
    }

    // Hinna arvutamine
    private decimal CalculateTotal()
    {
        decimal total = 0;
        if (CoffeePrice.TryGetValue(coffeeType, out decimal cp)) total += cp;
        if (SizeExtra.TryGetValue(cupSize, out decimal se)) total += se;
        if (hasSyrup) total += SyrupPrice;
        return total;
    }

    // Kviitungi koostamine
    private string BuildReceipt()
    {
        decimal basePrice = CoffeePrice.TryGetValue(coffeeType, out var cp) ? cp : 0;
        decimal sizeExtra = SizeExtra.TryGetValue(cupSize, out var se) ? se : 0;
        string syrupLine = hasSyrup
            ? $"Karamellisiirup:  jah  +{SyrupPrice:0.00} €"
            : "Karamellisiirup:  ei";
        decimal total = CalculateTotal();

        return $"─────────────────\n" +
               $"Klient:     {customerName}\n" +
               $"Jook:       {coffeeType}  {basePrice:0.00} €\n" +
               $"Suurus:     {cupSize}  +{sizeExtra:0.00} €\n" +
               $"Suhkur:     {sugarAmount} lusikatäit\n" +
               $"{syrupLine}\n" +
               $"─────────────────\n" +
               $"KOKKU:      {total:0.00} €";
    }

    // Taastamine 
    private void ResetOrder()
    {
        customerName = "";
        coffeeType = "";
        cupSize = "";
        sugarAmount = "";
        hasSyrup = false;

        nameOnCupLabel.Text = "";
        nameOnCupLabel.IsVisible = false;
        infoLabel.Text = "Vajuta 'Uus tellimus', et koostada oma ideaalne kohv...";
        priceBorder.IsVisible = false;
        BackgroundColor = Color.FromArgb("#3E2A1A");
        infoBorder.BackgroundColor = Color.FromArgb("#5C3317");

        cupBorder.WidthRequest = 150;
        cupBorder.HeightRequest = 190;
        cupEmojiLabel.FontSize = 80;
        cupHandle.WidthRequest = 55; cupHandle.HeightRequest = 85;
        cupWrapper.WidthRequest = 185; cupWrapper.HeightRequest = 190;

        manageOrderButton.IsEnabled = false;
        manageOrderButton.TextColor = Color.FromArgb("#888888");

        Preferences.Default.Remove("Kohvik_Nimi");
        Preferences.Default.Remove("Kohvik_Jook");
        Preferences.Default.Remove("Kohvik_Suurus");
        Preferences.Default.Remove("Kohvik_Suhkur");
        Preferences.Default.Remove("Kohvik_Siirup");
    }
}

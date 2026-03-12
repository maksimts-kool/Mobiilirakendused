using Microsoft.Maui.Storage;

namespace Tund2;

public partial class KohvikPage : ContentPage
{
    // Tellimuse andmed (klassi taseme muutujad)
    private string customerName = "";
    private string coffeeType = "";
    private string cupSize = "";
    private string sugarAmount = "";
    private bool hasSyrup = false;

    public KohvikPage()
    {
        InitializeComponent();
    }

    // ─── Salvestatud tellimuse taastamine rakenduse avamisel ──────────────────
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
            // Taasta visuaalne olek
            nameOnCupLabel.Text = customerName;
            nameOnCupLabel.IsVisible = true;

            BackgroundColor = coffeeType switch
            {
                "Espresso" => Color.FromArgb("#1A0A00"),
                "Americano" => Color.FromArgb("#2C1A0E"),
                "Cappuccino" => Color.FromArgb("#5C3317"),
                "Latte" => Color.FromArgb("#7A4E30"),
                _ => Color.FromArgb("#3E2A1A")
            };

            switch (cupSize)
            {
                case "S (Väike)":
                    cupBorder.WidthRequest = 100; cupBorder.HeightRequest = 130; cupEmojiLabel.FontSize = 55;
                    break;
                case "M (Keskmine)":
                    cupBorder.WidthRequest = 150; cupBorder.HeightRequest = 190; cupEmojiLabel.FontSize = 80;
                    break;
                case "L (Suur)":
                    cupBorder.WidthRequest = 195; cupBorder.HeightRequest = 240; cupEmojiLabel.FontSize = 100;
                    break;
            }

            infoLabel.Text = $"☕ {coffeeType} ({cupSize}) — {customerName} jaoks valmis!";
            manageOrderButton.IsEnabled = true;
            manageOrderButton.TextColor = Colors.White;
        }
    }

    // ─── Samm-sammult tellimuse loomine ───────────────────────────────────────
    private async void NewOrder_Clicked(object? sender, EventArgs e)
    {
        newOrderButton.IsEnabled = false;

        // ── Samm 1: Nimi (DisplayPromptAsync) ─────────────────────────────────
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

        // ── Samm 2: Joogi valik (DisplayActionSheet) ──────────────────────────
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

        // Taustavärv joogi tugevuse järgi
        BackgroundColor = coffeeType switch
        {
            "Espresso" => Color.FromArgb("#1A0A00"),
            "Americano" => Color.FromArgb("#2C1A0E"),
            "Cappuccino" => Color.FromArgb("#5C3317"),
            "Latte" => Color.FromArgb("#7A4E30"),
            _ => Color.FromArgb("#3E2A1A")
        };
        infoLabel.Text = $"Valmistame {coffeeType}'t {customerName} jaoks...";

        // ── Samm 3: Suurus (DisplayActionSheet + suuruse muutmine) ────────────
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

        // Staakaniku visuaalne suuruse muutmine (switch/case)
        switch (cupSize)
        {
            case "S (Väike)":
                cupBorder.WidthRequest = 100;
                cupBorder.HeightRequest = 130;
                cupEmojiLabel.FontSize = 55;
                break;
            case "M (Keskmine)":
                cupBorder.WidthRequest = 150;
                cupBorder.HeightRequest = 190;
                cupEmojiLabel.FontSize = 80;
                break;
            case "L (Suur)":
                cupBorder.WidthRequest = 195;
                cupBorder.HeightRequest = 240;
                cupEmojiLabel.FontSize = 100;
                break;
        }

        // ── Samm 4: Suhkur (DisplayPromptAsync koos vaikeväärtusega) ──────────
        string? sugar = await DisplayPromptAsync(
            "Suhkur",
            "Mitu lusikatäit suhkrut lisada?",
            accept: "Lisa",
            cancel: "Ilma suhkruta",
            initialValue: "2",
            maxLength: 2,
            keyboard: Keyboard.Numeric);

        // Kui kasutaja vajutab "Ilma suhkruta" (Cancel), sugar = null → 0
        sugarAmount = string.IsNullOrEmpty(sugar) ? "0" : sugar;

        // ── Samm 5: Karamellisiirup (DisplayAlert Jah/Ei) ─────────────────────
        bool syrup = await DisplayAlertAsync(
            "Lisand",
            "Kas lisada karamellisiirup 0,50 € eest?",
            "Jah, palun!",
            "Ei, aitäh");
        hasSyrup = syrup;

        // ── Samm 6: Kviitung (lõplik DisplayAlert) ────────────────────────────
        await DisplayAlertAsync(
            "✅ Teie tellimus on valmis!",
            BuildReceipt(),
            "Aitäh!");

        infoLabel.Text = $"☕ {coffeeType} ({cupSize}) — {customerName} jaoks valmis!";
        manageOrderButton.IsEnabled = true;
        manageOrderButton.TextColor = Colors.White;
        newOrderButton.IsEnabled = true;

        // Salvesta tellimus seadme mällu
        Preferences.Default.Set("Kohvik_Nimi", customerName);
        Preferences.Default.Set("Kohvik_Jook", coffeeType);
        Preferences.Default.Set("Kohvik_Suurus", cupSize);
        Preferences.Default.Set("Kohvik_Suhkur", sugarAmount);
        Preferences.Default.Set("Kohvik_Siirup", hasSyrup);
    }

    // ─── Tellimuse haldamine (Destructive ActionSheet) ────────────────────────
    private async void ManageOrder_Clicked(object? sender, EventArgs e)
    {
        // "Viska kohv ära" on Destructive nupp (iOS-il punane)
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

    // ─── Kviitungi koostamine ─────────────────────────────────────────────────
    private string BuildReceipt()
    {
        string syrupLine = hasSyrup
            ? "Karamellisiirup:  jah  +0,50 €"
            : "Karamellisiirup:  ei";

        return $"─────────────────\n" +
               $"Klient:    {customerName}\n" +
               $"Jook:      {coffeeType}\n" +
               $"Suurus:    {cupSize}\n" +
               $"Suhkur:    {sugarAmount} lusikatäit\n" +
               $"{syrupLine}\n" +
               $"─────────────────";
    }

    // ─── Lähtestamine ─────────────────────────────────────────────────────────
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
        BackgroundColor = Color.FromArgb("#3E2A1A");

        cupBorder.WidthRequest = 150;
        cupBorder.HeightRequest = 190;
        cupEmojiLabel.FontSize = 80;

        manageOrderButton.IsEnabled = false;
        manageOrderButton.TextColor = Color.FromArgb("#888888");

        // Kustuta salvestatud tellimus seadme mälust
        Preferences.Default.Remove("Kohvik_Nimi");
        Preferences.Default.Remove("Kohvik_Jook");
        Preferences.Default.Remove("Kohvik_Suurus");
        Preferences.Default.Remove("Kohvik_Suhkur");
        Preferences.Default.Remove("Kohvik_Siirup");
    }
}

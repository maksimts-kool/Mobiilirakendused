using Microsoft.Maui.Storage;

namespace Tund2;

public partial class Pop_Up_Page : ContentPage
{
    public Pop_Up_Page()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Loeme seadme mälust muutuja "EsimeneKäivitamine".
        bool onEsimeneStart = Preferences.Default.Get("EsimeneKäivitamine", true);

        // 2. Kui on esimene start, kuvame dialoogiakna
        if (onEsimeneStart)
        {
            bool vastus = await DisplayAlertAsync("Tere tulemast!",
                                            "Tundub, et avasid selle rakenduse esimest korda. Kas soovid näha lühikest juhendit?",
                                            "Jah, palun",
                                            "Ei, saan ise hakkama");

            if (vastus)
            {
                await DisplayAlertAsync("Juhend",
                    "Siin on sinu lühike juhend: vali menüüst sobiv teema ja uuri, kuidas elemendid töötavad!",
                    "Selge");
            }

            // 3. Salvestame info, et esimene käivitamine on tehtud.
            Preferences.Default.Set("EsimeneKäivitamine", false);
        }
    }

    // 1. Nupp: Lihtne teade
    private async void AlertButton_Clicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync("Teade", "Teil on uus teade", "OK");
    }

    // 2. Nupp: Jah või ei valik
    private async void AlertYesNoButton_Clicked(object? sender, EventArgs e)
    {
        bool result = await DisplayAlertAsync("Kinnitus", "Kas oled kindel?", "Olen kindel", "Ei ole kindel");

        await DisplayAlertAsync("Teade", "Teie valik on: " + (result ? "Jah" : "Ei"), "OK");
    }

    // 3. Nupp: Valikute nimekiri
    private async void AlertListButton_Clicked(object? sender, EventArgs e)
    {
        string action = await DisplayActionSheetAsync("Mida teha?", "Loobu", "Kustutada", "Tantsida", "Laulda", "Joonestada");

        if (action != null && action != "Loobu")
        {
            await DisplayAlertAsync("Valik", "Sa valisid tegevuse: " + action, "OK");
        }
    }

    // Valikuvastusega
    private async void AlertQuestButton_Clicked(object? sender, EventArgs e)
    {
        string result1 = await DisplayPromptAsync("Küsimus", "Kuidas läheb?", placeholder: "Tore!");
        string result2 = await DisplayPromptAsync("Vasta", "Millega võrdub 5 + 5?", initialValue:
            "10", maxLength: 2, keyboard: Keyboard.Numeric);
    }

    // Nulli seaded nupp
    private async void NulliNupp_Clicked(object? sender, EventArgs e)
    {
        Preferences.Default.Remove("EsimeneKäivitamine");

        await DisplayAlertAsync("Edukalt nullitud", "Mälu on tühjendatud. Kui sa lehe uuesti avad, käitub äpp nagu täiesti uus!", "OK");
    }
}

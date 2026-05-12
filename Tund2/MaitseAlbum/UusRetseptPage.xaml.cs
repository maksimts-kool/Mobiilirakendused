namespace Tund2;

public partial class UusRetseptPage : ContentPage
{
    private static readonly Brush TavalineStroke = new SolidColorBrush(Color.FromArgb("#D1FAE5"));
    private static readonly Brush VeaStroke = new SolidColorBrush(Color.FromArgb("#DC2626"));

    private string valitudPildiTee = string.Empty;

    public UusRetseptPage()
    {
        InitializeComponent();
    }

    private async void OnValiPiltClicked(object? sender, EventArgs e)
    {
        try
        {
            IEnumerable<FileResult> pildid = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Vali retsepti pilt"
            });
            FileResult? pilt = pildid.FirstOrDefault();

            if (pilt is null)
            {
                return;
            }

            string laiend = Path.GetExtension(pilt.FileName);

            if (string.IsNullOrWhiteSpace(laiend))
            {
                laiend = ".jpg";
            }

            string uusFailiNimi = $"retsept_{DateTime.Now:yyyyMMddHHmmssfff}{laiend}";
            string uusPildiTee = Path.Combine(FileSystem.AppDataDirectory, uusFailiNimi);

            using Stream vanaPilt = await pilt.OpenReadAsync();
            using FileStream uusPilt = File.OpenWrite(uusPildiTee);
            await vanaPilt.CopyToAsync(uusPilt);

            valitudPildiTee = uusPildiTee;
            PildiEelvaade.Source = ImageSource.FromFile(valitudPildiTee);
            PildiNimiLabel.Text = pilt.FileName;
            MuudaViga(PildiBorder, PildiVigaLabel, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Pildi valimise viga: {ex.Message}");
            await DisplayAlertAsync("Pildi viga", "Pilti ei saanud valida.", "OK");
        }
    }

    private async void OnSalvestaClicked(object? sender, EventArgs e)
    {
        string nimi = NimiEntry.Text?.Trim() ?? string.Empty;
        string kategooria = KategooriaPicker.SelectedItem?.ToString() ?? string.Empty;

        bool nimiPuudub = string.IsNullOrWhiteSpace(nimi);
        bool kategooriaPuudub = string.IsNullOrWhiteSpace(kategooria);
        bool piltPuudub = string.IsNullOrWhiteSpace(valitudPildiTee);

        MuudaViga(NimiBorder, NimiVigaLabel, nimiPuudub);
        MuudaViga(KategooriaBorder, KategooriaVigaLabel, kategooriaPuudub);
        MuudaViga(PildiBorder, PildiVigaLabel, piltPuudub);

        if (nimiPuudub || kategooriaPuudub || piltPuudub)
        {
            return;
        }

        FailiHaldur.SalvestaRetsept(new Retsept
        {
            Nimi = nimi,
            Kategooria = kategooria,
            PildiLink = valitudPildiTee
        });

        NimiEntry.Text = string.Empty;
        KategooriaPicker.SelectedIndex = -1;
        valitudPildiTee = string.Empty;
        PildiEelvaade.Source = null;
        PildiNimiLabel.Text = "Pilt valimata";
        MuudaViga(NimiBorder, NimiVigaLabel, false);
        MuudaViga(KategooriaBorder, KategooriaVigaLabel, false);
        MuudaViga(PildiBorder, PildiVigaLabel, false);

        await DisplayAlertAsync("Salvestatud", "Retsept lisati nimekirja.", "OK");
    }

    private void OnNimiChanged(object? sender, TextChangedEventArgs e)
    {
        MuudaViga(NimiBorder, NimiVigaLabel, string.IsNullOrWhiteSpace(e.NewTextValue));
    }

    private void OnKategooriaChanged(object? sender, EventArgs e)
    {
        bool kategooriaPuudub = KategooriaPicker.SelectedItem is null;
        MuudaViga(KategooriaBorder, KategooriaVigaLabel, kategooriaPuudub);
    }

    private static void MuudaViga(Border border, Label label, bool naitaViga)
    {
        border.Stroke = naitaViga ? VeaStroke : TavalineStroke;
        label.IsVisible = naitaViga;
    }
}

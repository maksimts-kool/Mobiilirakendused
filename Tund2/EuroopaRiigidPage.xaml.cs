using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Tund2;

public partial class EuroopaRiigidPage : ContentPage
{
    public ObservableCollection<Riik> Riigid => riigid;

    private readonly ObservableCollection<Riik> riigid = new();
    private readonly string riigidFailiTee = Path.Combine(FileSystem.Current.AppDataDirectory, "riigid.json");
    private bool andmedLaetud;
    private Riik? valitudRiik;

    public EuroopaRiigidPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (andmedLaetud)
        {
            return;
        }

        andmedLaetud = true;
        await LaeRiigidAsync();
    }

    private async Task LaeRiigidAsync()
    {
        if (File.Exists(riigidFailiTee))
        {
            try
            {
                await using var voog = File.OpenRead(riigidFailiTee);
                var salvestatudRiigid = await JsonSerializer.DeserializeAsync<List<Riik>>(voog);

                if (salvestatudRiigid is { Count: > 0 })
                {
                    AsendaRiigid(salvestatudRiigid);
                    return;
                }
            }
            catch
            {
                // Kui lugemine ebaonnestub, laadime vaikimisi andmed.
            }
        }

        AsendaRiigid(LooVaikimisiRiigid());
        await SalvestaRiigidAsync();
    }

    private List<Riik> LooVaikimisiRiigid()
    {
        return
        [
            new Riik
        {
            Nimi = "Eesti",
            Pealinn = "Tallinn",
            Rahvaarv = 1365884,
            OnEuroopaLiidus = true,
            Lipp = "estonia.png"
        },

            new Riik
        {
            Nimi = "Soome",
            Pealinn = "Helsingi",
            Rahvaarv = 5609057,
            OnEuroopaLiidus = true,
            Lipp = "finland.png"
        },

            new Riik
        {
            Nimi = "Rootsi",
            Pealinn = "Stockholm",
            Rahvaarv = 10536265,
            OnEuroopaLiidus = true,
            Lipp = "sweden.png"
        },

            new Riik
        {
            Nimi = "Läti",
            Pealinn = "Riia",
            Rahvaarv = 1883008,
            OnEuroopaLiidus = true,
            Lipp = "latvia.png"
        }
        ];
    }

    private void AsendaRiigid(IEnumerable<Riik> uuedRiigid)
    {
        riigid.Clear();
        foreach (var riik in uuedRiigid)
        {
            riigid.Add(riik);
        }
    }

    private async Task SalvestaRiigidAsync()
    {
        await using var voog = File.Create(riigidFailiTee);
        await JsonSerializer.SerializeAsync(voog, riigid, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        if (!TryGetRiikFromFields(out var uusRiik, out var veateade))
        {
            await DisplayAlertAsync("Viga", veateade, "OK");
            return;
        }

        var olemas = riigid.Any(r => r.Nimi.Equals(uusRiik.Nimi, StringComparison.OrdinalIgnoreCase));
        if (olemas)
        {
            await DisplayAlertAsync("Viga", "See riik on juba andmebaasis!", "OK");
            return;
        }

        riigid.Add(uusRiik);
        ValiRiikMuutmiseks(uusRiik);
        await SalvestaRiigidAsync();

        await DisplayAlertAsync("Valmis", $"Riik \"{uusRiik.Nimi}\" lisati nimekirja.", "OK");
    }

    private async void OnPickFlagImageClicked(object? sender, EventArgs e)
    {
        try
        {
            var tulemused = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Vali riigi lipu pilt"
            });

            var tulemus = tulemused?.FirstOrDefault();

            if (tulemus is null)
            {
                return;
            }

            await SalvestaValitudLipuPiltAsync(tulemus);
        }
        catch (FeatureNotSupportedException)
        {
            var tulemus = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Galerii pole saadaval, vali pildifail",
                FileTypes = FilePickerFileType.Images
            });

            if (tulemus is null)
            {
                return;
            }

            await SalvestaValitudLipuPiltAsync(tulemus);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Viga", $"Pildi valimine ebaonnestus: {ex.Message}", "OK");
        }
    }

    private async void OnSaveChangesClicked(object? sender, EventArgs e)
    {
        if (valitudRiik is null)
        {
            await DisplayAlertAsync("Viga", "Vali enne nimekirjast riik, mida soovid muuta.", "OK");
            return;
        }

        if (!TryGetRiikFromFields(out var muudetudRiik, out var veateade))
        {
            await DisplayAlertAsync("Viga", veateade, "OK");
            return;
        }

        var duplikaat = riigid.Any(r =>
            !ReferenceEquals(r, valitudRiik) &&
            r.Nimi.Equals(muudetudRiik.Nimi, StringComparison.OrdinalIgnoreCase));

        if (duplikaat)
        {
            await DisplayAlertAsync("Viga", "Sellise nimega riik on juba olemas!", "OK");
            return;
        }

        valitudRiik.Nimi = muudetudRiik.Nimi;
        valitudRiik.Pealinn = muudetudRiik.Pealinn;
        valitudRiik.Rahvaarv = muudetudRiik.Rahvaarv;
        valitudRiik.OnEuroopaLiidus = muudetudRiik.OnEuroopaLiidus;
        valitudRiik.Lipp = muudetudRiik.Lipp;

        KuvastaValitudRiik(valitudRiik);
        await SalvestaRiigidAsync();

        await DisplayAlertAsync("Salvestatud", "Riigi andmed on uuendatud.", "OK");
    }

    private void OnLippEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        UuendaLipuEelvaade(e.NewTextValue);
    }

    private async void OnEditCountryClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: Riik riik })
        {
            return;
        }

        ValiRiikMuutmiseks(riik);

        await DisplayAlertAsync(
            "Muuda riiki",
            $"Riigi \"{riik.Nimi}\" andmed toodi vormi. Tee muudatused ja vajuta \"Uuenda valitud\".",
            "OK");
    }

    private async void OnDeleteCountryClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: Riik riik })
        {
            return;
        }

        await KustutaRiikAsync(riik);
    }

    private async Task KustutaRiikAsync(Riik riik)
    {
        var kasKustutada = await DisplayAlertAsync(
            "Kustuta riik",
            $"Kas soovid riigi \"{riik.Nimi}\" kustutada?",
            "Jah",
            "Ei");

        if (!kasKustutada)
        {
            return;
        }

        riigid.Remove(riik);

        if (ReferenceEquals(valitudRiik, riik))
        {
            valitudRiik = null;
            TuhjendaValjad();
        }

        await SalvestaRiigidAsync();
    }

    private async void OnClearClicked(object? sender, EventArgs e)
    {
        valitudRiik = null;
        TuhjendaValjad();
    }

    private bool TryGetRiikFromFields(out Riik riik, out string veateade)
    {
        var nimi = NimiEntry.Text?.Trim() ?? string.Empty;
        var pealinn = PealinnEntry.Text?.Trim() ?? string.Empty;
        var lipp = LippEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nimi) ||
            string.IsNullOrWhiteSpace(pealinn) ||
            string.IsNullOrWhiteSpace(lipp))
        {
            riik = new Riik();
            veateade = "Palun taida nimi, pealinn ja vali lipu pilt.";
            return false;
        }

        if (OnKohalikFailitee(lipp) && !File.Exists(lipp))
        {
            riik = new Riik();
            veateade = "Valitud lipu pildifaili ei leitud enam.";
            return false;
        }

        if (!int.TryParse(RahvaarvEntry.Text?.Trim(), out var rahvaarv) || rahvaarv <= 0)
        {
            riik = new Riik();
            veateade = "Rahvaarv peab olema positiivne täisarv.";
            return false;
        }

        riik = new Riik
        {
            Nimi = nimi,
            Pealinn = pealinn,
            Rahvaarv = rahvaarv,
            OnEuroopaLiidus = EuSwitch.IsToggled,
            Lipp = lipp
        };
        veateade = string.Empty;
        return true;
    }

    private void KuvastaValitudRiik(Riik riik)
    {
        NimiEntry.Text = riik.Nimi;
        PealinnEntry.Text = riik.Pealinn;
        RahvaarvEntry.Text = riik.Rahvaarv.ToString();
        LippEntry.Text = riik.Lipp;
        EuSwitch.IsToggled = riik.OnEuroopaLiidus;
    }

    private void ValiRiikMuutmiseks(Riik riik)
    {
        valitudRiik = riik;
        KuvastaValitudRiik(riik);
    }

    private void TuhjendaValjad()
    {
        NimiEntry.Text = string.Empty;
        PealinnEntry.Text = string.Empty;
        RahvaarvEntry.Text = string.Empty;
        LippEntry.Text = string.Empty;
        EuSwitch.IsToggled = false;
    }

    private static bool OnKohalikFailitee(string failitee)
    {
        return Path.IsPathRooted(failitee) ||
               failitee.Contains(Path.DirectorySeparatorChar) ||
               failitee.Contains(Path.AltDirectorySeparatorChar);
    }

    private async Task SalvestaValitudLipuPiltAsync(FileResult tulemus)
    {
        var laiend = Path.GetExtension(tulemus.FileName);
        if (string.IsNullOrWhiteSpace(laiend))
        {
            laiend = ".png";
        }

        var lipudKaust = Path.Combine(FileSystem.Current.AppDataDirectory, "riigid");
        Directory.CreateDirectory(lipudKaust);

        var salvestatudFail = Path.Combine(lipudKaust, $"{Guid.NewGuid()}{laiend}");

        await using var sisend = await tulemus.OpenReadAsync();
        await using var valjund = File.Create(salvestatudFail);
        await sisend.CopyToAsync(valjund);

        LippEntry.Text = salvestatudFail;
    }

    private void UuendaLipuEelvaade(string? lipuAsukoht)
    {
        LippPreviewImage.Source = null;

        if (string.IsNullOrWhiteSpace(lipuAsukoht))
        {
            return;
        }

        if (OnKohalikFailitee(lipuAsukoht) && !File.Exists(lipuAsukoht))
        {
            return;
        }

        LippPreviewImage.Source = ImageSource.FromFile(lipuAsukoht);
    }
}

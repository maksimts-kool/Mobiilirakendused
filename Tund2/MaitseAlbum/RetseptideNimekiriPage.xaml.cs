namespace Tund2;

public partial class RetseptideNimekiriPage : ContentPage
{
    public RetseptideNimekiriPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LaeRetseptid();
    }

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

    private async void OnKustutaClicked(object? sender, EventArgs e)
    {
        if ((sender as MenuItem)?.CommandParameter is not Retsept valitudRetsept)
        {
            return;
        }

        var retseptid = FailiHaldur.LoeRetseptid();
        var kustutatavRetsept = retseptid.FirstOrDefault(retsept =>
            retsept.Nimi == valitudRetsept.Nimi &&
            retsept.Kategooria == valitudRetsept.Kategooria &&
            retsept.PildiLink == valitudRetsept.PildiLink);

        if (kustutatavRetsept is not null)
        {
            retseptid.Remove(kustutatavRetsept);
            FailiHaldur.SalvestaKoikRetseptid(retseptid);

            try
            {
                if (File.Exists(valitudRetsept.PildiLink) &&
                    valitudRetsept.PildiLink.StartsWith(FileSystem.AppDataDirectory, StringComparison.Ordinal))
                {
                    File.Delete(valitudRetsept.PildiLink);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pildi kustutamise viga: {ex.Message}");
            }
        }

        LaeRetseptid();
    }
}

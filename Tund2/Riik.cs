using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Tund2;

public class Riik : INotifyPropertyChanged
{
    private string nimi = string.Empty;
    private string pealinn = string.Empty;
    private int rahvaarv;
    private bool onEuroopaLiidus;
    private string lipp = string.Empty;

    public string Nimi
    {
        get => nimi;
        set => SetProperty(ref nimi, value?.Trim() ?? string.Empty);
    }

    public string Pealinn
    {
        get => pealinn;
        set => SetProperty(ref pealinn, value?.Trim() ?? string.Empty);
    }

    public int Rahvaarv
    {
        get => rahvaarv;
        set => SetProperty(ref rahvaarv, value);
    }

    public bool OnEuroopaLiidus
    {
        get => onEuroopaLiidus;
        set => SetProperty(ref onEuroopaLiidus, value);
    }

    public string Lipp
    {
        get => lipp;
        set => SetProperty(ref lipp, NormaliseeriLipuFailinimi(value));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private static string NormaliseeriLipuFailinimi(string? value)
    {
        var failinimi = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(failinimi))
        {
            return string.Empty;
        }

        if (Path.IsPathRooted(failinimi) ||
            failinimi.Contains(Path.DirectorySeparatorChar) ||
            failinimi.Contains(Path.AltDirectorySeparatorChar))
        {
            return failinimi;
        }

        return Path.GetExtension(failinimi).Equals(".svg", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(failinimi, ".png")
            : failinimi;
    }
}

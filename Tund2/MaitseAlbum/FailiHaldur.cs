namespace Tund2;

public static class FailiHaldur
{
    private static readonly string FailiTee = Path.Combine(FileSystem.AppDataDirectory, "retseptid.txt");

    public static List<Retsept> LoeRetseptid()
    {
        var nimekiri = new List<Retsept>();

        if (!File.Exists(FailiTee))
        {
            return nimekiri;
        }

        string[] read = File.ReadAllLines(FailiTee);

        foreach (string rida in read)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rida))
                {
                    continue;
                }

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Viga retsepti lugemisel: {ex.Message}");
            }
        }

        return nimekiri;
    }

    public static void SalvestaRetsept(Retsept retsept)
    {
        string rida = $"{PuhastaTekst(retsept.Nimi)};{PuhastaTekst(retsept.Kategooria)};{PuhastaTekst(retsept.PildiLink)}";
        File.AppendAllText(FailiTee, rida + Environment.NewLine);
    }

    public static void SalvestaKoikRetseptid(List<Retsept> retseptid)
    {
        var read = retseptid.Select(retsept =>
            $"{PuhastaTekst(retsept.Nimi)};{PuhastaTekst(retsept.Kategooria)};{PuhastaTekst(retsept.PildiLink)}");

        File.WriteAllLines(FailiTee, read);
    }

    private static string PuhastaTekst(string tekst)
    {
        return tekst.Replace(";", ",").Replace("\r", " ").Replace("\n", " ").Trim();
    }
}

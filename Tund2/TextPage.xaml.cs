using Microsoft.Maui.Controls;

namespace Tund2;

public partial class TextPage : ContentPage
{
	Label lbl;
	Editor ed;
	HorizontalStackLayout ha;
	VerticalStackLayout va;
	Button räägiNupp;

	List<string> nupud = new List<string>() { "Tagasi", "Avaleht", "Edasi" };

	public TextPage(int i)
	{
		InitializeComponent();
		Title = "Teksti ja TTS leht";

		lbl = new Label()
		{
			Text = $"Pealkiri {i}",
			FontFamily = "NunitoSansRegular",
			FontAttributes = FontAttributes.Bold,
			FontSize = 30,
			HorizontalOptions = LayoutOptions.Center
		};

		ed = new Editor()
		{
			Placeholder = "Kirjuta siia midagi...",
			FontFamily = "NunitoSansItalic",
			FontSize = 20,
			HorizontalOptions = LayoutOptions.Fill,
			HeightRequest = 100
		};

		räägiNupp = new Button()
		{
			Text = "Kuula teksti",
			FontFamily = "NunitoSansRegular",
			FontSize = 20,
			BackgroundColor = Colors.Orange,
			TextColor = Colors.White
		};
		räägiNupp.Clicked += Loe_Tekst;

		ha = new HorizontalStackLayout() { HorizontalOptions = LayoutOptions.Center, Spacing = 20 };

		for (int j = 0; j < nupud.Count; j++)
		{
			Button nupp = new Button() { Text = nupud[j], FontFamily = "NunitoSansRegular", FontSize = 20 };
			if (nupud[j] == "Edasi")
			{
				nupp.IsEnabled = false;
			}
			ha.Children.Add(nupp);

			nupp.Clicked += (s, e) =>
			{
				if (nupp.Text == "Tagasi")
					Navigation.PopAsync();
				else if (nupp.Text == "Avaleht")
					Navigation.PopToRootAsync();
				else if (nupp.Text.Contains("Edasi"))
					Navigation.PushAsync(new TextPage(i + 1));
			};
		}

		va = new VerticalStackLayout()
		{
			Children = { lbl, ed, räägiNupp, ha },
			Spacing = 20,
			Padding = new Thickness(30),
			VerticalOptions = LayoutOptions.Start
		};

		ed.TextChanged += (s, e) =>
		{
			lbl.Text = ed.Text;

			if (ha.Children.LastOrDefault() is Button edasiNupp)
			{
				if (!string.IsNullOrEmpty(ed.Text))
				{
					edasiNupp.IsEnabled = true;
				}
				else
				{
					edasiNupp.IsEnabled = false;
				}
			}
		};

		Content = new ScrollView { Content = va };
	}

	private async void Loe_Tekst(object? sender, EventArgs e)
	{
		var text = ed.Text;

		if (string.IsNullOrWhiteSpace(text))
		{
			await DisplayAlertAsync("Viga", "Palun sisesta tekst!", "OK");
			return;
		}

		try
		{
			IEnumerable<Locale> locales = await TextToSpeech.Default.GetLocalesAsync();
			Locale? valitudKeel = locales.FirstOrDefault(l => l.Language == "et-EE") ?? locales.FirstOrDefault();

			SpeechOptions options = new SpeechOptions()
			{
				Pitch = 1.5f,
				Volume = 0.75f,
				Locale = valitudKeel
			};

			await TextToSpeech.Default.SpeakAsync(text, options);
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("TTS Viga", ex.Message, "OK");
		}
	}
}
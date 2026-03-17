namespace Tund2;

public partial class TicTacToeMenuPage : ContentPage
{
	public TicTacToeMenuPage()
	{
		InitializeComponent();
	}

	private async void OnBotGame(object? sender, TappedEventArgs e)
	{
		await Navigation.PushAsync(new TicTacToePage(botEnabled: true));
	}

	private async void OnMultiGame(object? sender, TappedEventArgs e)
	{
		await Navigation.PushAsync(new TicTacToePage(botEnabled: false));
	}

	private async void OnShowRules(object? sender, TappedEventArgs e)
	{
		await Navigation.PushAsync(new TicTacToeRulesPage());
	}

	private async void OnResetScore(object? sender, TappedEventArgs e)
	{
		bool confirm = await DisplayAlertAsync("Nulli skoor", "Kas soovid skoori nullida?", "Jah", "Ei");
		if (confirm)
		{
			Preferences.Default.Set("TTT_XWins", 0);
			Preferences.Default.Set("TTT_OWins", 0);
			await DisplayAlertAsync("Valmis", "Skoor on nullitud!", "OK");
		}
	}

	private async void OnClearData(object? sender, TappedEventArgs e)
	{
		bool confirm = await DisplayAlertAsync("Kustuta andmed", "Kas soovid kõik mänguandmed kustutada?", "Jah", "Ei");
		if (confirm)
		{
			Preferences.Default.Remove("TTT_XWins");
			Preferences.Default.Remove("TTT_OWins");
			Preferences.Default.Remove("TTT_Draws");
			await DisplayAlertAsync("Valmis", "Andmed kustutatud!", "OK");
		}
	}
}

namespace Tund2.TicTacToe;

public partial class TicTacToeWonPage : ContentPage
{
	private readonly string _winner;
	private readonly bool _isBotEnabled;

	public TicTacToeWonPage(string winner, bool isBotEnabled = false)
	{
		InitializeComponent();
		_winner = winner;
		_isBotEnabled = isBotEnabled;

		if (winner == "X")
		{
			winnerImage.Source = "crosswonlose.png";
			winnerTitle.Text = "X MÄNGIJA";
		}
		else
		{
			winnerImage.Source = "circlewonlose.png";
			winnerTitle.Text = "O MÄNGIJA";
		}
	}

	private async void OnPlayAgain(object? sender, TappedEventArgs e)
	{
		await Navigation.PushAsync(new TicTacToePage(_isBotEnabled));
	}

	private async void OnMainMenu(object? sender, TappedEventArgs e)
	{
		// Remove intermediate pages (e.g. TicTacToePage) between MenuPage and this page
		var pages = Navigation.NavigationStack.ToList();
		foreach (var page in pages)
		{
			if (page != pages[0] && page is not TicTacToeMenuPage && page != this)
				Navigation.RemovePage(page);
		}
		// Pop this page to land on TicTacToeMenuPage
		await Navigation.PopAsync();
	}
}

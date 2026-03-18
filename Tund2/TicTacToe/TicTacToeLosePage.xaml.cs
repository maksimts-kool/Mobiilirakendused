namespace Tund2.TicTacToe;

public partial class TicTacToeLosePage : ContentPage
{
	private readonly string _loser;
	private readonly bool _isBotEnabled;

	public TicTacToeLosePage(string loser, bool isBotEnabled = false)
	{
		InitializeComponent();
		_loser = loser;
		_isBotEnabled = isBotEnabled;

		if (loser == "X")
		{
			loserImage.Source = "crosswonlose.png";
			loserTitle.Text = "X MÄNGIJA";
		}
		else
		{
			loserImage.Source = "circlewonlose.png";
			loserTitle.Text = "O MÄNGIJA";
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

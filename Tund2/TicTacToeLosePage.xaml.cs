namespace Tund2;

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
		await Navigation.PopToRootAsync();
	}
}

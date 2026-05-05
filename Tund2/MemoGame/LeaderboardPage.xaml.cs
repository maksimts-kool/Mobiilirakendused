namespace Tund2.MemoGame;

public partial class LeaderboardPage : ContentPage
{
	private readonly Leaderboard leaderboard = new();

	public LeaderboardPage(Theme theme)
	{
		InitializeComponent();

		theme.Apply(this);
		ResultsCollection.ItemsSource = leaderboard
			.GetTopResults(10)
			.Select((result, index) => new LeaderboardEntry(index + 1, result))
			.ToList();
	}
}

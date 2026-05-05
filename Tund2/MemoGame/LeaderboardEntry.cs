namespace Tund2.MemoGame;

public class LeaderboardEntry
{
	public int Place { get; }
	public GameResult Result { get; }
	public string PlaceText => Place.ToString();
	public string PlayerName => Result.PlayerName;
	public int Points => Result.Points;
	public string TimeText => Result.TimeText;
	public string MovesText => Result.MovesText;
	public string MedalColor => Place switch
	{
		1 => "#FACC15",
		2 => "#CBD5E1",
		3 => "#CD7F32",
		_ => "#2F6FED"
	};

	public string MedalTextColor => Place switch
	{
		1 => "#422006",
		2 => "#1E293B",
		3 => "#FFFFFF",
		_ => "#FFFFFF"
	};

	public LeaderboardEntry(int place, GameResult result)
	{
		Place = place;
		Result = result;
	}
}

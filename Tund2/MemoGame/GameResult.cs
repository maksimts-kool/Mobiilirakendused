namespace Tund2.MemoGame;

public class GameResult
{
	public string PlayerName { get; set; } = string.Empty;
	public int Points { get; set; }
	public int Seconds { get; set; }
	public int Moves { get; set; }
	public DateTime PlayedAt { get; set; }

	public GameResult()
	{
	}

	public GameResult(string playerName, int points, int seconds, int moves)
	{
		PlayerName = playerName;
		Points = points;
		Seconds = Math.Max(1, seconds);
		Moves = moves;
		PlayedAt = DateTime.Now;
	}

	public string TimeText => $"{Seconds}s";
	public string MovesText => $"{Moves} käiku";
	public string DateText => PlayedAt.ToString("dd.MM.yyyy");
}

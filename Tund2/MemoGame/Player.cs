using Microsoft.Maui.Storage;

namespace Tund2.MemoGame;

public class Player
{
	private const string BestSecondsKey = "MemoBestSeconds";
	private const string PlayerNameKey = "MemoPlayerName";

	public string Name { get; private set; }
	public int Points { get; private set; }
	public int GamesPlayed { get; private set; }
	public int BestSeconds { get; private set; }

	public Player(string name)
	{
		Name = Preferences.Get(PlayerNameKey, name);
		BestSeconds = Preferences.Get(BestSecondsKey, 0);
	}

	public void ChangeName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return;
		}

		Name = name.Trim();
		Preferences.Set(PlayerNameKey, Name);
	}

	public void ResetRound()
	{
		Points = 0;
	}

	public void AddPoints(int points)
	{
		Points += points;
	}

	public void RemovePoint()
	{
		if (Points > 0)
		{
			Points--;
		}
	}

	public void SaveFinishedGame(int seconds)
	{
		GamesPlayed++;

		if (BestSeconds == 0 || seconds < BestSeconds)
		{
			BestSeconds = seconds;
			Preferences.Set(BestSecondsKey, BestSeconds);
		}
	}
}

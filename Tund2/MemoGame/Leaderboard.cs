using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Tund2.MemoGame;

public class Leaderboard
{
	private const string ResultsKey = "MemoLeaderboardResults";
	private const int MaxSavedResults = 20;

	public List<GameResult> Results { get; private set; }

	public Leaderboard()
	{
		Results = LoadResults();
	}

	public void AddResult(GameResult result)
	{
		var existingResult = Results.FirstOrDefault(item => HasSamePlayerName(item, result));

		if (existingResult is null)
		{
			Results.Add(result);
		}
		else if (IsBetterResult(result, existingResult))
		{
			existingResult.Points = result.Points;
			existingResult.Seconds = result.Seconds;
			existingResult.Moves = result.Moves;
			existingResult.PlayedAt = result.PlayedAt;
		}

		Results = NormalizeResults(Results)
			.Take(MaxSavedResults)
			.ToList();

		SaveResults();
	}

	public List<GameResult> GetTopResults(int count)
	{
		return SortResults(Results)
			.Take(count)
			.ToList();
	}

	private static List<GameResult> SortResults(IEnumerable<GameResult> results)
	{
		return results
			.OrderByDescending(result => result.Points)
			.ThenBy(result => result.Seconds)
			.ThenBy(result => result.Moves)
			.ToList();
	}

	private static List<GameResult> NormalizeResults(IEnumerable<GameResult> results)
	{
		return SortResults(results)
			.GroupBy(result => result.PlayerName.Trim(), StringComparer.OrdinalIgnoreCase)
			.Select(group => group.First())
			.ToList();
	}

	private static bool HasSamePlayerName(GameResult first, GameResult second)
	{
		return string.Equals(
			first.PlayerName.Trim(),
			second.PlayerName.Trim(),
			StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsBetterResult(GameResult newResult, GameResult oldResult)
	{
		if (newResult.Points != oldResult.Points)
		{
			return newResult.Points > oldResult.Points;
		}

		if (newResult.Seconds != oldResult.Seconds)
		{
			return newResult.Seconds < oldResult.Seconds;
		}

		return newResult.Moves < oldResult.Moves;
	}

	private static List<GameResult> LoadResults()
	{
		var json = Preferences.Get(ResultsKey, string.Empty);
		if (string.IsNullOrWhiteSpace(json))
		{
			return new List<GameResult>();
		}

		try
		{
			var results = JsonSerializer.Deserialize<List<GameResult>>(json) ?? new List<GameResult>();
			foreach (var result in results)
			{
				if (result.Seconds <= 0)
				{
					result.Seconds = 1;
				}
			}

			return NormalizeResults(results);
		}
		catch (JsonException)
		{
			return new List<GameResult>();
		}
	}

	private void SaveResults()
	{
		var json = JsonSerializer.Serialize(Results);
		Preferences.Set(ResultsKey, json);
	}
}

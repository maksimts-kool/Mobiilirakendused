namespace Tund2.MemoGame;

public class Game
{
	private readonly string[] cardTexts = { "A", "B", "C", "D", "E", "F" };
	private Card? firstCard;
	private DateTime startedAt;

	public Player Player { get; }
	public List<Card> Cards { get; private set; } = new();
	public int Moves { get; private set; }
	public int Seconds { get; private set; }
	public bool IsRunning { get; private set; }
	public bool IsFinished { get; private set; }

	public Game(Player player)
	{
		Player = player;
	}

	public void Start()
	{
		var cards = new List<Card>();
		var id = 1;

		foreach (var text in cardTexts)
		{
			cards.Add(new Card(id++, text, text));
			cards.Add(new Card(id++, text, text));
		}

		Cards = cards
			.OrderBy(_ => Random.Shared.Next())
			.ToList();

		firstCard = null;
		Moves = 0;
		Seconds = 0;
		startedAt = DateTime.Now;
		IsRunning = true;
		IsFinished = false;
		Player.ResetRound();
	}

	public void AddSecond()
	{
		if (IsRunning && !IsFinished)
		{
			UpdateElapsedTime();
		}
	}

	public GameTurnResult SelectCard(Card card)
	{
		if (!IsRunning || IsFinished || card.IsFaceUp || card.IsMatched)
		{
			return new GameTurnResult(GameTurnKind.Ignored, Array.Empty<Card>());
		}

		card.Reveal();

		if (firstCard is null)
		{
			firstCard = card;
			return new GameTurnResult(GameTurnKind.FirstCard, new[] { card });
		}

		Moves++;
		var previousCard = firstCard;
		firstCard = null;

		if (previousCard.PairKey == card.PairKey)
		{
			previousCard.MarkMatched();
			card.MarkMatched();
			Player.AddPoints(10);

			var finished = Cards.All(item => item.IsMatched);
			if (finished)
			{
				Finish();
			}

			return new GameTurnResult(GameTurnKind.Match, new[] { previousCard, card }, finished);
		}

		Player.RemovePoint();
		return new GameTurnResult(GameTurnKind.Mismatch, new[] { previousCard, card });
	}

	public void HideCards(IEnumerable<Card> cards)
	{
		foreach (var card in cards)
		{
			card.Hide();
		}
	}

	public void Finish()
	{
		if (IsFinished)
		{
			return;
		}

		IsRunning = false;
		IsFinished = true;
		UpdateElapsedTime();
		Player.SaveFinishedGame(Seconds);
	}

	private void UpdateElapsedTime()
	{
		var elapsedSeconds = (int)Math.Ceiling((DateTime.Now - startedAt).TotalSeconds);
		Seconds = Math.Max(1, elapsedSeconds);
	}
}

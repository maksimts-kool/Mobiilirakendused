namespace Tund2.MemoGame;

public enum GameTurnKind
{
	Ignored,
	FirstCard,
	Match,
	Mismatch
}

public class GameTurnResult
{
	public GameTurnKind Kind { get; }
	public IReadOnlyList<Card> Cards { get; }
	public bool IsGameFinished { get; }

	public GameTurnResult(GameTurnKind kind, IReadOnlyList<Card> cards, bool isGameFinished = false)
	{
		Kind = kind;
		Cards = cards;
		IsGameFinished = isGameFinished;
	}
}

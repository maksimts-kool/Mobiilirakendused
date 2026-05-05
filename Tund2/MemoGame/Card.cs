namespace Tund2.MemoGame;

public class Card
{
	public int Id { get; }
	public string PairKey { get; }
	public string Text { get; }
	public string ImageFile { get; }
	public bool IsFaceUp { get; private set; }
	public bool IsMatched { get; private set; }

	public Card(int id, string pairKey, string text, string imageFile)
	{
		Id = id;
		PairKey = pairKey;
		Text = text;
		ImageFile = imageFile;
	}

	public void Reveal()
	{
		if (!IsMatched)
		{
			IsFaceUp = true;
		}
	}

	public void Hide()
	{
		if (!IsMatched)
		{
			IsFaceUp = false;
		}
	}

	public void MarkMatched()
	{
		IsFaceUp = true;
		IsMatched = true;
	}
}

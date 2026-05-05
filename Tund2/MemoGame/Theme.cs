namespace Tund2.MemoGame;

public class Theme
{
	public string Name { get; }
	public Color BackgroundColor { get; }
	public Color TextColor { get; }
	public Color AccentColor { get; }
	public Color PanelColor { get; }
	public Color CardBackColor { get; }
	public Color CardFrontColor { get; }
	public Color SelectedStrokeColor { get; }
	public Color CorrectStrokeColor { get; }
	public Color WrongStrokeColor { get; }
	public string FontFamily { get; }

	public Theme(
		string name,
		string backgroundColor,
		string textColor,
		string accentColor,
		string panelColor,
		string cardBackColor,
		string cardFrontColor,
		string fontFamily,
		string selectedStrokeColor = "#2F6FED",
		string correctStrokeColor = "#22C55E",
		string wrongStrokeColor = "#EF4444")
	{
		Name = name;
		BackgroundColor = Color.FromArgb(backgroundColor);
		TextColor = Color.FromArgb(textColor);
		AccentColor = Color.FromArgb(accentColor);
		PanelColor = Color.FromArgb(panelColor);
		CardBackColor = Color.FromArgb(cardBackColor);
		CardFrontColor = Color.FromArgb(cardFrontColor);
		SelectedStrokeColor = Color.FromArgb(selectedStrokeColor);
		CorrectStrokeColor = Color.FromArgb(correctStrokeColor);
		WrongStrokeColor = Color.FromArgb(wrongStrokeColor);
		FontFamily = fontFamily;
	}

	public void Apply(ContentPage page)
	{
		ApplyTo(page.Resources);

		if (Application.Current?.Resources is ResourceDictionary appResources)
		{
			ApplyTo(appResources);
		}

		page.BackgroundColor = BackgroundColor;
	}

	private void ApplyTo(ResourceDictionary resources)
	{
		resources["MemoBackgroundColor"] = BackgroundColor;
		resources["MemoTextColor"] = TextColor;
		resources["MemoAccentColor"] = AccentColor;
		resources["MemoPanelColor"] = PanelColor;
		resources["MemoCardBackColor"] = CardBackColor;
		resources["MemoCardFrontColor"] = CardFrontColor;
		resources["MemoSelectedStrokeColor"] = SelectedStrokeColor;
		resources["MemoCorrectStrokeColor"] = CorrectStrokeColor;
		resources["MemoWrongStrokeColor"] = WrongStrokeColor;
		resources["MemoFontFamily"] = FontFamily;
	}

	public static List<Theme> CreateDefaultThemes()
	{
		return new List<Theme>
		{
			new("Hele", "#F4F7FB", "#18212F", "#2F6FED", "#FFFFFF", "#2F6FED", "#FFFFFF", "PoppinsMedium"),
			new("Tume", "#141821", "#F8FAFC", "#2F6FED", "#1E2430", "#334155", "#111827", "OpenSansSemibold")
		};
	}
}

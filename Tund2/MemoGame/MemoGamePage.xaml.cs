using Microsoft.Maui.Controls.Shapes;

namespace Tund2.MemoGame;

public partial class MemoGamePage : ContentPage
{
	private const int BoardColumnCount = 3;

	private readonly Player player = new("Mängija");
	private readonly Game game;
	private readonly Leaderboard leaderboard = new();
	private readonly List<Theme> themes;
	private readonly Dictionary<Card, Border> cardViews = new();
	private readonly Dictionary<Card, Label> cardLabels = new();
	private readonly Dictionary<Card, Image> cardImages = new();
	private readonly Dictionary<Card, Color> strokeOverrides = new();
	private readonly IDispatcherTimer timer;
	private Theme currentTheme;
	private bool hasStartedFirstGame;
	private bool isBusy;

	public MemoGamePage()
	{
		themes = Theme.CreateDefaultThemes();
		currentTheme = themes[0];
		game = new Game(player);

		InitializeComponent();

		ThemePicker.ItemsSource = themes;
		ThemePicker.ItemDisplayBinding = new Binding(nameof(Theme.Name));
		ThemePicker.SelectedIndex = 0;
		PlayerNameLabel.Text = $"Mängija: {player.Name}";

		timer = Dispatcher.CreateTimer();
		timer.Interval = TimeSpan.FromSeconds(1);
		timer.Tick += OnTimerTick;

		currentTheme.Apply(this);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (hasStartedFirstGame)
		{
			return;
		}

		hasStartedFirstGame = true;
		await AskPlayerNameAsync();
		StartNewGame();
	}

	protected override void OnDisappearing()
	{
		timer.Stop();
		base.OnDisappearing();
	}

	private void StartNewGame()
	{
		isBusy = false;
		game.Start();
		CreateBoard();
		UpdateStats();
		timer.Start();
	}

	private void CreateBoard()
	{
		BoardGrid.Children.Clear();
		BoardGrid.RowDefinitions.Clear();
		BoardGrid.ColumnDefinitions.Clear();
		cardViews.Clear();
		cardLabels.Clear();
		cardImages.Clear();
		strokeOverrides.Clear();

		var rowCount = (int)Math.Ceiling((double)game.Cards.Count / BoardColumnCount);

		for (var row = 0; row < rowCount; row++)
		{
			BoardGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
		}

		for (var column = 0; column < BoardColumnCount; column++)
		{
			BoardGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
		}

		for (var index = 0; index < game.Cards.Count; index++)
		{
			var card = game.Cards[index];
			var cardView = CreateCardView(card);
			var row = index / BoardColumnCount;
			var column = index % BoardColumnCount;

			BoardGrid.Add(cardView, column, row);
		}

		UpdateBoardSize();
	}

	private void OnBoardFrameSizeChanged(object? sender, EventArgs e)
	{
		UpdateBoardSize();
	}

	private void UpdateBoardSize()
	{
		if (BoardFrame.Width <= 0 || BoardFrame.Height <= 0 || game.Cards.Count == 0)
		{
			return;
		}

		var rowCount = (int)Math.Ceiling((double)game.Cards.Count / BoardColumnCount);
		var availableWidth = BoardFrame.Width - BoardFrame.Padding.Left - BoardFrame.Padding.Right;
		var availableHeight = BoardFrame.Height - BoardFrame.Padding.Top - BoardFrame.Padding.Bottom;
		var totalColumnSpacing = BoardGrid.ColumnSpacing * (BoardColumnCount - 1);
		var totalRowSpacing = BoardGrid.RowSpacing * (rowCount - 1);
		var cardWidth = (availableWidth - totalColumnSpacing) / BoardColumnCount;
		var cardHeight = (availableHeight - totalRowSpacing) / rowCount;
		var cardSize = Math.Floor(Math.Min(cardWidth, cardHeight));

		if (cardSize <= 0)
		{
			return;
		}

		BoardGrid.WidthRequest = (cardSize * BoardColumnCount) + totalColumnSpacing;
		BoardGrid.HeightRequest = (cardSize * rowCount) + totalRowSpacing;
	}

	private Border CreateCardView(Card card)
	{
		var label = new Label
		{
			Text = "?",
			FontSize = 24,
			FontAttributes = FontAttributes.Bold,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center
		};
		label.SetDynamicResource(Label.FontFamilyProperty, "MemoFontFamily");

		var image = new Image
		{
			Source = card.ImageFile,
			Aspect = Aspect.AspectFit,
			IsVisible = false,
			Margin = 12
		};

		var content = new Grid();
		content.Add(image);
		content.Add(label);

		var cardView = new Border
		{
			StrokeThickness = 2,
			StrokeShape = new RoundRectangle { CornerRadius = 14 },
			Content = content
		};

		var tap = new TapGestureRecognizer();
		tap.CommandParameter = card;
		tap.Tapped += OnCardTapped;
		cardView.GestureRecognizers.Add(tap);

		cardViews[card] = cardView;
		cardLabels[card] = label;
		cardImages[card] = image;
		UpdateCardView(card);

		return cardView;
	}

	private async void OnCardTapped(object? sender, TappedEventArgs e)
	{
		if (isBusy || e.Parameter is not Card card)
		{
			return;
		}

		var result = game.SelectCard(card);
		if (result.Kind == GameTurnKind.Ignored)
		{
			return;
		}

		await FlipCardAsync(card);
		UpdateStats();

		if (result.Kind == GameTurnKind.Match)
		{
			SetCardsStroke(result.Cards, currentTheme.CorrectStrokeColor);
			await AnimateMatchAsync(result.Cards);
			await Task.Delay(450);
			ClearCardsStroke(result.Cards);
		}

		if (result.Kind == GameTurnKind.Mismatch)
		{
			isBusy = true;
			SetCardsStroke(result.Cards, currentTheme.WrongStrokeColor);
			await Task.Delay(700);
			ClearCardsStroke(result.Cards);
			game.HideCards(result.Cards);

			foreach (var wrongCard in result.Cards)
			{
				await FlipCardAsync(wrongCard);
			}

			isBusy = false;
		}

		UpdateStats();

		if (result.IsGameFinished)
		{
			timer.Stop();
			leaderboard.AddResult(new GameResult(player.Name, player.Points, game.Seconds, game.Moves));

			await DisplayAlertAsync(
				"Mäng läbi!",
				$"{player.Name}, said {player.Points} punkti. Aeg: {game.Seconds}s. Käigud: {game.Moves}.",
				"OK");
		}
	}

	private async Task FlipCardAsync(Card card)
	{
		if (!cardViews.TryGetValue(card, out var cardView))
		{
			return;
		}

		await cardView.RotateYToAsync(90, 120, Easing.CubicIn);
		UpdateCardView(card);
		await cardView.RotateYToAsync(0, 120, Easing.CubicOut);
	}

	private async Task AnimateMatchAsync(IReadOnlyList<Card> cards)
	{
		var animations = cards
			.Where(cardViews.ContainsKey)
			.Select(async card =>
			{
				var view = cardViews[card];
				await view.ScaleToAsync(1.08, 110, Easing.CubicOut);
				await view.ScaleToAsync(1, 140, Easing.SpringOut);
			});

		await Task.WhenAll(animations);
	}

	private void UpdateCardView(Card card)
	{
		if (!cardViews.TryGetValue(card, out var cardView) ||
			!cardLabels.TryGetValue(card, out var label) ||
			!cardImages.TryGetValue(card, out var image))
		{
			return;
		}

		cardView.BackgroundColor = card.IsFaceUp ? currentTheme.CardFrontColor : currentTheme.CardBackColor;
		cardView.Stroke = strokeOverrides.TryGetValue(card, out var strokeColor)
			? strokeColor
			: currentTheme.SelectedStrokeColor;
		cardView.Opacity = card.IsMatched ? 0.75 : 1;

		label.IsVisible = !card.IsFaceUp;
		label.TextColor = Colors.White;
		image.IsVisible = card.IsFaceUp;
	}

	private void UpdateAllCards()
	{
		foreach (var card in game.Cards)
		{
			UpdateCardView(card);
		}
	}

	private void SetCardsStroke(IEnumerable<Card> cards, Color color)
	{
		foreach (var card in cards)
		{
			strokeOverrides[card] = color;
			UpdateCardView(card);
		}
	}

	private void ClearCardsStroke(IEnumerable<Card> cards)
	{
		foreach (var card in cards)
		{
			strokeOverrides.Remove(card);
			UpdateCardView(card);
		}
	}

	private void UpdateStats()
	{
		PointsLabel.Text = player.Points.ToString();
		MovesLabel.Text = game.Moves.ToString();
		TimeLabel.Text = $"{game.Seconds}s";
		BestScoreLabel.Text = player.BestSeconds == 0
			? "Parim aeg puudub"
			: $"Parim aeg: {player.BestSeconds}s";
	}

	private void OnTimerTick(object? sender, EventArgs e)
	{
		game.AddSecond();
		UpdateStats();
	}

	private void OnThemeChanged(object? sender, EventArgs e)
	{
		if (ThemePicker.SelectedItem is not Theme selectedTheme)
		{
			return;
		}

		currentTheme = selectedTheme;
		currentTheme.Apply(this);
		UpdateAllCards();
	}

	private async Task AskPlayerNameAsync()
	{
		var name = await DisplayPromptAsync(
			"Mängija nimi",
			"Sisesta nimi enne mängu alustamist:",
			"Alusta",
			"Jäta vahele",
			"Nimi");

		if (!string.IsNullOrWhiteSpace(name))
		{
			player.ChangeName(name);
		}

		PlayerNameLabel.Text = $"Mängija: {player.Name}";
	}

	private async void OnRestartClicked(object? sender, EventArgs e)
	{
		timer.Stop();
		await AskPlayerNameAsync();
		StartNewGame();
	}

	private async void OnLeaderboardClicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new LeaderboardPage(currentTheme));
	}
}

namespace Tund2;

public partial class TicTacToePage : ContentPage
{
	private GameLogic game;
	private Button[,] boardButtons = new Button[3, 3];

	// Skoor
	private int xWins, oWins, draws;

	// Teema
	private bool isDarkTheme = false;

	// Bot
	private bool isBotEnabled = false;
	private bool isBotThinking = false;

	// Seaded (rakendatakse "Uus mäng" vajutamisel)
	private string selectedFirstPlayer = "X";
	private int selectedBoardSize = 3;

	// Värvid
	private Color bgLight = Color.FromArgb("#f0f0f5");
	private Color bgDark = Color.FromArgb("#1a1a2e");
	private Color cardLight = Color.FromArgb("#ffffff");
	private Color cardDark = Color.FromArgb("#16213e");
	private Color textLight = Color.FromArgb("#2f3542");
	private Color textDark = Color.FromArgb("#e0e0e0");
	private Color xColor = Color.FromArgb("#e74c3c");
	private Color oColor = Color.FromArgb("#3498db");

	public TicTacToePage()
	{
		InitializeComponent();
		LoadScore();
		game = new GameLogic(3);
		scoreLabel.Text = GetScoreText();
		BuildBoard(3);
		UpdateFirstPlayerButtons();
	}

	// ===== Mänguvälja ehitamine =====

	private void BuildBoard(int size)
	{
		boardGrid.Children.Clear();
		boardGrid.RowDefinitions.Clear();
		boardGrid.ColumnDefinitions.Clear();

		boardButtons = new Button[size, size];

		int cellSize = size <= 3 ? 90 : (size == 4 ? 72 : 60);
		int fontSize = size <= 3 ? 36 : (size == 4 ? 28 : 22);

		for (int i = 0; i < size; i++)
		{
			boardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(cellSize) });
			boardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(cellSize) });
		}

		for (int row = 0; row < size; row++)
		{
			for (int col = 0; col < size; col++)
			{
				var btn = new Button
				{
					Text = "",
					FontSize = fontSize,
					FontAttributes = FontAttributes.Bold,
					WidthRequest = cellSize,
					HeightRequest = cellSize,
					CornerRadius = 12,
					BackgroundColor = isDarkTheme ? cardDark : cardLight,
					TextColor = textLight,
					BorderWidth = 2,
					BorderColor = isDarkTheme ? Color.FromArgb("#2a2a4a") : Color.FromArgb("#d0d0d8"),
					Shadow = new Shadow
					{
						Brush = Brush.Black,
						Offset = new Point(0, 2),
						Opacity = 0.15f,
						Radius = 4
					}
				};

				int r = row, c = col;
				btn.Clicked += async (s, e) => await OnCellClicked(r, c);

				boardButtons[row, col] = btn;
				boardGrid.Add(btn, col, row);
			}
		}
	}

	// ===== Lahtri vajutus =====

	private async Task OnCellClicked(int row, int col, bool isBot = false)
	{
		if (game.IsGameOver) return;
		if (isBotThinking && !isBot) return;

		string player = game.CurrentPlayer;
		if (!game.MakeMove(row, col)) return;

		UpdateSettingsEnabled();

		var btn = boardButtons[row, col];
		btn.Text = player;
		btn.TextColor = player == "X" ? xColor : oColor;

		// Skaala-animatsioon
		await btn.ScaleToAsync(1.2, 80);
		await btn.ScaleToAsync(1.0, 80);

		// Kontrolli võitjat
		string? winner = game.CheckWinner();
		if (winner != null)
		{
			game.EndGame();
			if (winner == "X") xWins++;
			else oWins++;
			SaveScore();
			scoreLabel.Text = GetScoreText();
			statusLabel.Text = $"Mängija {winner} võitis!";

			// Kaotaja alustab järgmist mängu
			selectedFirstPlayer = winner == "X" ? "O" : "X";
			UpdateFirstPlayerButtons();

			await AnimateWinningCells();
			UpdateSettingsEnabled();
			await DisplayAlertAsync("Mäng läbi!", $"Mängija {winner} võitis!", "OK");
			return;
		}

		if (game.IsBoardFull())
		{
			game.EndGame();
			draws++;
			SaveScore();
			scoreLabel.Text = GetScoreText();
			statusLabel.Text = "Viik!";
			UpdateSettingsEnabled();
			await DisplayAlertAsync("Mäng läbi!", "Viik!", "OK");
			return;
		}

		game.SwitchPlayer();
		statusLabel.Text = $"Mängija {game.CurrentPlayer} käik";

		// Bot käik
		if (isBotEnabled && game.CurrentPlayer == "O" && !game.IsGameOver)
		{
			isBotThinking = true;
			await Task.Delay(300);
			await BotMove();
			isBotThinking = false;
		}
	}

	// ===== Bot loogika =====

	private async Task BotMove()
	{
		var (row, col) = GetBotMove();
		await OnCellClicked(row, col, isBot: true);
	}

	private (int Row, int Col) GetBotMove()
	{
		int n = game.BoardSize;
		int winLength = n <= 3 ? n : 4;

		// 1. Proovi võita
		var winMove = FindWinningMove("O", winLength);
		if (winMove != null) return winMove.Value;

		// 2. Blokeeri vastane
		var blockMove = FindWinningMove("X", winLength);
		if (blockMove != null) return blockMove.Value;

		// 3. Keskmine ruut
		int center = n / 2;
		if (string.IsNullOrEmpty(game.Board[center, center]))
			return (center, center);

		// 4. Nurgad
		int[][] corners = { new[] { 0, 0 }, new[] { 0, n - 1 }, new[] { n - 1, 0 }, new[] { n - 1, n - 1 } };
		var rng = new Random();
		var shuffled = corners.OrderBy(_ => rng.Next()).ToArray();
		foreach (var c in shuffled)
			if (string.IsNullOrEmpty(game.Board[c[0], c[1]]))
				return (c[0], c[1]);

		// 5. Juhuslik tühi ruut
		var empty = new List<(int, int)>();
		for (int r = 0; r < n; r++)
			for (int c = 0; c < n; c++)
				if (string.IsNullOrEmpty(game.Board[r, c]))
					empty.Add((r, c));

		return empty[rng.Next(empty.Count)];
	}

	private (int Row, int Col)? FindWinningMove(string player, int winLength)
	{
		int n = game.BoardSize;
		for (int r = 0; r < n; r++)
			for (int c = 0; c < n; c++)
			{
				if (!string.IsNullOrEmpty(game.Board[r, c])) continue;

				// Simuleeri käik
				game.Board[r, c] = player;
				bool wins = game.CheckWinner() == player;
				game.Board[r, c] = "";

				if (wins) return (r, c);
			}
		return null;
	}

	// ===== Võidurida animatsioon =====

	private async Task AnimateWinningCells()
	{
		var cells = game.GetWinningCells();
		if (cells == null) return;

		for (int flash = 0; flash < 3; flash++)
		{
			foreach (var (row, col) in cells)
				boardButtons[row, col].BackgroundColor = Colors.Gold;
			await Task.Delay(250);

			foreach (var (row, col) in cells)
				boardButtons[row, col].BackgroundColor = isDarkTheme ? cardDark : cardLight;
			await Task.Delay(250);
		}

		foreach (var (row, col) in cells)
			boardButtons[row, col].BackgroundColor = Colors.Gold;
	}

	// ===== Nuppude sündmused =====

	private async void OnNewGame(object? sender, EventArgs e)
	{
		// Rakenda seaded
		if (selectedBoardSize != game.BoardSize)
		{
			game.SetBoardSize(selectedBoardSize);
			BuildBoard(selectedBoardSize);
			ApplyTheme();
		}
		else
		{
			game.ResetBoard();
		}

		game.SetStartingPlayer(selectedFirstPlayer);
		RefreshBoard();
		UpdateSettingsEnabled();
		statusLabel.Text = $"Mängija {selectedFirstPlayer} käik";

		await statusLabel.ScaleToAsync(1.2, 100);
		await statusLabel.ScaleToAsync(1.0, 100);

		// Kui bot on sees ja O alustab
		if (isBotEnabled && selectedFirstPlayer == "O")
		{
			isBotThinking = true;
			await Task.Delay(300);
			await BotMove();
			isBotThinking = false;
		}
	}

	private void OnSelectFirstX(object? sender, EventArgs e)
	{
		if (game.IsGameInProgress()) return;
		selectedFirstPlayer = "X";
		UpdateFirstPlayerButtons();
	}

	private void OnSelectFirstO(object? sender, EventArgs e)
	{
		if (game.IsGameInProgress()) return;
		selectedFirstPlayer = "O";
		UpdateFirstPlayerButtons();
	}

	private void UpdateFirstPlayerButtons()
	{
		firstXBtn.BackgroundColor = selectedFirstPlayer == "X" ? Color.FromArgb("#e74c3c") : Color.FromArgb("#95a5a6");
		firstOBtn.BackgroundColor = selectedFirstPlayer == "O" ? Color.FromArgb("#3498db") : Color.FromArgb("#95a5a6");
	}

	private async void OnShowRules(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new TicTacToeRulesPage());
	}

	private void OnToggleTheme(object? sender, EventArgs e)
	{
		isDarkTheme = !isDarkTheme;
		ApplyTheme();
	}

	private void OnToggleBot(object? sender, EventArgs e)
	{
		if (game.IsGameInProgress()) return;
		isBotEnabled = !isBotEnabled;
		botButton.Text = isBotEnabled ? "Sees" : "Väljas";
		botButton.BackgroundColor = isBotEnabled ? Color.FromArgb("#27ae60") : Color.FromArgb("#7f8c8d");
	}

	private void OnSize3(object? sender, EventArgs e) => SelectBoardSize(3);
	private void OnSize4(object? sender, EventArgs e) => SelectBoardSize(4);
	private void OnSize5(object? sender, EventArgs e) => SelectBoardSize(5);

	private void SelectBoardSize(int newSize)
	{
		if (game.IsGameInProgress()) return;
		selectedBoardSize = newSize;
		sizeBtn3.BackgroundColor = newSize == 3 ? Color.FromArgb("#1abc9c") : Color.FromArgb("#95a5a6");
		sizeBtn4.BackgroundColor = newSize == 4 ? Color.FromArgb("#1abc9c") : Color.FromArgb("#95a5a6");
		sizeBtn5.BackgroundColor = newSize == 5 ? Color.FromArgb("#1abc9c") : Color.FromArgb("#95a5a6");
	}

	private void OnResetScore(object? sender, EventArgs e)
	{
		xWins = 0;
		oWins = 0;
		draws = 0;
		SaveScore();
		scoreLabel.Text = GetScoreText();
	}

	// ===== Abimeetodid =====

	private void UpdateSettingsEnabled()
	{
		bool enabled = !game.IsGameInProgress();
		firstXBtn.IsEnabled = enabled;
		firstOBtn.IsEnabled = enabled;
		botButton.IsEnabled = enabled;
		sizeBtn3.IsEnabled = enabled;
		sizeBtn4.IsEnabled = enabled;
		sizeBtn5.IsEnabled = enabled;

		firstXBtn.Opacity = enabled ? 1.0 : 0.5;
		firstOBtn.Opacity = enabled ? 1.0 : 0.5;
		botButton.Opacity = enabled ? 1.0 : 0.5;
		sizeBtn3.Opacity = enabled ? 1.0 : 0.5;
		sizeBtn4.Opacity = enabled ? 1.0 : 0.5;
		sizeBtn5.Opacity = enabled ? 1.0 : 0.5;
	}

	private void RefreshBoard()
	{
		int size = game.BoardSize;
		for (int row = 0; row < size; row++)
			for (int col = 0; col < size; col++)
			{
				boardButtons[row, col].Text = "";
				boardButtons[row, col].BackgroundColor = isDarkTheme ? cardDark : cardLight;
			}
	}

	private void ApplyTheme()
	{
		Color bg = isDarkTheme ? bgDark : bgLight;
		Color text = isDarkTheme ? textDark : textLight;
		Color card = isDarkTheme ? cardDark : cardLight;
		Color borderStroke = isDarkTheme ? Color.FromArgb("#2a2a4a") : Color.FromArgb("#d0d0d8");

		BackgroundColor = bg;

		// Title
		titleLabel.TextColor = text;

		// Status
		statusBorder.BackgroundColor = isDarkTheme ? Color.FromArgb("#2a2a4a") : Color.FromArgb("#e8e8f0");
		statusLabel.TextColor = text;

		// Score
		scoreBorder.BackgroundColor = card;
		scoreBorder.Stroke = isDarkTheme ? Color.FromArgb("#2a2a4a") : Color.FromArgb("#ddd");
		scoreLabel.TextColor = text;

		// Board border
		boardBorder.BackgroundColor = isDarkTheme ? Color.FromArgb("#0f3460") : Color.FromArgb("#e4e4ec");
		boardBorder.Stroke = borderStroke;

		// Settings border
		settingsBorder.BackgroundColor = card;
		settingsBorder.Stroke = borderStroke;

		// Settings labels
		firstLabel.TextColor = text;
		botLabel.TextColor = text;
		sizeLabel.TextColor = text;

		// Reset score button
		resetScoreBtn.BackgroundColor = isDarkTheme ? Color.FromArgb("#4a4a6a") : Color.FromArgb("#bdc3c7");
		resetScoreBtn.TextColor = text;

		// Board cells
		int size = game.BoardSize;
		for (int row = 0; row < size; row++)
			for (int col = 0; col < size; col++)
			{
				var btn = boardButtons[row, col];
				if (btn.BackgroundColor != Colors.Gold)
					btn.BackgroundColor = card;
				btn.BorderColor = borderStroke;
			}
	}

	private string GetScoreText() => $"X: {xWins}  |  O: {oWins}  |  Viik: {draws}";

	private void LoadScore()
	{
		xWins = Preferences.Default.Get("TTT_XWins", 0);
		oWins = Preferences.Default.Get("TTT_OWins", 0);
		draws = Preferences.Default.Get("TTT_Draws", 0);
	}

	private void SaveScore()
	{
		Preferences.Default.Set("TTT_XWins", xWins);
		Preferences.Default.Set("TTT_OWins", oWins);
		Preferences.Default.Set("TTT_Draws", draws);
	}

	// ===== Mänguloogika klass (OOP) =====

	public class GameLogic
	{
		public int BoardSize { get; private set; }
		public string[,] Board { get; private set; }
		public string CurrentPlayer { get; private set; }
		public bool IsGameOver { get; private set; }

		public GameLogic(int boardSize = 3)
		{
			BoardSize = boardSize;
			Board = new string[boardSize, boardSize];
			CurrentPlayer = "X";
			IsGameOver = false;
		}

		public bool MakeMove(int row, int col)
		{
			if (IsGameOver || !string.IsNullOrEmpty(Board[row, col]))
				return false;
			Board[row, col] = CurrentPlayer;
			return true;
		}

		public void SwitchPlayer()
		{
			CurrentPlayer = CurrentPlayer == "X" ? "O" : "X";
		}

		public string? CheckWinner()
		{
			int n = BoardSize;
			int winLength = n <= 3 ? n : 4;

			for (int row = 0; row < n; row++)
			{
				for (int col = 0; col < n; col++)
				{
					string cell = Board[row, col];
					if (string.IsNullOrEmpty(cell)) continue;

					if (col + winLength <= n && CheckLine(row, col, 0, 1, winLength, cell))
						return cell;
					if (row + winLength <= n && CheckLine(row, col, 1, 0, winLength, cell))
						return cell;
					if (row + winLength <= n && col + winLength <= n && CheckLine(row, col, 1, 1, winLength, cell))
						return cell;
					if (row + winLength <= n && col - winLength + 1 >= 0 && CheckLine(row, col, 1, -1, winLength, cell))
						return cell;
				}
			}
			return null;
		}

		private bool CheckLine(int startRow, int startCol, int dRow, int dCol, int length, string player)
		{
			for (int i = 0; i < length; i++)
				if (Board[startRow + i * dRow, startCol + i * dCol] != player)
					return false;
			return true;
		}

		public bool IsBoardFull()
		{
			for (int row = 0; row < BoardSize; row++)
				for (int col = 0; col < BoardSize; col++)
					if (string.IsNullOrEmpty(Board[row, col]))
						return false;
			return true;
		}

		public bool IsGameInProgress()
		{
			if (IsGameOver) return false;
			for (int row = 0; row < BoardSize; row++)
				for (int col = 0; col < BoardSize; col++)
					if (!string.IsNullOrEmpty(Board[row, col]))
						return true;
			return false;
		}

		public void ResetBoard()
		{
			Board = new string[BoardSize, BoardSize];
			CurrentPlayer = "X";
			IsGameOver = false;
		}

		public void SetBoardSize(int size)
		{
			BoardSize = size;
			ResetBoard();
		}

		public void SetStartingPlayer(string player)
		{
			CurrentPlayer = player;
		}

		public void EndGame()
		{
			IsGameOver = true;
		}

		public List<(int Row, int Col)>? GetWinningCells()
		{
			int n = BoardSize;
			int winLength = n <= 3 ? n : 4;

			for (int row = 0; row < n; row++)
			{
				for (int col = 0; col < n; col++)
				{
					string cell = Board[row, col];
					if (string.IsNullOrEmpty(cell)) continue;

					int[][] directions = { new[] { 0, 1 }, new[] { 1, 0 }, new[] { 1, 1 }, new[] { 1, -1 } };
					foreach (var dir in directions)
					{
						int endRow = row + (winLength - 1) * dir[0];
						int endCol = col + (winLength - 1) * dir[1];
						if (endRow < 0 || endRow >= n || endCol < 0 || endCol >= n) continue;

						if (CheckLine(row, col, dir[0], dir[1], winLength, cell))
						{
							var cells = new List<(int, int)>();
							for (int i = 0; i < winLength; i++)
								cells.Add((row + i * dir[0], col + i * dir[1]));
							return cells;
						}
					}
				}
			}
			return null;
		}
	}
}

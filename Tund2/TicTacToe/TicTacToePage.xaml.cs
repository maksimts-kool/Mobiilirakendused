using Microsoft.Maui.Controls.Shapes;

namespace Tund2.TicTacToe;

public partial class TicTacToePage : ContentPage
{
	private const string PlayerX = "X";
	private const string PlayerO = "O";

	private GameLogic game;
	private Border[,] boardCells = new Border[3, 3];
	private Image[,] boardSymbols = new Image[3, 3];
	private readonly Random random = new();

	// Skoor
	private int xWins, oWins;

	// Bot
	private bool isBotEnabled = false;
	private bool isBotThinking = false;
	private string botDifficulty = "Medium";

	// Värvid
	private Color xBgColor = Color.FromArgb("#6E00B8");
	private Color oBgColor = Color.FromArgb("#FF8000");
	private Color emptyCellBg = Colors.White;

	public TicTacToePage()
	{
		InitializeComponent();
		LoadScore();
		int boardSize = Preferences.Default.Get("TTT_BoardSize", 3);
		botDifficulty = Preferences.Default.Get("TTT_BotDifficulty", "Medium");
		game = new GameLogic(boardSize);
		UpdateScoreLabels();
		BuildBoard(boardSize);
	}

	public TicTacToePage(bool botEnabled) : this()
	{
		isBotEnabled = botEnabled;
	}

	// ===== Back navigation =====

	private async void OnBackTapped(object? sender, TappedEventArgs e)
	{
		await Navigation.PopAsync();
	}

	// ===== Mänguvälja ehitamine =====

	private void BuildBoard(int size)
	{
		boardGrid.Children.Clear();
		boardGrid.RowDefinitions.Clear();
		boardGrid.ColumnDefinitions.Clear();

		boardCells = new Border[size, size];
		boardSymbols = new Image[size, size];

		int cellSize = size switch
		{
			4 => 82,
			5 => 65,
			_ => 108
		};
		int imageSize = cellSize;
		const int outerCornerRadius = 35;

		for (int i = 0; i < size; i++)
		{
			boardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(cellSize) });
			boardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(cellSize) });
		}

		for (int row = 0; row < size; row++)
		{
			for (int col = 0; col < size; col++)
			{
				var symbolImage = new Image
				{
					WidthRequest = imageSize,
					HeightRequest = imageSize,
					Aspect = Aspect.AspectFit,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					IsVisible = false
				};

				var cell = new Border
				{
					WidthRequest = cellSize,
					HeightRequest = cellSize,
					Padding = 0,
					StrokeThickness = 0,
					BackgroundColor = emptyCellBg,
					Content = symbolImage,
					StrokeShape = new RoundRectangle
					{
						CornerRadius = GetCellCornerRadius(row, col, size, outerCornerRadius)
					}
				};

				int r = row, c = col;
				cell.GestureRecognizers.Add(new TapGestureRecognizer
				{
					Command = new Command(async () => await OnCellClicked(r, c))
				});

				boardCells[row, col] = cell;
				boardSymbols[row, col] = symbolImage;
				boardGrid.Add(cell, col, row);
			}
		}
	}

	private static CornerRadius GetCellCornerRadius(int row, int col, int size, double radius)
	{
		double topLeft = row == 0 && col == 0 ? radius : 0;
		double topRight = row == 0 && col == size - 1 ? radius : 0;
		double bottomLeft = row == size - 1 && col == 0 ? radius : 0;
		double bottomRight = row == size - 1 && col == size - 1 ? radius : 0;

		return new CornerRadius(topLeft, topRight, bottomLeft, bottomRight);
	}

	// ===== Lahtri vajutus =====

	private async Task OnCellClicked(int row, int col, bool isBot = false)
	{
		if (game.IsGameOver) return;
		if (isBotThinking && !isBot) return;

		string player = game.CurrentPlayer;
		if (!game.MakeMove(row, col)) return;

		var cell = boardCells[row, col];
		var symbol = boardSymbols[row, col];
		symbol.Source = player == PlayerX ? "crossbox.png" : "circlebox.png";
		symbol.IsVisible = true;
		cell.BackgroundColor = player == PlayerX ? xBgColor : oBgColor;

		// Skaala-animatsioon
		await cell.ScaleToAsync(1.08, 90);
		await cell.ScaleToAsync(1.0, 90);

		// Kontrolli võitjat
		string? winner = game.CheckWinner();
		if (winner != null)
		{
			game.EndGame();
			if (winner == PlayerX) xWins++;
			else oWins++;
			SaveScore();
			UpdateScoreLabels();
			statusLabel.Text = $"Mängija {winner} võitis! 🎉";

			await AnimateWinningCells();

			// Navigate to Won page for the winner
			if (isBotEnabled && winner == PlayerO)
			{
				// Bot won — player lost
				await Navigation.PushAsync(new TicTacToeLosePage(PlayerX, isBotEnabled));
			}
			else
			{
				await Navigation.PushAsync(new TicTacToeWonPage(winner, isBotEnabled));
			}
			return;
		}

		if (game.IsBoardFull())
		{
			game.EndGame();
			statusLabel.Text = "Viik! 🤝";
			await DisplayAlertAsync("Mäng läbi!", "Viik!", "OK");
			return;
		}

		game.SwitchPlayer();
		statusLabel.Text = $"Mängija {game.CurrentPlayer} käik";

		// Bot käik
		await PlayBotTurnAsync(400);
	}

	// ===== Bot loogika =====

	private async Task PlayBotTurnAsync(int delay)
	{
		if (!isBotEnabled || game.CurrentPlayer != PlayerO || game.IsGameOver)
			return;

		isBotThinking = true;
		try
		{
			await Task.Delay(delay);

			var (row, col) = botDifficulty switch
			{
				"Easy" => GetBotMoveEasy(),
				"Hard" => GetBotMoveHard(),
				_ => GetBotMoveMedium()
			};

			await OnCellClicked(row, col, isBot: true);
		}
		finally
		{
			isBotThinking = false;
		}
	}

	// Easy: purely random
	private (int Row, int Col) GetBotMoveEasy()
	{
		int n = game.BoardSize;
		var empty = new List<(int, int)>();
		for (int r = 0; r < n; r++)
			for (int c = 0; c < n; c++)
				if (string.IsNullOrEmpty(game.Board[r, c]))
					empty.Add((r, c));
		return empty[random.Next(empty.Count)];
	}

	// Medium: block/win + center/corners (original logic)
	private (int Row, int Col) GetBotMoveMedium()
	{
		int n = game.BoardSize;

		var winMove = FindWinningMove(PlayerO);
		if (winMove != null) return winMove.Value;

		var blockMove = FindWinningMove(PlayerX);
		if (blockMove != null) return blockMove.Value;

		int center = n / 2;
		if (string.IsNullOrEmpty(game.Board[center, center]))
			return (center, center);

		int[][] corners = { new[] { 0, 0 }, new[] { 0, n - 1 }, new[] { n - 1, 0 }, new[] { n - 1, n - 1 } };
		var shuffled = corners.OrderBy(_ => random.Next()).ToArray();
		foreach (var c in shuffled)
			if (string.IsNullOrEmpty(game.Board[c[0], c[1]]))
				return (c[0], c[1]);

		var empty = new List<(int, int)>();
		for (int r = 0; r < n; r++)
			for (int c = 0; c < n; c++)
				if (string.IsNullOrEmpty(game.Board[r, c]))
					empty.Add((r, c));

		return empty[random.Next(empty.Count)];
	}

	// Hard: minimax (depth-limited for larger boards)
	private (int Row, int Col) GetBotMoveHard()
	{
		int n = game.BoardSize;
		int maxDepth = n switch
		{
			3 => 9,
			4 => 5,
			_ => 3
		};

		int bestScore = int.MinValue;
		(int Row, int Col) bestMove = (-1, -1);

		for (int r = 0; r < n; r++)
		{
			for (int c = 0; c < n; c++)
			{
				if (!string.IsNullOrEmpty(game.Board[r, c])) continue;
				game.Board[r, c] = PlayerO;
				int score = Minimax(game, false, 0, maxDepth, int.MinValue, int.MaxValue);
				game.Board[r, c] = "";
				if (score > bestScore)
				{
					bestScore = score;
					bestMove = (r, c);
				}
			}
		}
		return bestMove;
	}

	private int Minimax(GameLogic g, bool isMaximizing, int depth, int maxDepth, int alpha, int beta)
	{
		string? winner = g.CheckWinner();
		if (winner == PlayerO) return 100 - depth;
		if (winner == PlayerX) return depth - 100;
		if (g.IsBoardFull() || depth >= maxDepth) return 0;

		int n = g.BoardSize;
		int best = isMaximizing ? int.MinValue : int.MaxValue;
		string player = isMaximizing ? PlayerO : PlayerX;

		for (int r = 0; r < n; r++)
		{
			for (int c = 0; c < n; c++)
			{
				if (!string.IsNullOrEmpty(g.Board[r, c])) continue;

				g.Board[r, c] = player;
				int score = Minimax(g, !isMaximizing, depth + 1, maxDepth, alpha, beta);
				g.Board[r, c] = "";

				if (isMaximizing)
				{
					best = Math.Max(best, score);
					alpha = Math.Max(alpha, score);
				}
				else
				{
					best = Math.Min(best, score);
					beta = Math.Min(beta, score);
				}

				if (beta <= alpha) return best;
			}
		}

		return best;
	}

	private (int Row, int Col)? FindWinningMove(string player)
	{
		int n = game.BoardSize;
		for (int r = 0; r < n; r++)
			for (int c = 0; c < n; c++)
			{
				if (!string.IsNullOrEmpty(game.Board[r, c])) continue;

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

		var goldStroke = new SolidColorBrush(Color.FromArgb("#FFD600"));

		for (int flash = 0; flash < 3; flash++)
		{
			foreach (var (row, col) in cells)
			{
				boardCells[row, col].Stroke = goldStroke;
				boardCells[row, col].StrokeThickness = 5;
			}
			await Task.Delay(250);

			foreach (var (row, col) in cells)
			{
				boardCells[row, col].Stroke = null;
				boardCells[row, col].StrokeThickness = 0;
			}
			await Task.Delay(200);
		}

		// Leave stroke visible at the end
		foreach (var (row, col) in cells)
		{
			boardCells[row, col].Stroke = goldStroke;
			boardCells[row, col].StrokeThickness = 5;
		}
		await Task.Delay(300);
	}

	// ===== Nuppude sündmused =====

	private async void OnNewGame(object? sender, EventArgs e)
	{
		game.ResetBoard();
		RefreshBoard();
		statusLabel.Text = $"Mängija {game.CurrentPlayer} käik";

		await statusLabel.ScaleToAsync(1.2, 100);
		await statusLabel.ScaleToAsync(1.0, 100);

		// Kui bot on sees ja O alustab
		await PlayBotTurnAsync(300);
	}

	// ===== Abimeetodid =====

	private void RefreshBoard()
	{
		int size = game.BoardSize;
		for (int row = 0; row < size; row++)
			for (int col = 0; col < size; col++)
			{
				boardSymbols[row, col].Source = null;
				boardSymbols[row, col].IsVisible = false;
				boardCells[row, col].BackgroundColor = emptyCellBg;
			}
	}

	private void UpdateScoreLabels()
	{
		xScoreLabel.Text = $"Võit: {xWins}";
		oScoreLabel.Text = $"Võit: {oWins}";
	}

	private void LoadScore()
	{
		xWins = Preferences.Default.Get("TTT_XWins", 0);
		oWins = Preferences.Default.Get("TTT_OWins", 0);
	}

	private void SaveScore()
	{
		Preferences.Default.Set("TTT_XWins", xWins);
		Preferences.Default.Set("TTT_OWins", oWins);
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
			CurrentPlayer = CurrentPlayer == PlayerX ? PlayerO : PlayerX;
		}

		public string? CheckWinner()
		{
			var winningCells = FindWinningCells();
			return winningCells == null ? null : Board[winningCells[0].Row, winningCells[0].Col];
		}

		private List<(int Row, int Col)>? FindWinningCells()
		{
			int n = BoardSize;
			int[][] directions = { new[] { 0, 1 }, new[] { 1, 0 }, new[] { 1, 1 }, new[] { 1, -1 } };

			for (int row = 0; row < n; row++)
			{
				for (int col = 0; col < n; col++)
				{
					string cell = Board[row, col];
					if (string.IsNullOrEmpty(cell)) continue;

					foreach (var direction in directions)
					{
						var winningCells = GetWinningLine(row, col, direction[0], direction[1], cell);
						if (winningCells != null)
							return winningCells;
					}
				}
			}
			return null;
		}

		private List<(int Row, int Col)>? GetWinningLine(int startRow, int startCol, int dRow, int dCol, string player)
		{
			int endRow = startRow + (BoardSize - 1) * dRow;
			int endCol = startCol + (BoardSize - 1) * dCol;
			if (endRow < 0 || endRow >= BoardSize || endCol < 0 || endCol >= BoardSize)
				return null;

			var cells = new List<(int Row, int Col)>(BoardSize);
			for (int i = 0; i < BoardSize; i++)
			{
				int row = startRow + i * dRow;
				int col = startCol + i * dCol;
				if (Board[row, col] != player)
					return null;
				cells.Add((row, col));
			}

			return cells;
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
			CurrentPlayer = PlayerX;
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
			=> FindWinningCells();
	}
}

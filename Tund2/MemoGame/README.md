# Игра Memo

Эта игра Memo использует флаги стран. В каждой новой игре программа случайно выбирает 6 флагов из 14, создает по 2 карточки для каждого флага и показывает их на поле 3 x 4.

Номера строк показывают, где находится код в файле. Некоторые блоки кода укорочены, чтобы показать только главную часть функции.

## Основные файлы

- `Game.cs` — правила игры: выбор флагов, проверка пары, очки, время.
- `Card.cs` — данные и состояние одной карточки.
- `MemoGamePage.xaml.cs` — экран игры, клики по карточкам, анимации, размер поля.
- `Leaderboard.cs` — сохранение и сортировка лучших результатов.

## Начать новую игру

Файл: `Game.cs`, строка 40

```csharp
public void Start()
{
	var cards = new List<Card>();
	var id = 1;
	var selectedFlags = flagCards
		.OrderBy(_ => Random.Shared.Next())
		.Take(PairCount);

	foreach (var flagCard in selectedFlags)
	{
		cards.Add(new Card(id++, flagCard.Name, flagCard.Name, flagCard.ImageFile));
		cards.Add(new Card(id++, flagCard.Name, flagCard.Name, flagCard.ImageFile));
	}

	Cards = cards
		.OrderBy(_ => Random.Shared.Next())
		.ToList();
}
```

Что делает:

- Случайно выбирает 6 флагов, создает для каждого пару карточек и перемешивает поле.
- Сбрасывает состояние игры: выбранную карточку, ходы, время и очки.

## Выбрать карточку

Файл: `Game.cs`, строка 75

```csharp
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
	}
}
```

Что делает:

- Открывает карточку и сравнивает ее с первой выбранной.
- Если пара совпала, карточки остаются открытыми и игрок получает очки.
- Если пара не совпала, игрок теряет очко, а карточки потом закрываются.

## Спрятать неправильные карточки

Файл: `Game.cs`, строка 113

```csharp
public void HideCards(IEnumerable<Card> cards)
{
	foreach (var card in cards)
	{
		card.Hide();
	}
}
```

Что делает:

- Закрывает карточки, которые не образовали пару.

## Состояние карточки

Файл: `Card.cs`, строки 20, 28 и 36

```csharp
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
```

Что делает:

- `Reveal()` открывает карточку, `Hide()` закрывает ее, а `MarkMatched()` помечает найденную пару.

## Запуск игры на странице

Файл: `MemoGamePage.xaml.cs`, строка 62

```csharp
private void StartNewGame()
{
	isBusy = false;
	game.Start();
	CreateBoard();
	UpdateStats();
	timer.Start();
}
```

Что делает:

- Запускает новую игру, создает поле, обновляет статистику и включает таймер.

## Создать поле 3 x 4

Файл: `MemoGamePage.xaml.cs`, строка 71

```csharp
private void CreateBoard()
{
	BoardGrid.Children.Clear();
	BoardGrid.RowDefinitions.Clear();
	BoardGrid.ColumnDefinitions.Clear();

	var rowCount = (int)Math.Ceiling((double)game.Cards.Count / BoardColumnCount);

	for (var row = 0; row < rowCount; row++)
	{
		BoardGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
	}

	for (var column = 0; column < BoardColumnCount; column++)
	{
		BoardGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
	}
}
```

Что делает:

- Очищает старое поле и создает сетку 3 x 4.
- Добавляет карточки в нужные ячейки и обновляет их размер.

## Сделать карточки квадратными

Файл: `MemoGamePage.xaml.cs`, строка 111

```csharp
private void UpdateBoardSize()
{
	var rowCount = (int)Math.Ceiling((double)game.Cards.Count / BoardColumnCount);
	var availableWidth = BoardFrame.Width - BoardFrame.Padding.Left - BoardFrame.Padding.Right;
	var availableHeight = BoardFrame.Height - BoardFrame.Padding.Top - BoardFrame.Padding.Bottom;
	var cardWidth = (availableWidth - totalColumnSpacing) / BoardColumnCount;
	var cardHeight = (availableHeight - totalRowSpacing) / rowCount;
	var cardSize = Math.Floor(Math.Min(cardWidth, cardHeight));

	BoardGrid.WidthRequest = (cardSize * BoardColumnCount) + totalColumnSpacing;
	BoardGrid.HeightRequest = (cardSize * rowCount) + totalRowSpacing;
}
```

Что делает:

- Считает доступное место на экране и выбирает такой размер, чтобы карточки оставались квадратными.

## Создать вид одной карточки

Файл: `MemoGamePage.xaml.cs`, строка 136

```csharp
private Border CreateCardView(Card card)
{
	var image = new Image
	{
		Source = card.ImageFile,
		Aspect = Aspect.AspectFit,
		IsVisible = false,
		Margin = 12
	};

	var tap = new TapGestureRecognizer();
	tap.CommandParameter = card;
	tap.Tapped += OnCardTapped;
}
```

Что делает:

- Создает внешний вид карточки: знак `?`, картинку флага, рамку и обработчик клика.
- Сохраняет элементы карточки, чтобы потом менять ее вид.

## Клик по карточке

Файл: `MemoGamePage.xaml.cs`, строка 180

```csharp
private async void OnCardTapped(object? sender, TappedEventArgs e)
{
	if (isBusy || e.Parameter is not Card card)
	{
		return;
	}

	var result = game.SelectCard(card);
	await FlipCardAsync(card);

	if (result.Kind == GameTurnKind.Mismatch)
	{
		isBusy = true;
		await Task.Delay(700);
		game.HideCards(result.Cards);
		isBusy = false;
	}
}
```

Что делает:

- Передает выбранную карточку в игру, переворачивает ее и обновляет статистику.
- Для правильной пары показывает успех, для неправильной - ошибку и снова закрывает карточки.
- В конце игры сохраняет результат и показывает сообщение.

## Перевернуть карточку

Файл: `MemoGamePage.xaml.cs`, строка 234

```csharp
private async Task FlipCardAsync(Card card)
{
	await cardView.RotateYToAsync(90, 120, Easing.CubicIn);
	UpdateCardView(card);
	await cardView.RotateYToAsync(0, 120, Easing.CubicOut);
}
```

Что делает:

- Делает анимацию переворота и в середине меняет сторону карточки.

## Показать переднюю или заднюю сторону карточки

Файл: `MemoGamePage.xaml.cs`, строка 260

```csharp
private void UpdateCardView(Card card)
{
	cardView.BackgroundColor = card.IsFaceUp ? currentTheme.CardFrontColor : currentTheme.CardBackColor;
	cardView.Opacity = card.IsMatched ? 0.75 : 1;

	label.IsVisible = !card.IsFaceUp;
	image.IsVisible = card.IsFaceUp;
}
```

Что делает:

- Обновляет цвет, прозрачность и показывает либо `?`, либо флаг.

## Обновить статистику

Файл: `MemoGamePage.xaml.cs`, строка 306

```csharp
private void UpdateStats()
{
	PointsLabel.Text = player.Points.ToString();
	MovesLabel.Text = game.Moves.ToString();
	TimeLabel.Text = $"{game.Seconds}s";
}
```

Что делает:

- Показывает очки, ходы, время и лучший результат игрока.

## Сохранить результат в таблицу лидеров

Файл: `Leaderboard.cs`, строка 18

```csharp
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
	}

	SaveResults();
}
```

Что делает:

- Добавляет новый результат или заменяет старый, если новый лучше.
- Сортирует таблицу, оставляет лучшие записи и сохраняет их.

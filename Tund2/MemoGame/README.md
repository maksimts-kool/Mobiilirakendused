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

- Строки 42-43: создает пустой список карточек и переменную `id`.
- Строки 44-46: случайно перемешивает все флаги и берет только 6 штук.
- Строки 48-52: для каждого выбранного флага создает 2 одинаковые карточки.
- Строки 54-56: перемешивает все карточки перед показом на поле.
- Строки 58-64: сбрасывает первую выбранную карточку, ходы, время, состояние игры и очки игрока.

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

- Строки 77-80: игнорирует клик, если игра не идет, закончилась, карточка уже открыта или уже найдена.
- Строка 82: открывает выбранную карточку.
- Строки 84-88: если это первая карточка в паре, запоминает ее и ждет вторую.
- Строки 90-92: если это вторая карточка, добавляет один ход и очищает `firstCard`.
- Строки 94-98: если ключи пары одинаковые, отмечает карточки найденными и добавляет 10 очков.
- Строки 100-106: проверяет, закончилась ли игра, и возвращает результат `Match`.
- Строки 109-110: если пара неправильная, снимает 1 очко и возвращает результат `Mismatch`.

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

- Строки 115-118: проходит по всем неправильным карточкам и закрывает каждую.
- Строка 117: вызывает `Hide()` у карточки, чтобы она снова стала закрытой.

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

- Строки 20-25: `Reveal()` открывает карточку, если она еще не найдена.
- Строки 28-33: `Hide()` закрывает карточку, если она еще не найдена.
- Строки 36-40: `MarkMatched()` оставляет карточку открытой и помечает ее как найденную.

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

- Строка 64: разрешает клики по карточкам.
- Строка 65: запускает игровую логику из `Game.cs`.
- Строка 66: создает видимое поле карточек.
- Строка 67: обновляет очки, ходы и время на экране.
- Строка 68: запускает таймер игры.

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

- Строки 73-75: очищает старые карточки, строки и колонки.
- Строки 76-79: очищает словари, где хранятся элементы карточек.
- Строка 81: считает, сколько строк нужно для всех карточек.
- Строки 83-86: добавляет строки в `BoardGrid`.
- Строки 88-91: добавляет 3 колонки, потому что `BoardColumnCount = 3`.
- Строки 93-101: добавляет каждую карточку в нужную строку и колонку.
- Строка 103: обновляет размер поля, чтобы карточки были квадратными.

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

- Строки 113-116: выходит из функции, если размер поля еще неизвестен или карточек нет.
- Строка 118: считает количество строк.
- Строки 119-120: считает доступную ширину и высоту внутри рамки.
- Строки 121-122: считает расстояние между колонками и строками.
- Строки 123-125: выбирает самый большой квадратный размер карточки, который помещается на экран.
- Строки 127-130: выходит из функции, если размер получился неправильным.
- Строки 132-133: задает размер всего поля так, чтобы все карточки были квадратными.

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

- Строки 138-146: создает текст `?` для закрытой карточки.
- Строки 148-154: создает картинку флага и делает ее сначала невидимой.
- Строки 156-158: кладет картинку и текст в один `Grid`.
- Строки 160-165: создает рамку карточки.
- Строки 167-170: добавляет обработчик клика по карточке.
- Строки 172-175: сохраняет элементы карточки в словари и обновляет ее вид.
- Строка 177: возвращает готовую карточку на страницу.

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

- Строки 182-185: не дает нажимать карточки, пока игра занята.
- Строки 187-191: отправляет карточку в игровую логику и игнорирует неправильный клик.
- Строки 193-194: переворачивает карточку и обновляет статистику.
- Строки 196-202: если пара правильная, показывает зеленую рамку и анимацию.
- Строки 204-218: если пара неправильная, показывает красную рамку, ждет и закрывает карточки.
- Строки 220-230: обновляет статистику, сохраняет результат и показывает сообщение, если игра закончилась.

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

- Строки 236-239: выходит из функции, если вид карточки не найден.
- Строка 241: поворачивает карточку до середины анимации.
- Строка 242: меняет вид карточки на открытую или закрытую.
- Строка 243: заканчивает поворот карточки.

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

- Строки 262-267: выходит из функции, если нужные элементы карточки не найдены.
- Строки 269-272: выбирает цвет карточки и цвет рамки.
- Строка 273: делает найденные карточки немного прозрачными.
- Строки 275-277: показывает `?` на закрытой карточке и флаг на открытой.

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

- Строка 308: показывает очки игрока.
- Строка 309: показывает количество ходов.
- Строка 310: показывает время игры в секундах.
- Строки 311-313: показывает лучший результат игрока или текст, что результата еще нет.

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

- Строка 20: ищет старый результат игрока с таким же именем.
- Строки 22-25: если результата еще нет, добавляет новый.
- Строки 26-32: если новый результат лучше старого, заменяет очки, время, ходы и дату.
- Строки 34-36: сортирует результаты, убирает дубли и оставляет максимум 20 записей.
- Строка 38: сохраняет результаты в память приложения.

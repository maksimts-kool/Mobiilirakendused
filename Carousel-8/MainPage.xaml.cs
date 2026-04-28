using System.Collections.ObjectModel;
using System.ComponentModel;
using Carousel_8.Models;
using Carousel_8.Services;

namespace Carousel_8;

public partial class MainPage : ContentPage
{
	private const string EnglishLanguageCode = "en";
	private const string EstonianLanguageCode = "et";
	private const string CardTitleAutomationId = "CardTitle";
	private const double FontSizeStep = 0.5;
	private const double TextFitTolerance = 0.5;
	private const double PreviewSettledOpacity = 0.78;
	private const double PreviewMovingOpacity = 0.12;
	private const double PreviewSettledScale = 1.0;
	private const double PreviewMovingScale = 0.94;
	private const double PreviewTravelDistance = 28;
	private const double PreviewSettledMaskOpacity = 0.84;
	private const double PreviewMovingMaskOpacity = 1.0;

	private static readonly TimeSpan AutoScrollInterval = TimeSpan.FromSeconds(4);

	private static readonly CardDefinition[] CardDefinitions =
	{
		new("solar_home.jpg", "Card1Title", "Card1Description", "Card1DetailText"),
		new("mps_farm.webp", "Card2Title", "Card2Description", "Card2DetailText"),
		new("vertical_farm.jpg", "Card3Title", "Card3Description", "Card3DetailText"),
		new("recycle_plant.jpg", "Card4Title", "Card4Description", "Card4DetailText"),
		new("drone_underwater.jpg", "Card5Title", "Card5Description", "Card5DetailText")
	};

	private readonly IDispatcherTimer autoScrollTimer;
	private readonly Dictionary<Label, double> cardTextBaseSizes = new();
	private readonly HashSet<Label> cardTextSizingInProgress = new();
	private readonly HashSet<Label> subscribedCardTextLabels = new();
	private ObservableCollection<GreenTechCard> cards = new();
	private int currentPosition;
	private GreenTechCard? previousCard;
	private GreenTechCard? nextCard;

	public MainPage()
	{
		InitializeComponent();

		Localizer = LocalizationManager.Instance;
		BindingContext = this;

		autoScrollTimer = CreateAutoScrollTimer();

		Localizer.CultureChanged += OnCultureChanged;

		ReloadCards();
	}

	public ObservableCollection<GreenTechCard> Cards
	{
		get => cards;
		private set
		{
			if (ReferenceEquals(cards, value))
			{
				return;
			}

			cards = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(HasPreviewCards));
		}
	}

	public LocalizationManager Localizer { get; }

	public string HeroTitleText => Localizer["HeroTitle"];

	public string HeroSubtitleText => Localizer["HeroSubtitle"];

	public string LanguageLabelText => Localizer["LanguageLabel"];

	public string LanguageEnglishText => Localizer["LanguageEnglish"];

	public string LanguageEstonianText => Localizer["LanguageEstonian"];

	public string SectionLabelText => Localizer["SectionLabel"];

	public GreenTechCard? PreviousCard
	{
		get => previousCard;
		private set
		{
			if (ReferenceEquals(previousCard, value))
			{
				return;
			}

			previousCard = value;
			OnPropertyChanged();
		}
	}

	public GreenTechCard? NextCard
	{
		get => nextCard;
		private set
		{
			if (ReferenceEquals(nextCard, value))
			{
				return;
			}

			nextCard = value;
			OnPropertyChanged();
		}
	}

	public bool HasPreviewCards => Cards.Count > 1;

	public bool IsEnglishSelected => IsSelectedLanguage(EnglishLanguageCode);

	public bool IsEstonianSelected => IsSelectedLanguage(EstonianLanguageCode);

	protected override void OnAppearing()
	{
		base.OnAppearing();
		StartAutoScroll();
		_ = PlayIntroAnimationAsync();
	}

	protected override void OnDisappearing()
	{
		StopAutoScroll();
		base.OnDisappearing();
	}

	private void OnEnglishClicked(object? sender, EventArgs e)
	{
		Localizer.SetCulture(EnglishLanguageCode);
	}

	private void OnEstonianClicked(object? sender, EventArgs e)
	{
		Localizer.SetCulture(EstonianLanguageCode);
	}

	private async void OnCardTapped(object? sender, TappedEventArgs e)
	{
		if (!TryGetTappedCard(sender, out var card, out var tappedCardIndex))
		{
			return;
		}

		if (tappedCardIndex != currentPosition)
		{
			ScrollToCard(tappedCardIndex, animate: true);
			return;
		}

		await ShowCardDetailsAsync(card);
	}

	private void OnCarouselPositionChanged(object? sender, PositionChangedEventArgs e)
	{
		currentPosition = e.CurrentPosition;
		UpdatePreviewCards();
		ApplyPreviewProgress(0);
	}

	private void OnCardTextLoaded(object? sender, EventArgs e)
	{
		if (sender is not Label label)
		{
			return;
		}

		if (!cardTextBaseSizes.ContainsKey(label))
		{
			cardTextBaseSizes[label] = label.FontSize;
		}

		if (subscribedCardTextLabels.Add(label))
		{
			label.PropertyChanged += OnCardTextPropertyChanged;
		}

		FitCardTextLabel(label);
	}

	private void OnCardTextUnloaded(object? sender, EventArgs e)
	{
		if (sender is not Label label)
		{
			return;
		}

		if (subscribedCardTextLabels.Remove(label))
		{
			label.PropertyChanged -= OnCardTextPropertyChanged;
		}

		cardTextBaseSizes.Remove(label);
		cardTextSizingInProgress.Remove(label);
	}

	private void OnCardTextSizeChanged(object? sender, EventArgs e)
	{
		if (sender is Label label)
		{
			FitCardTextLabel(label);
		}
	}

	private void OnCardTextPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (sender is not Label label || e.PropertyName != Label.TextProperty.PropertyName)
		{
			return;
		}

		Dispatcher.Dispatch(() => FitCardTextLabel(label));
	}

	private void OnCarouselScrolled(object? sender, ItemsViewScrolledEventArgs e)
	{
		if (Cards.Count <= 1)
		{
			ApplyPreviewProgress(0);
			return;
		}

		var viewportWidth = MainCarousel.Width;

		if (viewportWidth <= 0)
		{
			ApplyPreviewProgress(0);
			return;
		}

		var rawOffset = e.HorizontalOffset / viewportWidth;
		var progress = Math.Abs(rawOffset - Math.Round(rawOffset, MidpointRounding.AwayFromZero)) * 2;
		ApplyPreviewProgress(Math.Clamp(progress, 0, 1));
	}

	private void OnCultureChanged(object? sender, EventArgs e)
	{
		NotifyLocalizedPropertiesChanged();
		ReloadCards();
	}

	private void OnAutoScrollTick(object? sender, EventArgs e)
	{
		if (Cards.Count <= 1)
		{
			return;
		}

		ScrollToCard(currentPosition + 1, animate: true);
	}

	private void ReloadCards()
	{
		currentPosition = Math.Clamp(currentPosition, 0, Math.Max(CardDefinitions.Length - 1, 0));

		Cards = CreateLocalizedCards();
		Title = HeroTitleText;

		UpdatePreviewCards();
		ApplyPreviewProgress(0);

		if (Cards.Count > 0)
		{
			MainCarousel.Position = currentPosition;
		}
	}

	private ObservableCollection<GreenTechCard> CreateLocalizedCards()
	{
		var localizedCards = new ObservableCollection<GreenTechCard>();

		foreach (var card in CardDefinitions)
		{
			localizedCards.Add(new GreenTechCard(
				card.ImageSource,
				Localizer[card.TitleKey],
				Localizer[card.DescriptionKey],
				Localizer[card.DetailKey]));
		}

		return localizedCards;
	}

	private void UpdatePreviewCards()
	{
		if (Cards.Count == 0)
		{
			PreviousCard = null;
			NextCard = null;
			return;
		}

		PreviousCard = Cards[WrapIndex(currentPosition - 1)];
		NextCard = Cards[WrapIndex(currentPosition + 1)];
	}

	private int WrapIndex(int index)
	{
		return Cards.Count == 0
			? 0
			: (index % Cards.Count + Cards.Count) % Cards.Count;
	}

	private void ApplyPreviewProgress(double progress)
	{
		if (LeftPreviewCard is null || RightPreviewCard is null || LeftPreviewMaskLayer is null || RightPreviewMaskLayer is null)
		{
			return;
		}

		var clampedProgress = Math.Clamp(progress, 0, 1);
		var previewOpacity = Lerp(PreviewSettledOpacity, PreviewMovingOpacity, clampedProgress);
		var previewScale = Lerp(PreviewSettledScale, PreviewMovingScale, clampedProgress);
		var previewMaskOpacity = Lerp(PreviewSettledMaskOpacity, PreviewMovingMaskOpacity, clampedProgress);
		var travel = PreviewTravelDistance * clampedProgress;

		LeftPreviewCard.Opacity = previewOpacity;
		LeftPreviewCard.Scale = previewScale;
		LeftPreviewCard.TranslationX = -travel;

		RightPreviewCard.Opacity = previewOpacity;
		RightPreviewCard.Scale = previewScale;
		RightPreviewCard.TranslationX = travel;

		LeftPreviewMaskLayer.Opacity = previewMaskOpacity;
		RightPreviewMaskLayer.Opacity = previewMaskOpacity;
	}

	private IDispatcherTimer CreateAutoScrollTimer()
	{
		var timer = Dispatcher.CreateTimer();
		timer.Interval = AutoScrollInterval;
		timer.Tick += OnAutoScrollTick;

		return timer;
	}

	private bool IsSelectedLanguage(string languageCode)
	{
		return Localizer.CurrentLanguageCode == languageCode;
	}

	private void NotifyLocalizedPropertiesChanged()
	{
		OnPropertyChanged(nameof(IsEnglishSelected));
		OnPropertyChanged(nameof(IsEstonianSelected));
		OnPropertyChanged(nameof(HeroTitleText));
		OnPropertyChanged(nameof(HeroSubtitleText));
		OnPropertyChanged(nameof(LanguageLabelText));
		OnPropertyChanged(nameof(LanguageEnglishText));
		OnPropertyChanged(nameof(LanguageEstonianText));
		OnPropertyChanged(nameof(SectionLabelText));
	}

	private bool TryGetTappedCard(object? sender, out GreenTechCard card, out int cardIndex)
	{
		card = null!;
		cardIndex = -1;

		var cardView = GetTappedCardView(sender);

		if (cardView?.BindingContext is not GreenTechCard tappedCard)
		{
			return false;
		}

		var tappedCardIndex = Cards.IndexOf(tappedCard);

		if (tappedCardIndex < 0)
		{
			return false;
		}

		card = tappedCard;
		cardIndex = tappedCardIndex;
		return true;
	}

	private static VisualElement? GetTappedCardView(object? sender)
	{
		return sender as VisualElement
			?? (sender as TapGestureRecognizer)?.Parent as VisualElement;
	}

	private void ScrollToCard(int cardIndex, bool animate)
	{
		currentPosition = WrapIndex(cardIndex);
		MainCarousel.ScrollTo(currentPosition, position: ScrollToPosition.Center, animate: animate);
		UpdatePreviewCards();
	}

	private async Task ShowCardDetailsAsync(GreenTechCard card)
	{
		StopAutoScroll();

		try
		{
			await DisplayAlertAsync(Localizer["AlertTitle"], card.DetailText, Localizer["AlertClose"]);
		}
		finally
		{
			StartAutoScroll();
		}
	}

	private void FitCardTextLabel(Label label)
	{
		if (cardTextSizingInProgress.Contains(label) ||
			!cardTextBaseSizes.TryGetValue(label, out var baseSize) ||
			string.IsNullOrWhiteSpace(label.Text))
		{
			return;
		}

		var availableSize = GetAvailableTextSize(label);

		if (availableSize.Width <= 0 || availableSize.Height <= 0)
		{
			return;
		}

		cardTextSizingInProgress.Add(label);

		try
		{
			label.FontSize = baseSize;

			var minimumSize = GetMinimumFontSize(label, baseSize);
			label.FontSize = FindFittingFontSize(label, baseSize, minimumSize, availableSize);
		}
		finally
		{
			cardTextSizingInProgress.Remove(label);
		}
	}

	private static (double Width, double Height) GetAvailableTextSize(Label label)
	{
		var availableWidth = label.Width;
		var availableHeight = label.Height;

		if (label.Parent is VisualElement container)
		{
			availableWidth = Math.Max(availableWidth, container.Width);
			availableHeight = Math.Max(availableHeight, container.Height);
		}

		return (availableWidth, availableHeight);
	}

	private static double GetMinimumFontSize(Label label, double baseSize)
	{
		var scale = label.AutomationId == CardTitleAutomationId ? 0.66 : 0.72;
		return baseSize * scale;
	}

	private static double FindFittingFontSize(
		Label label,
		double baseSize,
		double minimumSize,
		(double Width, double Height) availableSize)
	{
		var fittedSize = baseSize;

		for (var fontSize = baseSize; fontSize >= minimumSize; fontSize -= FontSizeStep)
		{
			label.FontSize = fontSize;
			var requestedSize = label.Measure(availableSize.Width, double.PositiveInfinity);

			if (requestedSize.Height <= availableSize.Height + TextFitTolerance)
			{
				fittedSize = fontSize;
				break;
			}

			fittedSize = fontSize - FontSizeStep;
		}

		return Math.Max(minimumSize, fittedSize);
	}

	private static double Lerp(double start, double end, double progress)
	{
		return start + ((end - start) * progress);
	}

	private async Task PlayIntroAnimationAsync()
	{
		HeroSection.Opacity = 0;
		HeroSection.TranslationY = 18;

		CarouselSection.Opacity = 0;
		CarouselSection.TranslationY = 26;

		await Task.WhenAll(
			HeroSection.FadeToAsync(1, 360, Easing.CubicOut),
			HeroSection.TranslateToAsync(0, 0, 360, Easing.CubicOut),
			CarouselSection.FadeToAsync(1, 500, Easing.CubicOut),
			CarouselSection.TranslateToAsync(0, 0, 500, Easing.CubicOut));
	}

	private void StartAutoScroll()
	{
		if (Cards.Count > 1 && !autoScrollTimer.IsRunning)
		{
			autoScrollTimer.Start();
		}
	}

	private void StopAutoScroll()
	{
		if (autoScrollTimer.IsRunning)
		{
			autoScrollTimer.Stop();
		}
	}

	private readonly record struct CardDefinition(
		string ImageSource,
		string TitleKey,
		string DescriptionKey,
		string DetailKey);
}

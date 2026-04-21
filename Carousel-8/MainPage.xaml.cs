using System.Collections.ObjectModel;
using System.ComponentModel;
using Carousel_8.Models;
using Carousel_8.Services;

namespace Carousel_8;

public partial class MainPage : ContentPage
{
	private const double PreviewSettledOpacity = 0.78;
	private const double PreviewMovingOpacity = 0.12;
	private const double PreviewSettledScale = 1.0;
	private const double PreviewMovingScale = 0.94;
	private const double PreviewTravelDistance = 28;
	private const double PreviewSettledMaskOpacity = 0.84;
	private const double PreviewMovingMaskOpacity = 1.0;

	private static readonly CardDefinition[] CardDefinitions =
	{
		new("smart_solar_rooftops.svg", "Card1Title", "Card1Description", "Card1DetailText"),
		new("floating_wind_farms.svg", "Card2Title", "Card2Description", "Card2DetailText"),
		new("vertical_farms.svg", "Card3Title", "Card3Description", "Card3DetailText"),
		new("battery_recycling_labs.svg", "Card4Title", "Card4Description", "Card4DetailText"),
		new("ocean_cleaning_drones.svg", "Card5Title", "Card5Description", "Card5DetailText")
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

		autoScrollTimer = Dispatcher.CreateTimer();
		autoScrollTimer.Interval = TimeSpan.FromSeconds(4);
		autoScrollTimer.Tick += OnAutoScrollTick;

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

	public bool IsEnglishSelected => Localizer.CurrentLanguageCode == "en";

	public bool IsEstonianSelected => Localizer.CurrentLanguageCode == "et";

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
		Localizer.SetCulture("en");
	}

	private void OnEstonianClicked(object? sender, EventArgs e)
	{
		Localizer.SetCulture("et");
	}

	private async void OnCardTapped(object? sender, TappedEventArgs e)
	{
		var cardView = sender as VisualElement ?? (sender as TapGestureRecognizer)?.Parent as VisualElement;

		if (cardView?.BindingContext is not GreenTechCard card)
		{
			return;
		}

		var tappedCardIndex = Cards.IndexOf(card);

		if (tappedCardIndex < 0)
		{
			return;
		}

		if (tappedCardIndex != currentPosition)
		{
			currentPosition = tappedCardIndex;
			MainCarousel.ScrollTo(tappedCardIndex, position: ScrollToPosition.Center, animate: true);
			UpdatePreviewCards();
			return;
		}

		StopAutoScroll();
		await DisplayAlertAsync(Localizer["AlertTitle"], card.DetailText, Localizer["AlertClose"]);
		StartAutoScroll();
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
		OnPropertyChanged(nameof(IsEnglishSelected));
		OnPropertyChanged(nameof(IsEstonianSelected));
		OnPropertyChanged(nameof(HeroTitleText));
		OnPropertyChanged(nameof(HeroSubtitleText));
		OnPropertyChanged(nameof(LanguageLabelText));
		OnPropertyChanged(nameof(LanguageEnglishText));
		OnPropertyChanged(nameof(LanguageEstonianText));
		OnPropertyChanged(nameof(SectionLabelText));
		ReloadCards();
	}

	private void OnAutoScrollTick(object? sender, EventArgs e)
	{
		if (Cards.Count <= 1)
		{
			return;
		}

		currentPosition = WrapIndex(currentPosition + 1);
		MainCarousel.ScrollTo(currentPosition, position: ScrollToPosition.Center, animate: true);
	}

	private void ReloadCards()
	{
		currentPosition = Math.Clamp(currentPosition, 0, Math.Max(CardDefinitions.Length - 1, 0));
		var localizedCards = new ObservableCollection<GreenTechCard>();

		foreach (var card in CardDefinitions)
		{
			localizedCards.Add(new GreenTechCard(
				card.ImageSource,
				Localizer[card.TitleKey],
				Localizer[card.DescriptionKey],
				Localizer[card.DetailKey]));
		}

		Cards = localizedCards;
		Title = HeroTitleText;

		UpdatePreviewCards();
		ApplyPreviewProgress(0);

		if (Cards.Count > 0)
		{
			MainCarousel.Position = currentPosition;
		}
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

	private void FitCardTextLabel(Label label)
	{
		if (cardTextSizingInProgress.Contains(label) ||
			!cardTextBaseSizes.TryGetValue(label, out var baseSize) ||
			string.IsNullOrWhiteSpace(label.Text))
		{
			return;
		}

		var availableWidth = label.Width;
		var availableHeight = label.Height;

		if (label.Parent is VisualElement container)
		{
			availableWidth = Math.Max(availableWidth, container.Width);
			availableHeight = Math.Max(availableHeight, container.Height);
		}

		if (availableWidth <= 0 || availableHeight <= 0)
		{
			return;
		}

		cardTextSizingInProgress.Add(label);

		try
		{
			label.FontSize = baseSize;

			var minimumSize = label.AutomationId == "CardTitle"
				? baseSize * 0.66
				: baseSize * 0.72;

			var fittedSize = baseSize;

			for (var fontSize = baseSize; fontSize >= minimumSize; fontSize -= 0.5)
			{
				label.FontSize = fontSize;
				var request = label.Measure(availableWidth, double.PositiveInfinity);

				if (request.Height <= availableHeight + 0.5)
				{
					fittedSize = fontSize;
					break;
				}

				fittedSize = fontSize - 0.5;
			}

			label.FontSize = Math.Max(minimumSize, fittedSize);
		}
		finally
		{
			cardTextSizingInProgress.Remove(label);
		}
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

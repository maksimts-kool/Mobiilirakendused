using Tund2.TicTacToe;
using Tund2.MemoGame;
using Tund2.CityExplorer.Services;
using Tund2.CityExplorer.ViewModels;
using Tund2.CityExplorer.Views;

namespace Tund2;

public partial class StartPage : ContentPage
{
	private readonly List<VisualElement> animatedElements = new();
	private readonly List<(MenuPage Page, Border Card)> featuredCardViews = new();
	private readonly IDispatcherTimer autoScrollTimer;
	private MenuPage[] featuredPages = Array.Empty<MenuPage>();
	private int featuredPageCount;
	private bool hasPlayedIntro;
	private bool isNavigating;

	public StartPage()
	{
		InitializeComponent();

		autoScrollTimer = Dispatcher.CreateTimer();
		autoScrollTimer.Interval = TimeSpan.FromSeconds(4);
		autoScrollTimer.Tick += OnAutoScrollTick;

		var pages = new MenuPage[]
		{
			new("Tekst", "Töö tekstiga", () => new TextPage(0), "TE", "#F1F6FF", "#3B68D8"),
			new("Kujund", "Erinevad kujundid", () => new FigurePage(1), "KU", "#FFF7E8", "#CE7A16"),
			new("Taimer", "Taimeri kasutamine", () => new Timer_Page(), "TA", "#ECFBF5", "#12875B"),
			new("Valgusfoor", "Klassikaline valgusfoor", () => new ValgusfoorPage(), "VA", "#FFF0F0", "#CC3D3D"),
			new("DateTime", "Kuupäev ja kellaaeg", () => new DateTimePage(), "DT", "#F4F0FF", "#6D4BC7"),
			new("Slider", "Liugurid ja väärtused", () => new StepperSliderPage(), "SL", "#EEF8FF", "#22769E"),
			new("Lumememm", "Lumememme joonistamine", () => new LumememmPage(), "LU", "#EBFAFF", "#247EA0"),
			new("Pop-up aknad", "Dialoogiaknad ja hüpikud", () => new Pop_Up_Page(), "PU", "#FFF2FA", "#B94886"),
			new("Kohvikonstruktor", "Ideaalse kohvi tellimine", () => new KohvikPage(), "KO", "#FFF4EC", "#A65D24"),
			new("Minu retseptiraamat", "Menüü retseptide loomiseks, vaatamiseks ja kustutamiseks", () => new RecipeBookMenuPage(), "MR", "#F0FAEF", "#2F8D43"),
			new("Euroopa riigid", "Riikide vaatamine, lisamine, muutmine ja kustutamine", () => new EuroopaRiigidPage(), "ER", "#EEF6FF", "#276FBF"),
			new("Trips-Traps-Trull", "Klassikaline lauamäng", () => new TicTacToeMenuPage(), "TT", "#F7F1FF", "#7A4BD6"),
			new("Memo", "Mälumäng teemade, punktide ja animatsioonidega", () => new MemoGamePage(), "ME", "#FFF7ED", "#F97316"),
			new("Maitsealbum", "Lihtne failiga retseptileht piltide ja kategooriatega", () => new MaitseAlbumPage(), "MA", "#ECFDF5", "#0F766E"),
			new("CityExplorer", "Nutikas Tallinna giid karusselli, lemmikute ja keelevalikuga", () =>
			{
				var databaseService = new DatabaseService();
				return new MainTabbedPage(
					new ExplorePage(new ExploreViewModel(databaseService)),
					new FavoritesPage(new FavoritesViewModel(databaseService)),
					new SettingsPage(new SettingsViewModel()));
			}, "CE", "#E8F7F7", "#165B66")
		};

		featuredPages = pages.TakeLast(3).ToArray();
		var historyPages = pages.Except(featuredPages).ToArray();

		featuredPageCount = featuredPages.Length;
		FeaturedCarousel.ItemsSource = featuredPages;
		BindableLayout.SetItemsSource(HistoryList, historyPages);

		PrepareIntroElement(HeaderLayout, 18);
		PrepareIntroElement(FeaturedTitle, 14);
		PrepareIntroElement(FeaturedCarousel, 28);
		PrepareIntroElement(HistoryTitle, 14);
		PrepareIntroElement(HistoryList, 20);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (hasPlayedIntro)
		{
			StartAutoScroll();
			return;
		}

		hasPlayedIntro = true;
		await PlayIntroAnimationAsync();
		StartAutoScroll();
	}

	protected override void OnDisappearing()
	{
		StopAutoScroll();
		base.OnDisappearing();
	}

	private void PrepareIntroElement(VisualElement element, double startY)
	{
		element.Opacity = 0;
		element.TranslationY = startY;
		animatedElements.Add(element);
	}

	private async Task PlayIntroAnimationAsync()
	{
		var animationTasks = new List<Task>();

		for (var i = 0; i < animatedElements.Count; i++)
		{
			var element = animatedElements[i];
			animationTasks.Add(AnimateIntroElementAsync(element, i * 45));
		}

		await Task.WhenAll(animationTasks);
	}

	private async Task AnimateIntroElementAsync(VisualElement element, int delay)
	{
		await Task.Delay(delay);
		await Task.WhenAll(
			element.FadeToAsync(1, 360, Easing.CubicOut),
			element.TranslateToAsync(0, 0, 360, Easing.CubicOut));
	}

	private async void OnMenuItemTapped(object? sender, TappedEventArgs e)
	{
		if (e.Parameter is not MenuPage page)
		{
			return;
		}

		var tappedView = sender switch
		{
			VisualElement element => element,
			Element { Parent: VisualElement parent } => parent,
			_ => null
		};

		await NavigateWithAnimationAsync(tappedView, page);
	}

	private async Task NavigateWithAnimationAsync(VisualElement? view, MenuPage page)
	{
		if (isNavigating)
		{
			return;
		}

		isNavigating = true;
		StopAutoScroll();

		try
		{
			if (view is not null)
			{
				await Task.WhenAll(
					view.ScaleToAsync(0.97, 90, Easing.CubicOut),
					view.FadeToAsync(0.82, 90, Easing.CubicOut));
				await Task.WhenAll(
					view.ScaleToAsync(1, 150, Easing.SpringOut),
					view.FadeToAsync(1, 150, Easing.CubicOut));
			}

			await Navigation.PushAsync(page.CreatePage());
		}
		finally
		{
			isNavigating = false;
		}
	}

	private void StartAutoScroll()
	{
		if (featuredPageCount <= 1 || autoScrollTimer.IsRunning)
		{
			return;
		}

		autoScrollTimer.Start();
	}

	private void StopAutoScroll()
	{
		if (autoScrollTimer.IsRunning)
		{
			autoScrollTimer.Stop();
		}
	}

	private void OnAutoScrollTick(object? sender, EventArgs e)
	{
		if (featuredPageCount <= 1 || isNavigating || FeaturedCarousel.IsDragging)
		{
			return;
		}

		var nextPosition = (GetFeaturedPosition() + 1) % featuredPageCount;
		FeaturedCarousel.ScrollTo(nextPosition, position: ScrollToPosition.Center, animate: true);
	}

	private void OnFeaturedPositionChanged(object? sender, PositionChangedEventArgs e)
	{
		UpdateFeaturedCardScales(true);
	}

	private void OnFeaturedCardLoaded(object? sender, EventArgs e)
	{
		if (sender is not Border card || card.BindingContext is not MenuPage page)
		{
			return;
		}

		featuredCardViews.RemoveAll(item => ReferenceEquals(item.Card, card));
		featuredCardViews.Add((page, card));
		UpdateFeaturedCardScales(false);
	}

	private void OnFeaturedCardUnloaded(object? sender, EventArgs e)
	{
		if (sender is Border card)
		{
			featuredCardViews.RemoveAll(item => ReferenceEquals(item.Card, card));
		}
	}

	private int GetFeaturedPosition()
	{
		if (featuredPageCount == 0)
		{
			return 0;
		}

		var position = FeaturedCarousel.Position % featuredPageCount;
		return position < 0 ? position + featuredPageCount : position;
	}

	private void UpdateFeaturedCardScales(bool animate)
	{
		if (featuredPageCount == 0)
		{
			return;
		}

		var selectedPage = featuredPages[GetFeaturedPosition()];

		foreach (var (page, card) in featuredCardViews.ToArray())
		{
			var isSelected = page == selectedPage;
			var targetScale = isSelected ? 1.0 : 0.94;
			var targetOpacity = isSelected ? 1.0 : 0.72;

			if (animate)
			{
				_ = Task.WhenAll(
					card.ScaleToAsync(targetScale, 220, Easing.CubicOut),
					card.FadeToAsync(targetOpacity, 220, Easing.CubicOut));
				continue;
			}

			card.Scale = targetScale;
			card.Opacity = targetOpacity;
		}
	}

	private sealed record MenuPage(
		string Name,
		string Description,
		Func<Page> CreatePage,
		string Initials,
		Color BackgroundColor,
		Color AccentColor)
	{
		public MenuPage(
			string name,
			string description,
			Func<Page> createPage,
			string initials,
			string backgroundColor,
			string accentColor)
			: this(
				name,
				description,
				createPage,
				initials,
				Color.FromArgb(backgroundColor),
				Color.FromArgb(accentColor))
		{
		}
	}
}

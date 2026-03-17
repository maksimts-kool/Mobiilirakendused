namespace Tund2;

using Microsoft.Maui.Controls.Shapes; // For RoundRectangle

public partial class StartPage : ContentPage
{
	public StartPage()
	{
		InitializeComponent();
		Title = "Minu Projektid";

		var pages = new (string Name, string Description, Func<ContentPage> CreatePage, string Icon)[]
		{
("Tekst", "Töö tekstiga", () => new TextPage(0), "📝"),
("Kujund", "Erinevad kujundid", () => new FigurePage(1), "🟡"),
("Taimer", "Taimeri kasutamine", () => new Timer_Page(), "⏱️"),
("Valgusfoor", "Klassikaline valgusfoor", () => new ValgusfoorPage(), "🚦"),
("DateTime", "Kuupäev ja kellaaeg", () => new DateTimePage(), "📅"),
("Slider", "Liugurid ja väärtused", () => new StepperSliderPage(), "🎚️"),
("Lumememm", "Lumememme joonistamine", () => new LumememmPage(), "⛄"),
("Pop-up aknad", "Dialoogiaknad ja hüpikud", () => new Pop_Up_Page(), "💬"),
("Kohvikonstruktor", "Ideaalse kohvi tellimine", () => new KohvikPage(), "☕"),
("Trips-Traps-Trull", "Klassikaline lauamäng", () => new TicTacToePage(), "❌")
		};

		var layout = new VerticalStackLayout
		{
			Spacing = 15,
			Padding = new Thickness(20)
		};

		var header = new Label
		{
			Text = "Vali rakendus",
			FontSize = 28,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 10, 0, 20)
		};
		layout.Children.Add(header);

		foreach (var item in pages)
		{
			var border = new Border
			{
				StrokeShape = new RoundRectangle
				{
					CornerRadius = new CornerRadius(15)
				},
				Padding = new Thickness(15),
				Stroke = Colors.Transparent,
				BackgroundColor = Color.FromArgb("#f1f2f6"),
				Shadow = new Shadow
				{
					Brush = Brush.Black,
					Offset = new Point(0, 4),
					Opacity = 0.2f,
					Radius = 5
				}
			};

			var grid = new Grid
			{
				ColumnDefinitions =
{
new ColumnDefinition { Width = new GridLength(50) },
new ColumnDefinition { Width = GridLength.Star }
}
			};

			var iconLabel = new Label
			{
				Text = item.Icon,
				FontSize = 24,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};
			grid.Add(iconLabel, 0, 0);

			var textLayout = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center
			};

			var titleLabel = new Label
			{
				Text = item.Name,
				FontSize = 18,
				FontAttributes = FontAttributes.Bold,
				TextColor = Color.FromArgb("#2f3542")
			};

			var descLabel = new Label
			{
				Text = item.Description,
				FontSize = 14,
				TextColor = Color.FromArgb("#57606f")
			};

			textLayout.Children.Add(titleLabel);
			textLayout.Children.Add(descLabel);

			grid.Add(textLayout, 1, 0);

			border.Content = grid;

			var tapGesture = new TapGestureRecognizer();
			bool isNavigating = false;
			tapGesture.Tapped += async (s, e) =>
			{
				if (isNavigating) return;
				isNavigating = true;
				try
				{
					await border.ScaleToAsync(0.95, 100);
					await border.ScaleToAsync(1, 100);
					await Navigation.PushAsync(item.CreatePage());
				}
				finally
				{
					isNavigating = false;
				}
			};
			border.GestureRecognizers.Add(tapGesture);

			layout.Children.Add(border);
		}

		Content = new ScrollView { Content = layout };
	}
}

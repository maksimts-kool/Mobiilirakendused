using Microsoft.Maui.Controls.Shapes;

namespace Tund2;

public partial class FigurePage : ContentPage
{
	Border bw;
	Polygon triangle;
	Random rnd = new Random();
	Grid nupudGrid;

	List<string> buttons = new List<string> { "Tagasi", "Avaleht", "Edasi" };

	public FigurePage(int k)
	{
		InitializeComponent();
		Title = "Kujundi leht";

		int r = rnd.Next(0, 255);
		int g = rnd.Next(0, 255);
		int b = rnd.Next(0, 255);

		bw = new Border
		{
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 20 },
			BackgroundColor = Color.FromRgb(r, g, b),
			WidthRequest = 200,
			HeightRequest = 200,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		TapGestureRecognizer tap = new TapGestureRecognizer();
		tap.Tapped += Klik_boksi_peal;
		bw.GestureRecognizers.Add(tap);

		triangle = new Polygon
		{
			Points = new PointCollection
			{
				new Point(100, 0),
				new Point(0, 200),
				new Point(200, 200)
			},
			Fill = Color.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		TapGestureRecognizer triTap = new TapGestureRecognizer();
		triTap.Tapped += Triangle_Tapped;
		triangle.GestureRecognizers.Add(triTap);

		nupudGrid = new Grid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star }
			},
			ColumnSpacing = 10,
			Padding = new Thickness(10, 0),
			HorizontalOptions = LayoutOptions.Fill
		};

		for (int i = 0; i < buttons.Count; i++)
		{
			Button nupp = new Button
			{
				Text = buttons[i],
				ZIndex = i,
			};

			nupudGrid.Add(nupp, i, 0);
			nupp.Clicked += Liikumine;
		}

		VerticalStackLayout vsl = new VerticalStackLayout
		{
			Children = { bw, triangle, nupudGrid },
			VerticalOptions = LayoutOptions.Center,
			Spacing = 50
		};

		Content = vsl;
	}

	private void Klik_boksi_peal(object? sender, TappedEventArgs e)
	{
		bw.BackgroundColor = Color.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));

		bw.WidthRequest += 20;
		bw.HeightRequest += 20;

		var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
		var screenWidthUnits = mainDisplayInfo.Width / mainDisplayInfo.Density;

		if (bw.WidthRequest > (screenWidthUnits - 20))
		{
			bw.HeightRequest = 200;
			bw.WidthRequest = 200;
		}
	}

	private void Triangle_Tapped(object? sender, TappedEventArgs e)
	{
		triangle.Fill = Color.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
		triangle.Rotation += 10;
		if (triangle.Rotation >= 360) triangle.Rotation = 0;
	}

	private async void Liikumine(object? sender, EventArgs e)
	{
		if (sender is Button btn)
		{
			if (btn.ZIndex == 0)
				await Navigation.PushAsync(new TextPage(btn.ZIndex));
			else if (btn.ZIndex == 1)
				await Navigation.PushAsync(new StartPage());
			else
				await Navigation.PushAsync(new FigurePage(btn.ZIndex));
		}
	}
}
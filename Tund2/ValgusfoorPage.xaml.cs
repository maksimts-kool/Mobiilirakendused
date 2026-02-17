using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls.Shapes;

namespace Tund2
{
	public partial class ValgusfoorPage : ContentPage
	{
		Label headerLabel;
		Border redLight, yellowLight, greenLight;
		Button onButton, offButton, autoButton, nightButton;
		Slider brightnessSlider;
		Label brightnessLabel;
		bool isTrafficLightOn = false; // Hoiab meeles, kas foor on sees
		bool isAutoMode = false; // Automaatrežiimi jaoks
		bool isNightMode = false; // Öörežiimi jaoks
		DateTime lastRedPress = DateTime.MinValue; // Punase tule vajutamise aeg

		public ValgusfoorPage()
		{
			Title = "Valgusfoor";

			// 1. Pealkiri (Label)
			headerLabel = new Label
			{
				Text = "Vali valgus",
				FontSize = 24,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Colors.Black
			};

			// 2. Loome foori tuled
			redLight = CreateLight("Punane");
			yellowLight = CreateLight("Kollane");
			greenLight = CreateLight("Roheline");

			// 3. Nupud
			onButton = new Button
			{
				Text = "Sisse",
				BackgroundColor = Colors.Green,
				TextColor = Colors.White,
				WidthRequest = 100
			};
			onButton.Clicked += OnSwitchOn;

			offButton = new Button
			{
				Text = "Välja",
				BackgroundColor = Colors.Red,
				TextColor = Colors.White,
				WidthRequest = 100
			};
			offButton.Clicked += OnSwitchOff;

			autoButton = new Button
			{
				Text = "Auto",
				BackgroundColor = Colors.Blue,
				TextColor = Colors.White,
				WidthRequest = 100
			};
			autoButton.Clicked += OnAutoMode;

			// Öörežiimi nupp
			nightButton = new Button
			{
				Text = "Öö",
				BackgroundColor = Colors.Purple,
				TextColor = Colors.White,
				WidthRequest = 100
			};
			nightButton.Clicked += OnNightMode;

			var buttonsLayout = new FlexLayout
			{
				Children = { onButton, offButton, autoButton, nightButton },
				Wrap = FlexWrap.Wrap,
				JustifyContent = FlexJustify.Center,
				AlignItems = FlexAlignItems.Center,
				Direction = FlexDirection.Row,
				HorizontalOptions = LayoutOptions.Center
			};

			// Lisame nuppudele veidi marginaali FlexLayouti sees
			foreach (var view in buttonsLayout.Children)
			{
				if (view is Button btn)
				{
					btn.Margin = new Thickness(5);
				}
			}

			// 3.5 Heleduse regulaator
			brightnessLabel = new Label
			{
				Text = "Heledus: 100%",
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 10, 0, 0)
			};

			brightnessSlider = new Slider
			{
				Minimum = 0.1,
				Maximum = 1.0,
				Value = 1.0,
				WidthRequest = 200,
				HorizontalOptions = LayoutOptions.Center
			};
			brightnessSlider.ValueChanged += (s, e) =>
			{
				double val = e.NewValue;
				redLight.Opacity = val;
				yellowLight.Opacity = val;
				greenLight.Opacity = val;
				brightnessLabel.Text = $"Heledus: {Math.Round(val * 100)}%";
			};

			// 4. Peamine paigutus (VerticalStackLayout)
			Content = new VerticalStackLayout
			{
				Children = {
					headerLabel,
					redLight,
					yellowLight,
					greenLight,
					buttonsLayout,
					brightnessLabel,
					brightnessSlider
				},
				Spacing = 15,
				VerticalOptions = LayoutOptions.Center,
				Padding = new Thickness(20)
			};
		}

		private Border CreateLight(string colorName)
		{
			Border lightBorder = new Border
			{
				WidthRequest = 100,
				HeightRequest = 100,
				StrokeShape = new RoundRectangle { CornerRadius = 50 }, // Teeb ruudust ringi
				Background = Colors.Gray, // Alguses hall
				Stroke = Colors.Black,
				HorizontalOptions = LayoutOptions.Center
			};

			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += async (s, e) =>
			{
				if (!isTrafficLightOn)
				{
					headerLabel.Text = "Lülita esmalt foor sisse!";
					// Väike animatsioon, et anda märku veast
					await lightBorder.TranslateToAsync(-5, 0, 50);
					await lightBorder.TranslateToAsync(5, 0, 50);
					await lightBorder.TranslateToAsync(0, 0, 50);
					return;
				}

				// Kui foor on sees, muuda teksti ja tee animatsioon
				string text = "";
				if (colorName == "Punane")
				{
					text = "Seisa";
					lastRedPress = DateTime.Now;
				}
				else if (colorName == "Kollane")
				{
					// Kui punast vajutati vähem kui 1 sekundit tagasi
					if (DateTime.Now - lastRedPress < TimeSpan.FromSeconds(1))
					{
						text = "Valmista";
					}
					else
					{
						text = "Tähelepanu";
					}
				}
				else if (colorName == "Roheline")
				{
					text = "Sõida";
				}

				headerLabel.Text = text;

				// Animatsioon
				await Task.WhenAll(
					lightBorder.ScaleToAsync(1.2, 150),
					lightBorder.FadeToAsync(0.5, 150)
				);
				await Task.WhenAll(
					lightBorder.ScaleToAsync(1.0, 150),
					lightBorder.FadeToAsync(1.0, 150)
				);
			};

			lightBorder.GestureRecognizers.Add(tapGesture);
			return lightBorder;
		}

		// Nuppude sündmused (Events)
		private void OnSwitchOn(object? sender, EventArgs e)
		{
			isTrafficLightOn = true;
			isAutoMode = false;
			isNightMode = false;
			headerLabel.Text = "Vali valgus";

			redLight.Background = Colors.Red;
			yellowLight.Background = Colors.Yellow;
			greenLight.Background = Colors.Green;
		}

		private void OnSwitchOff(object? sender, EventArgs e)
		{
			isTrafficLightOn = false;
			isAutoMode = false;
			isNightMode = false;
			headerLabel.Text = "Foor on väljas";

			ResetColors();
		}

		// Automaatrežiim (tsükkel)
		private async void OnAutoMode(object? sender, EventArgs e)
		{
			if (isAutoMode) return;

			isTrafficLightOn = true;
			isAutoMode = true;
			isNightMode = false;
			headerLabel.Text = "Automaatrežiim";

			while (isAutoMode)
			{
				// 1. Punane
				ResetColors();
				redLight.Background = Colors.Red;
				await CountdownDelay(3, "Seisa");
				if (!isAutoMode) break;

				// 2. Punane ja Kollane
				yellowLight.Background = Colors.Yellow;
				await CountdownDelay(2, "Valmista");
				if (!isAutoMode) break;

				// 3. Roheline
				ResetColors();
				greenLight.Background = Colors.Green;
				await CountdownDelay(3, "Sõida");
				if (!isAutoMode) break;

				// 4. Roheline vilgub 3 korda
				headerLabel.Text = "Varsti muutub...";
				for (int i = 0; i < 3; i++)
				{
					greenLight.Background = Colors.Gray;
					await Task.Delay(400);
					if (!isAutoMode) break;
					greenLight.Background = Colors.Green;
					await Task.Delay(400);
					if (!isAutoMode) break;
				}
				if (!isAutoMode) break;

				// 5. Kollane
				ResetColors();
				yellowLight.Background = Colors.Yellow;
				await CountdownDelay(2, "Tähelepanu");
				if (!isAutoMode) break;
			}
		}

		private async Task CountdownDelay(int seconds, string statusText)
		{
			for (int i = seconds; i > 0; i--)
			{
				if (!isAutoMode) return;
				headerLabel.Text = $"{statusText} ({i}s)";
				await Task.Delay(1000);
			}
		}

		private async void OnNightMode(object? sender, EventArgs e)
		{
			if (isNightMode) return;

			isTrafficLightOn = true;
			isAutoMode = false;
			isNightMode = true;
			headerLabel.Text = "Öörežiim";

			while (isNightMode)
			{
				ResetColors();
				yellowLight.Background = Colors.Yellow;
				await Task.Delay(400);
				if (!isNightMode) break;

				yellowLight.Background = Colors.Gray;
				await Task.Delay(400);
				if (!isNightMode) break;
			}
		}

		private void ResetColors()
		{
			redLight.Background = Colors.Gray;
			yellowLight.Background = Colors.Gray;
			greenLight.Background = Colors.Gray;
		}
	}
}
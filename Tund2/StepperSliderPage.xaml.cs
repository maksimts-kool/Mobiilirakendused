namespace Tund2;

public partial class StepperSliderPage : ContentPage
{
	public StepperSliderPage()
	{
		InitializeComponent();
		UpdateColor();
	}

	private void OnColorSliderChanged(object sender, ValueChangedEventArgs e)
	{
		UpdateColor();
	}

	private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
	{
		lblCornerRadius.Text = e.NewValue.ToString();
		ColorBox.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
		{
			CornerRadius = (float)e.NewValue
		};
	}

	private void UpdateColor()
	{
		int r = (int)sldRed.Value;
		int g = (int)sldGreen.Value;
		int b = (int)sldBlue.Value;

		// Uuendame Label tekstid
		lblRedValue.Text = r.ToString();
		lblGreenValue.Text = g.ToString();
		lblBlueValue.Text = b.ToString();

		// Määrame Borderile uue taustavärvi
		ColorBox.Background = Color.FromRgb(r, g, b);

		// Uuendame individuaalsed värvi-indikaatorid
		boxRed.Background = Color.FromRgb(r, 0, 0);
		boxGreen.Background = Color.FromRgb(0, g, 0);
		boxBlue.Background = Color.FromRgb(0, 0, b);

		lblHex.Text = $"#{r:X2}{g:X2}{b:X2}";

		if (r + g + b > 380)
			lblHex.TextColor = Colors.Black;
		else
			lblHex.TextColor = Colors.White;
	}

	private async void OnRandomColorClicked(object sender, EventArgs e)
	{
		Random rnd = new Random();

		// Genereerime uued väärtused
		int targetR = rnd.Next(0, 256);
		int targetG = rnd.Next(0, 256);
		int targetB = rnd.Next(0, 256);

		// Animeerime liugurid (peame arvestama inversiooniga: sld = 255 - target)
		sldRed.Value = targetR;
		await Task.Delay(50);
		sldGreen.Value = targetG;
		await Task.Delay(50);
		sldBlue.Value = targetB;
	}
}
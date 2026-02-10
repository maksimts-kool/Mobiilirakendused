namespace Tund2;

public partial class Timer_Page : ContentPage
{
	bool on_off = false;

	public Timer_Page()
	{
		InitializeComponent();
	}
	private async void ShowTime()
	{
		while (on_off)
		{
			timer_btn.Text = DateTime.Now.ToString("T");
			await Task.Delay(1000);
		}
	}

	private void timer_btn_Clicked(object sender, EventArgs e)
	{
		if (on_off)
		{
			on_off = false;
			timer_btn.Text = "Näita aega";
		}
		else
		{
			on_off = true;
			ShowTime();
		}
	}

	private async void tagasi_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		await DisplayAlert("Info", "See on label, mida saab vajutada!", "OK");
	}
}
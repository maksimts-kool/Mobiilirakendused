namespace Tund2;

public partial class DateTimePage : ContentPage
{
	public DateTimePage()
	{
		InitializeComponent();
		UpdateLabel();
	}

	// Sündmus: DateSelected (DatePicker)
	private void OnDateSelected(object sender, DateChangedEventArgs e)
	{
		UpdateLabel();
	}

	// Sündmus: PropertyChanged (TimePicker - jälgime Time omadust)
	private void OnTimePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "Time")
		{
			UpdateLabel();
		}
	}

	private void UpdateLabel()
	{
		// Kombineerime kuupäeva ja aja
		DateTime fullDate = (datePicker.Date ?? DateTime.Today) + (timePicker.Time ?? TimeSpan.Zero);
		lblResult.Text = $"Valitud aeg:\n{fullDate.ToString("dd.MM.yyyy HH:mm")}";
	}
}
namespace Tund2;

public partial class LumememmPage : ContentPage
{
    private bool isMelted = false;
    private bool isHidden = false;
    private bool isCycleRunning = false;

    public LumememmPage()
    {
        InitializeComponent();
    }

    // näita valitud tegevuse nimi Label-is
    private void OnPickerChanged(object sender, EventArgs e)
    {
        if (actionPicker.SelectedItem is string selected)
            actionLabel.Text = selected;
    }

    // muuda läbipaistvust
    private void OnOpacityChanged(object sender, ValueChangedEventArgs e)
    {
        if (!isHidden && !isMelted)
            snowmanLayout.Opacity = e.NewValue;
    }

    // uuenda kiiruse Label-it
    private void OnSpeedChanged(object sender, ValueChangedEventArgs e)
    {
        speedLabel.Text = $"{(int)e.NewValue} ms";
    }

    // käivita valitud tegevus
    private async void OnExecuteClicked(object sender, EventArgs e)
    {
        if (actionPicker.SelectedItem is not string selected)
        {
            await DisplayAlertAsync("Viga", "Palun vali esmalt tegevus Pickerist!", "OK");
            return;
        }

        int speed = (int)speedStepper.Value;

        switch (selected)
        {
            case "Peida lumememm":
                HideSnowman();
                break;
            case "Näita lumememm":
                ShowSnowman();
                break;
            case "Muuda värvi":
                await ChangeColorAsync();
                break;
            case "Sulata":
                await MeltSnowmanAsync(speed);
                break;
        }
    }

    private async void OnDayNightSwitchToggled(object sender, ToggledEventArgs e)
    {
        isCycleRunning = e.Value;

        if (isCycleRunning)
        {
            await RunDayNightCycle();
        }
    }

    private double currentTimeHour = 12.0;

    private async Task RunDayNightCycle()
    {
        while (isCycleRunning)
        {
            int tickDelay = 40; // fps
            double timeStep = 10.0 / ((int)speedStepper.Value * 4.0 / tickDelay);

            // Uuenda kellaaega
            currentTimeHour += timeStep;
            if (currentTimeHour >= 24) currentTimeHour -= 24;

            // Arvuta kellaaeg
            int h = (int)currentTimeHour;
            int m = (int)((currentTimeHour - h) * 60);
            timeLabel.Text = $"{h:D2}:{m:D2}";

            // Arvuta öö tagatausta läbipaistvus (0 = päev, 1 = öö)
            double targetOpacity = 0.0;

            if (currentTimeHour >= 18.0 && currentTimeHour < 24.0)
            {
                targetOpacity = (currentTimeHour - 18.0) / 6.0;
            }
            else if (currentTimeHour >= 0.0 && currentTimeHour < 6.0)
            {
                targetOpacity = 1.0;
            }
            else if (currentTimeHour >= 6.0 && currentTimeHour < 12.0)
            {
                targetOpacity = 1.0 - ((currentTimeHour - 6.0) / 6.0);
            }
            else
            {
                targetOpacity = 0.0;
            }

            boxBackgroundNight.Opacity = targetOpacity;

            await Task.Delay(tickDelay);
        }
    }

    // Peida
    private void HideSnowman()
    {
        snowmanLayout.IsVisible = false;
        boxBackgroundDay.Source = "snowmelt.png";
        boxBackgroundNight.Source = "snowmeltnight.png";
        isHidden = true;
        isMelted = false;
        actionLabel.Text = "Lumememm on peidetud";
    }

    // Näita
    private void ShowSnowman()
    {
        snowmanLayout.IsVisible = true;
        snowmanLayout.Opacity = opacitySlider.Value;
        snowmanLayout.ScaleX = 1;
        snowmanLayout.ScaleY = 1;
        snowmanLayout.TranslationY = 0;
        boxBackgroundDay.Source = "snownormal.png";
        boxBackgroundNight.Source = "snownormalnight.png";

        isHidden = false;
        isMelted = false;
        actionLabel.Text = "Lumememm on tagasi!";
    }

    // Muuda värvi 
    private async Task ChangeColorAsync()
    {
        if (isHidden)
        {
            await DisplayAlertAsync("Info", "Lumememm on peidetud! Vajuta 'Näita lumememm' enne värvimist.", "OK");
            return;
        }

        var rng = new Random();

        var newColor = Color.FromRgb(
            rng.Next(180, 256),
            rng.Next(180, 256),
            rng.Next(180, 256));

        bodyFrame.BackgroundColor = newColor;
        headFrame.BackgroundColor = newColor;
        actionLabel.Text = "Värv muudetud!";
    }

    // Sulata
    private async Task MeltSnowmanAsync(int durationMs)
    {
        if (isMelted)
        {
            await DisplayAlertAsync("Info", "Lumememm on juba sulanud! Vajuta 'Näita lumememm', et taastada.", "OK");
            return;
        }
        if (isHidden)
        {
            await DisplayAlertAsync("Info", "Lumememm on peidetud! Vajuta 'Näita lumememm' enne sulatamist.", "OK");
            return;
        }

        isMelted = true;
        actionLabel.Text = "Lumememm sulab... 💧";

        uint duration = (uint)(durationMs * 2);

        var tasks = Task.WhenAll(
            snowmanLayout.FadeToAsync(0, duration, Easing.CubicIn),
            snowmanLayout.ScaleToAsync(0.2, duration, Easing.CubicIn)
        );
        boxBackgroundDay.Source = "snowmelt.png";
        boxBackgroundNight.Source = "snowmeltnight.png";
        await tasks;

        snowmanLayout.IsVisible = false;

        snowmanLayout.Opacity = opacitySlider.Value;
        snowmanLayout.Scale = 1;

        actionLabel.Text = "Lumememm on sulanud";
    }
}

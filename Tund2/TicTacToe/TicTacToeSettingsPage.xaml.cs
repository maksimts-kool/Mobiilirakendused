namespace Tund2.TicTacToe;

public partial class TicTacToeSettingsPage : ContentPage
{
    private const string BoardSizePreferenceKey = "TTT_BoardSize";
    private const string BotDifficultyPreferenceKey = "TTT_BotDifficulty";
    private const string EasyDifficulty = "Easy";
    private const string MediumDifficulty = "Medium";
    private const string HardDifficulty = "Hard";

    private readonly Color selectedFillColor = Color.FromArgb("#37B726");
    private readonly Color selectedStrokeColor = Color.FromArgb("#14E934");
    private readonly Color unselectedFillColor = Color.FromArgb("#FF9900");
    private readonly Color unselectedStrokeColor = Color.FromArgb("#BC7100");

    private int pendingBoardSize;
    private string pendingDifficulty = MediumDifficulty;

    public TicTacToeSettingsPage()
    {
        InitializeComponent();

        pendingBoardSize = Preferences.Default.Get(BoardSizePreferenceKey, 3);
        pendingDifficulty = Preferences.Default.Get(BotDifficultyPreferenceKey, MediumDifficulty);

        ApplySelectionStates();
    }

    private void OnBoardSize3(object? sender, TappedEventArgs e) => SetBoardSize(3);
    private void OnBoardSize4(object? sender, TappedEventArgs e) => SetBoardSize(4);
    private void OnBoardSize5(object? sender, TappedEventArgs e) => SetBoardSize(5);

    private void OnDifficultyEasy(object? sender, TappedEventArgs e) => SetDifficulty(EasyDifficulty);
    private void OnDifficultyMedium(object? sender, TappedEventArgs e) => SetDifficulty(MediumDifficulty);
    private void OnDifficultyHard(object? sender, TappedEventArgs e) => SetDifficulty(HardDifficulty);

    private void SetBoardSize(int size)
    {
        pendingBoardSize = size;
        ApplySelectionStates();
    }

    private void SetDifficulty(string difficulty)
    {
        pendingDifficulty = difficulty;
        ApplySelectionStates();
    }

    private void ApplySelectionStates()
    {
        UpdateOptionButton(BoardSize3Button, pendingBoardSize == 3);
        UpdateOptionButton(BoardSize4Button, pendingBoardSize == 4);
        UpdateOptionButton(BoardSize5Button, pendingBoardSize == 5);

        UpdateOptionButton(DifficultyEasyButton, pendingDifficulty == EasyDifficulty);
        UpdateOptionButton(DifficultyMediumButton, pendingDifficulty == MediumDifficulty);
        UpdateOptionButton(DifficultyHardButton, pendingDifficulty == HardDifficulty);
    }

    private void UpdateOptionButton(Border border, bool isSelected)
    {
        border.BackgroundColor = isSelected ? selectedFillColor : unselectedFillColor;
        border.Stroke = new SolidColorBrush(isSelected ? selectedStrokeColor : unselectedStrokeColor);
        border.StrokeThickness = 2;
        border.Shadow = isSelected ? CreateSelectionShadow() : CreateDefaultShadow();
    }

    private static Shadow CreateSelectionShadow()
    {
        return new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#662AFF00")),
            Offset = new Point(0, 0),
            Radius = 6,
            Opacity = 1
        };
    }

    private static Shadow CreateDefaultShadow()
    {
        return new Shadow
        {
            Brush = new SolidColorBrush(Colors.Transparent),
            Offset = new Point(0, 0),
            Radius = 0,
            Opacity = 0
        };
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        Preferences.Default.Set(BoardSizePreferenceKey, pendingBoardSize);
        Preferences.Default.Set(BotDifficultyPreferenceKey, pendingDifficulty);

        await NavigateBackAsync();
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        if (!HasUnsavedChanges())
        {
            await NavigateBackAsync();
            return;
        }

        bool discardChanges = await DisplayAlertAsync(
            "Salvestamata muudatused",
            "Sa ei ole muudatusi salvestanud. Kui lähed tagasi ja vajutad \"Jah\", siis need muudatused ei rakendu.",
            "Jah",
            "Ei");

        if (discardChanges)
        {
            await NavigateBackAsync();
        }
    }

    protected override bool OnBackButtonPressed()
    {
        _ = HandleBackNavigationAsync();
        return true;
    }

    private async Task HandleBackNavigationAsync()
    {
        if (!HasUnsavedChanges())
        {
            await NavigateBackAsync();
            return;
        }

        bool discardChanges = await DisplayAlertAsync(
            "Salvestamata muudatused",
            "Sa ei ole muudatusi salvestanud. Kui lähed tagasi ja vajutad \"Jah\", siis need muudatused ei rakendu.",
            "Jah",
            "Ei");

        if (discardChanges)
        {
            await NavigateBackAsync();
        }
    }

    private bool HasUnsavedChanges()
    {
        int savedBoardSize = Preferences.Default.Get(BoardSizePreferenceKey, 3);
        string savedDifficulty = Preferences.Default.Get(BotDifficultyPreferenceKey, MediumDifficulty);

        return pendingBoardSize != savedBoardSize || pendingDifficulty != savedDifficulty;
    }

    private async Task NavigateBackAsync()
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }
}

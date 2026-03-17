namespace Tund2;

public partial class TicTacToeSettingsPage : ContentPage
{
    private int selectedBoardSize;
    private string selectedDifficulty;

    private readonly Color activeColor = Color.FromArgb("#6E00B8");
    private readonly Color inactiveColor = Color.FromArgb("#F54066");

    public TicTacToeSettingsPage()
    {
        InitializeComponent();
        selectedBoardSize = Preferences.Default.Get("TTT_BoardSize", 3);
        selectedDifficulty = Preferences.Default.Get("TTT_BotDifficulty", "Medium");
        UpdateBoardSizeButtons();
        UpdateDifficultyButtons();
    }

    // ===== Board size =====

    private void OnBoardSize3(object? sender, TappedEventArgs e) => SetBoardSize(3);
    private void OnBoardSize4(object? sender, TappedEventArgs e) => SetBoardSize(4);
    private void OnBoardSize5(object? sender, TappedEventArgs e) => SetBoardSize(5);

    private void SetBoardSize(int size)
    {
        selectedBoardSize = size;
        Preferences.Default.Set("TTT_BoardSize", size);
        UpdateBoardSizeButtons();
    }

    private void UpdateBoardSizeButtons()
    {
        btn3x3.BackgroundColor = selectedBoardSize == 3 ? activeColor : inactiveColor;
        btn4x4.BackgroundColor = selectedBoardSize == 4 ? activeColor : inactiveColor;
        btn5x5.BackgroundColor = selectedBoardSize == 5 ? activeColor : inactiveColor;
    }

    // ===== Bot difficulty =====

    private void OnDifficultyEasy(object? sender, TappedEventArgs e) => SetDifficulty("Easy");
    private void OnDifficultyMedium(object? sender, TappedEventArgs e) => SetDifficulty("Medium");
    private void OnDifficultyHard(object? sender, TappedEventArgs e) => SetDifficulty("Hard");

    private void SetDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        Preferences.Default.Set("TTT_BotDifficulty", difficulty);
        UpdateDifficultyButtons();
    }

    private void UpdateDifficultyButtons()
    {
        btnEasy.BackgroundColor = selectedDifficulty == "Easy" ? activeColor : inactiveColor;
        btnMedium.BackgroundColor = selectedDifficulty == "Medium" ? activeColor : inactiveColor;
        btnHard.BackgroundColor = selectedDifficulty == "Hard" ? activeColor : inactiveColor;
    }

    // ===== Navigation =====

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}

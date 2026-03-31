namespace Tund2;

public partial class RecipeBookPage : ContentPage
{
    public RecipeBookPage()
    {
        InitializeComponent();

        CookingDatePicker.Date = DateTime.Today;
        DifficultyPicker.SelectedIndex = 1;

        RecipeNameEntryCell.Text = "Minu retsept";
        DishTypeEntryCell.Text = "Pearoog";
        DescriptionEntryCell.Text = "Lihtne ja maitsev kodune roog";
        AuthorEntryCell.Text = "minu lemmikroog";

        Ingredient1EntryCell.Text = "Kanafilee";
        Ingredient2EntryCell.Text = "Paprika";
        Ingredient3EntryCell.Text = "Koor";
        Ingredient4EntryCell.Text = "Juust";
        Ingredient5EntryCell.Text = "Maitseained";

        InstructionEditor.Text = "Lõika koostisosad. Küpseta peamised ained pannil. Lisa maitseained ja serveeri soojalt.";

        UpdateCookingTimeLabel(CookingTimeSlider.Value);
        UpdatePortionsLabel(PortionsStepper.Value);
        UpdateExtraSection(false);
        UpdateExtraInfoTexts();
    }

    private void OnCookingTimeChanged(object? sender, ValueChangedEventArgs e)
    {
        UpdateCookingTimeLabel(e.NewValue);
        UpdateExtraInfoTexts();
    }

    private void OnPortionsChanged(object? sender, ValueChangedEventArgs e)
    {
        UpdatePortionsLabel(e.NewValue);
        UpdateExtraInfoTexts();
    }

    private void OnExtraInfoChanged(object? sender, ToggledEventArgs e)
    {
        UpdateExtraInfoTexts();
        UpdateExtraSection(e.Value);
    }

    private void UpdateCookingTimeLabel(double value)
    {
        CookingTimeValueLabel.Text = $"{Math.Round(value)} min";
    }

    private void UpdatePortionsLabel(double value)
    {
        var portions = (int)Math.Round(value);
        PortionsValueLabel.Text = portions == 1 ? "1 portsjon" : $"{portions} portsjonit";
    }

    private void UpdateExtraSection(bool isVisible)
    {
        ExtraInfoSection.IsVisible = isVisible;
    }

    private void UpdateExtraInfoTexts()
    {
        var portions = (int)Math.Round(PortionsStepper.Value);
        var caloriesPerPortion = SweetDishSwitchCell.IsToggled ? 520 : VegetarianSwitchCell.IsToggled ? 310 : 420;
        var totalCalories = caloriesPerPortion * Math.Max(1, portions);

        ServingTipTextCell.Text = SweetDishSwitchCell.IsToggled
            ? "Serveeri marjade, mündi või tuhksuhkruga."
            : "Serveeri värske salati, maitserohelise või sooja lisandiga.";

        CaloriesTextCell.Text = $"Umbes {caloriesPerPortion} kcal portsjoni kohta, kokku {totalCalories} kcal";
        ExtraNoteTextCell.Text = VegetarianSwitchCell.IsToggled
            ? "Sobib taimetoidu sõpradele ja kergeks õhtusöögiks."
            : SweetDishSwitchCell.IsToggled
                ? "Sobib magustoiduks, kohvilauale või nädalavahetuseks."
                : "Sobib lõuna- või õhtusöögiks ning perega jagamiseks.";
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "Retsept salvestatud",
            $"Retsept \"{GetRecipeName()}\" on edukalt salvestatud.",
            "OK");
    }

    private async void OnSummaryClicked(object? sender, EventArgs e)
    {
        UpdateExtraInfoTexts();

        await DisplayAlertAsync(
            "Retsepti kokkuvõte",
            BuildSummary(),
            "Sulge");
    }

    private async void OnClearClicked(object? sender, EventArgs e)
    {
        RecipeNameEntryCell.Text = string.Empty;
        DishTypeEntryCell.Text = string.Empty;
        DescriptionEntryCell.Text = string.Empty;
        AuthorEntryCell.Text = string.Empty;

        Ingredient1EntryCell.Text = string.Empty;
        Ingredient2EntryCell.Text = string.Empty;
        Ingredient3EntryCell.Text = string.Empty;
        Ingredient4EntryCell.Text = string.Empty;
        Ingredient5EntryCell.Text = string.Empty;

        CookingDatePicker.Date = DateTime.Today;
        CookingTimeSlider.Value = 30;
        PortionsStepper.Value = 1;
        DifficultyPicker.SelectedIndex = 0;
        VegetarianSwitchCell.IsToggled = false;
        SweetDishSwitchCell.IsToggled = false;

        InstructionEditor.Text = string.Empty;

        ExtraInfoSwitchCell.IsToggled = false;
        UpdateExtraInfoTexts();
        UpdateExtraSection(false);

        await DisplayAlertAsync(
            "Vorm puhastatud",
            "Kõik retsepti väljad on tühjendatud.",
            "OK");
    }

    private string BuildSummary()
    {
        var type = string.IsNullOrWhiteSpace(DishTypeEntryCell.Text) ? "Määramata" : DishTypeEntryCell.Text.Trim();
        var difficulty = DifficultyPicker.SelectedIndex >= 0 ? DifficultyPicker.Items[DifficultyPicker.SelectedIndex] : "Määramata";
        var portions = (int)Math.Round(PortionsStepper.Value);
        var recipeFlags = new List<string>();

        if (VegetarianSwitchCell.IsToggled)
        {
            recipeFlags.Add("taimetoit");
        }

        if (SweetDishSwitchCell.IsToggled)
        {
            recipeFlags.Add("magus roog");
        }

        var flagsText = recipeFlags.Count > 0 ? string.Join(", ", recipeFlags) : "tavaline roog";

        return $"Retsept: {GetRecipeName()}\n" +
               $"Tüüp: {type}\n" +
               $"Aeg: {Math.Round(CookingTimeSlider.Value)} min\n" +
               $"Portsjonid: {portions}\n" +
               $"Raskusaste: {difficulty}\n" +
               $"Omadused: {flagsText}";
    }

    private string GetRecipeName()
    {
        return string.IsNullOrWhiteSpace(RecipeNameEntryCell.Text)
            ? "Minu retsept"
            : RecipeNameEntryCell.Text.Trim();
    }
}

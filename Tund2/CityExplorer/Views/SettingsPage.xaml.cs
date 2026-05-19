using Tund2.CityExplorer.ViewModels;

namespace Tund2.CityExplorer.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

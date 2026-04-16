using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace Tund2;

public partial class App : Application
{
	public ObservableCollection<RecipeData> Recipes { get; } = new();

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var startPage = new StartPage();
		var navPage = new NavigationPage(startPage)
		{
			BarBackgroundColor = Colors.Blue,
			BarTextColor = Colors.White
		};
		return new Window(navPage);
	}
}

using Microsoft.Extensions.Logging;
using Tund2.CityExplorer.Services;
using Tund2.CityExplorer.ViewModels;
using Tund2.CityExplorer.Views;

namespace Tund2;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("NunitoSans-Regular.ttf", "NunitoSansRegular");
				fonts.AddFont("NunitoSans-Italic.ttf", "NunitoSansItalic");
				fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
				fonts.AddFont("Poppins-Medium.ttf", "PoppinsMedium");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<DatabaseService>();

		builder.Services.AddTransient<ExploreViewModel>();
		builder.Services.AddTransient<FavoritesViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();

		builder.Services.AddTransient<ExplorePage>();
		builder.Services.AddTransient<FavoritesPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<MainTabbedPage>();

		return builder.Build();
	}
}

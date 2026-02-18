namespace Tund2;

public partial class StartPage : ContentPage
{
	VerticalStackLayout layout;
	ScrollView scrollView;
	public List<ContentPage> pages = new List<ContentPage>()
{
	new TextPage(0),
	new FigurePage(1),
	new Timer_Page(),
	new ValgusfoorPage(),
	new DateTimePage(),
	new StepperSliderPage()
};
	public List<string> pageNames = new List<string>() { "Tekst", "Kujund", "Taimer", "Valgusfoor", "DateTime", "Slider" };
	public StartPage()
	{
		InitializeComponent();
		Title = "Avaleht";
		layout = new VerticalStackLayout()
		{
			Spacing = 25,
			Padding = new Thickness(30, 0),
			VerticalOptions = LayoutOptions.Center
		};
		for (int i = 0; i < pages.Count; i++)
		{
			var button = new Button() { Text = pageNames[i], FontFamily = "NunitoSansRegular", FontSize = 18, FontAttributes = FontAttributes.Bold };
			int index = i;
			button.Clicked += (sender, args) =>
			{
				Navigation.PushAsync(pages[index]);
			};
			layout.Children.Add(button);
		}
		scrollView = new ScrollView() { Content = layout };
		Content = scrollView;

	}
}
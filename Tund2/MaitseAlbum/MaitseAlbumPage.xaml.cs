namespace Tund2;

public partial class MaitseAlbumPage : TabbedPage
{
    public MaitseAlbumPage()
    {
        SetAppleTabColors();
        InitializeComponent();
    }

    private static void SetAppleTabColors()
    {
#if IOS || MACCATALYST
        var selectedColor = UIKit.UIColor.FromRGB(15, 118, 110);
        var normalColor = UIKit.UIColor.FromRGB(51, 65, 85);
        var selectedBackground = UIKit.UIColor.FromRGB(236, 253, 245);

        UIKit.UITabBar.Appearance.TintColor = selectedColor;
        UIKit.UITabBar.Appearance.UnselectedItemTintColor = normalColor;

        UIKit.UITabBarItem.Appearance.SetTitleTextAttributes(
            new UIKit.UIStringAttributes { ForegroundColor = selectedColor },
            UIKit.UIControlState.Selected);
        UIKit.UITabBarItem.Appearance.SetTitleTextAttributes(
            new UIKit.UIStringAttributes { ForegroundColor = normalColor },
            UIKit.UIControlState.Normal);

        UIKit.UISegmentedControl.Appearance.SelectedSegmentTintColor = selectedBackground;
        UIKit.UISegmentedControl.Appearance.SetTitleTextAttributes(
            new UIKit.UIStringAttributes { ForegroundColor = selectedColor },
            UIKit.UIControlState.Selected);
        UIKit.UISegmentedControl.Appearance.SetTitleTextAttributes(
            new UIKit.UIStringAttributes { ForegroundColor = normalColor },
            UIKit.UIControlState.Normal);
#endif
    }
}

namespace Tund2.CityExplorer.Views;

internal static class FavoriteIconAnimator
{
    public static async Task PopAsync(VisualElement icon)
    {
        await Task.WhenAll(
            icon.ScaleToAsync(0.72, 90, Easing.CubicOut),
            icon.RotateToAsync(-10, 90, Easing.CubicOut),
            icon.FadeToAsync(0.65, 90, Easing.CubicOut));

        await Task.WhenAll(
            icon.ScaleToAsync(1.12, 150, Easing.SpringOut),
            icon.RotateToAsync(8, 150, Easing.CubicOut),
            icon.FadeToAsync(1, 150, Easing.CubicOut));

        await Task.WhenAll(
            icon.ScaleToAsync(1, 90, Easing.CubicOut),
            icon.RotateToAsync(0, 90, Easing.CubicOut));
    }
}

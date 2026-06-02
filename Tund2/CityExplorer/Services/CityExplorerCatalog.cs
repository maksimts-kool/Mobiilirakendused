using Microsoft.Maui.Graphics;
using Tund2.CityExplorer.Models;

namespace Tund2.CityExplorer.Services;

internal static class CityExplorerCatalog
{
    private const string DefaultTourCategoryColor = "#8BE3C3";
    private const string DefaultTourCategoryIcon = "★";

    private static readonly CategoryDefinition[] CategoryDefinitions =
    [
        new("history", "⭐", "★", "CategoryHistory", "#F1D38B", "#F7F7F7", "#FFF5DF", "#E4B752"),
        new("parks", "🌳", "♣", "CategoryParks", "#8BE3C3", "#DFF8EF", "#E7FAF2", "#69D8B1"),
        new("food", "🍽", "◆", "CategoryFood", "#FFC78E", "#FFF3E6", "#FFF0E0", "#F2A85E")
    ];

    private static readonly PlaceDefinition[] PlaceDefinitions =
    [
        new(1, "history", "cityexplorer_toompea.jpg", "PlaceToompeaName", "PlaceToompeaShort", "PlaceToompeaDetail", "4,9", "TourPriceHistory", "TourDistanceCenter", "TourTagHistory"),
        new(2, "history", "cityexplorer_oldtown.jpg", "PlaceOldTownName", "PlaceOldTownShort", "PlaceOldTownDetail", "4,8", "TourPriceHistory", "TourDistanceCenter", "TourTagHistory"),
        new(3, "parks", "cityexplorer_kadriorg.jpg", "PlaceKadriorgName", "PlaceKadriorgShort", "PlaceKadriorgDetail", "5,0", "TourPriceNature", "TourDistanceQuiet", "TourTagNature"),
        new(4, "parks", "cityexplorer_pirita.jpg", "PlacePiritaName", "PlacePiritaShort", "PlacePiritaDetail", "4,7", "TourPriceNature", "TourDistanceSea", "TourTagNature"),
        new(5, "food", "cityexplorer_market.jpg", "PlaceMarketName", "PlaceMarketShort", "PlaceMarketDetail", "4,9", "TourPriceFood", "TourDistanceCenter", "TourTagFood"),
        new(6, "food", "cityexplorer_telliskivi.jpg", "PlaceTelliskiviName", "PlaceTelliskiviShort", "PlaceTelliskiviDetail", "4,8", "TourPriceFood", "TourDistanceTram", "TourTagFood")
    ];

    public static IReadOnlyList<Category> CreateCategories()
    {
        return CategoryDefinitions.Select(CreateCategory).ToArray();
    }

    public static IReadOnlyList<Place> CreatePlaces()
    {
        return PlaceDefinitions.Select(CreatePlace).ToArray();
    }

    public static FavoriteCategoryGroup CreateFavoriteGroup(string categoryKey)
    {
        var definition = FindCategoryDefinition(categoryKey) ?? CategoryDefinitions[0];

        return new FavoriteCategoryGroup
        {
            CategoryKey = categoryKey,
            TitleKey = definition.TitleKey,
            Icon = definition.Icon,
            HeaderColor = CreateColor(definition.AccentColor),
            BodyColor = CreateColor(definition.FavoriteBodyColor),
            ItemBorderColor = CreateColor(definition.FavoriteBorderColor)
        };
    }

    public static string GetTourCategoryTitle(string categoryKey, LocalizationManager localizer)
    {
        var definition = FindCategoryDefinition(categoryKey);
        return definition is null
            ? localizer["PlacesLabel"]
            : localizer[definition.TitleKey];
    }

    public static string GetTourCategoryIcon(string categoryKey)
    {
        return FindCategoryDefinition(categoryKey)?.Icon ?? DefaultTourCategoryIcon;
    }

    public static Color GetTourCategoryColor(string categoryKey)
    {
        var color = FindCategoryDefinition(categoryKey)?.AccentColor ?? DefaultTourCategoryColor;
        return CreateColor(color);
    }

    public static string GetCurrentImageName(int placeId, string savedImage)
    {
        return PlaceDefinitions.FirstOrDefault(place => place.Id == placeId)?.Image ?? savedImage;
    }

    private static Category CreateCategory(CategoryDefinition definition)
    {
        return new Category
        {
            Key = definition.Key,
            Emoji = definition.Emoji,
            Icon = definition.Icon,
            TitleKey = definition.TitleKey,
            AccentColor = CreateColor(definition.AccentColor),
            SoftColor = CreateColor(definition.ExploreSoftColor)
        };
    }

    private static Place CreatePlace(PlaceDefinition definition)
    {
        return new Place
        {
            Id = definition.Id,
            CategoryKey = definition.CategoryKey,
            Image = definition.Image,
            NameKey = definition.NameKey,
            ShortDescriptionKey = definition.ShortDescriptionKey,
            DetailKey = definition.DetailKey,
            Rating = definition.Rating,
            PriceTextKey = definition.PriceTextKey,
            DistanceTextKey = definition.DistanceTextKey,
            TagTextKey = definition.TagTextKey
        };
    }

    private static CategoryDefinition? FindCategoryDefinition(string categoryKey)
    {
        return CategoryDefinitions.FirstOrDefault(definition => definition.Key == categoryKey);
    }

    private static Color CreateColor(string color)
    {
        return Color.FromArgb(color);
    }

    private sealed record CategoryDefinition(
        string Key,
        string Emoji,
        string Icon,
        string TitleKey,
        string AccentColor,
        string ExploreSoftColor,
        string FavoriteBodyColor,
        string FavoriteBorderColor);

    private sealed record PlaceDefinition(
        int Id,
        string CategoryKey,
        string Image,
        string NameKey,
        string ShortDescriptionKey,
        string DetailKey,
        string Rating,
        string PriceTextKey,
        string DistanceTextKey,
        string TagTextKey);
}

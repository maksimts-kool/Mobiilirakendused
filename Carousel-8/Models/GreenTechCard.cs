namespace Carousel_8.Models;

public sealed class GreenTechCard
{
    public GreenTechCard(
        string imageSource,
        string title,
        string description,
        string detailText)
    {
        ImageSource = imageSource;
        Title = title;
        Description = description;
        DetailText = detailText;
    }

    public string ImageSource { get; }

    public string Title { get; }

    public string Description { get; }

    public string DetailText { get; }
}

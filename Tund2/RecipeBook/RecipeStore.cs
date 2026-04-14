using System.Collections.ObjectModel;

namespace Tund2;

public static class RecipeStore
{
    public static ObservableCollection<RecipeData> Recipes { get; } = new();

    public static RecipeData? GetById(Guid id)
    {
        return Recipes.FirstOrDefault(recipe => recipe.Id == id);
    }

    public static void Save(RecipeData recipe)
    {
        var index = Recipes
            .Select((item, itemIndex) => new { item, itemIndex })
            .FirstOrDefault(x => x.item.Id == recipe.Id)?
            .itemIndex ?? -1;

        if (index >= 0)
        {
            Recipes[index] = recipe;
        }
        else
        {
            Recipes.Insert(0, recipe);
        }
    }

    public static void Delete(Guid id)
    {
        var recipe = GetById(id);

        if (recipe is not null)
        {
            Recipes.Remove(recipe);
        }
    }
}

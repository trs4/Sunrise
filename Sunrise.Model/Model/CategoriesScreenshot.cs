namespace Sunrise.Model;

public sealed class CategoriesScreenshot
{
    internal CategoriesScreenshot(List<Category> categories)
    {
        Categories = categories ?? throw new ArgumentNullException(nameof(categories));
        var categoriesById = new Dictionary<int, Category>(categories.Count);
        var categoriesByName = new Dictionary<string, Category>(categories.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            categoriesById.Add(category.Id, category);
            categoriesByName[category.Name] = category;
        }

        CategoriesById = categoriesById;
        CategoriesByName = categoriesByName;
    }

    public List<Category> Categories { get; }

    public Dictionary<int, Category> CategoriesById { get; }

    public Dictionary<string, Category> CategoriesByName { get; }

    public void Add(Category? category)
    {
        if (category is null)
            return;

        Categories.Add(category);
        CategoriesById[category.Id] = category;
        CategoriesByName[category.Name] = category;
    }

    public void Remove(Category? category)
    {
        if (category is null)
            return;

        Categories.Remove(category);
        CategoriesById.Remove(category.Id);
        CategoriesByName.Remove(category.Name);
    }

    public override string ToString() => $"Count: {CategoriesById.Count}";
}

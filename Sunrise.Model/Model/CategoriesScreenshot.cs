namespace Sunrise.Model;

public sealed class CategoriesScreenshot
{
    internal CategoriesScreenshot(List<Category> categories)
    {
        Categories = categories ?? throw new ArgumentNullException(nameof(categories));
        var categoriesById = new Dictionary<int, Category>(categories.Count);
        var categoriesByGuid = new Dictionary<Guid, Category>(categories.Count);
        var categoriesByName = new Dictionary<string, Category>(categories.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            categoriesById.Add(category.Id, category);
            categoriesByGuid.Add(category.Guid, category);
            categoriesByName[category.Name] = category;
        }

        CategoriesById = categoriesById;
        CategoriesByGuid = categoriesByGuid;
        CategoriesByName = categoriesByName;
    }

    public List<Category> Categories { get; }

    public Dictionary<int, Category> CategoriesById { get; }

    public Dictionary<Guid, Category> CategoriesByGuid { get; }

    public Dictionary<string, Category> CategoriesByName { get; }

    public void Add(Category? category)
    {
        if (category is null)
            return;

        Categories.Add(category);
        CategoriesById[category.Id] = category;
        CategoriesByGuid[category.Guid] = category;
        CategoriesByName[category.Name] = category;
    }

    public void Remove(Category? category)
    {
        if (category is null)
            return;

        Categories.Remove(category);
        CategoriesById.Remove(category.Id);
        CategoriesByGuid.Remove(category.Guid);
        CategoriesByName.Remove(category.Name);
    }

    public void Remove(IReadOnlyCollection<int>? categoryIds)
    {
        if (categoryIds is null || categoryIds.Count == 0)
            return;

        foreach (int categoryId in categoryIds)
        {
            if (!CategoriesById.TryGetValue(categoryId, out var category))
                continue;

            Categories.Remove(category);
            CategoriesById.Remove(category.Id);
            CategoriesByGuid.Remove(category.Guid);
            CategoriesByName.Remove(category.Name);
        }
    }

    public override string ToString() => $"Count: {CategoriesById.Count}";
}

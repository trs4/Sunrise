namespace Sunrise.Model;

/// <summary>Категория</summary>
public class Category
{
    /// <summary>Идентификатор</summary>
    public int Id { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    /// <summary>Наименование</summary>
    public string Name { get; set; }

    public override string ToString() => Name;
}

namespace Sunrise.Model.Model.Exchange;

/// <summary>Категория</summary>
public class CategoryElement
{
    /// <summary>Наименование</summary>
    public string Name { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    public override string ToString() => Name;
}

namespace Sunrise.Model.Model.Exchange;

/// <summary>Список воспроизведения</summary>
public class PlaylistElement
{
    /// <summary>Наименование</summary>
    public string Name { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    /// <summary>Создано</summary>
    public DateTime Created { get; set; }

    /// <summary>Обновлено</summary>
    public DateTime Updated { get; set; }

    /// <summary>Треки</summary>
    public List<Guid>? Tracks { get; set; }

    /// <summary>Категории</summary>
    public List<Guid>? Categories { get; set; }

    public override string ToString() => Name;
}

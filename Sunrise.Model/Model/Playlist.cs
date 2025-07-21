namespace Sunrise.Model;

/// <summary>Список воспроизведения</summary>
public class Playlist
{
    /// <summary>Идентификатор</summary>
    public int Id { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    /// <summary>Наименование</summary>
    public string Name { get; set; }

    /// <summary>Создано</summary>
    public DateTime Created { get; set; }

    /// <summary>Треки</summary>
    public List<Track> Tracks { get; set; }

    /// <summary>Категории</summary>
    public List<Category> Categories { get; set; }

    public override string ToString() => Name;
}

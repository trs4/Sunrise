namespace Sunrise.Model.Model.Exchange;

/// <summary>Медиатека</summary>
public class MediaDocument
{
    /// <summary>Наименование машины</summary>
    public string Name { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    /// <summary>Версия</summary>
    public int Version { get; set; }

    /// <summary>Создано</summary>
    public DateTime Date { get; set; }

    /// <summary>Папки</summary>
    public List<string>? Folders { get; set; }

    /// <summary>Треки</summary>
    public List<TrackElement>? Tracks { get; set; }

    /// <summary>Плейлисты</summary>
    public List<PlaylistElement>? Playlists { get; set; }

    /// <summary>Категории</summary>
    public List<CategoryElement>? Categories { get; set; }

    public override string ToString() => Name;
}

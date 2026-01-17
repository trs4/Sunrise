namespace Sunrise.Model.Model.Exchange;

/// <summary>Трек</summary>
public class TrackElement
{
    /// <summary>Название</summary>
    public string Title { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    /// <summary>Путь до файла</summary>
    public string Path { get; set; }

    /// <summary>Год</summary>
    public int? Year { get; set; }

    /// <summary>Длительность</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Рейтинг</summary>
    public byte Rating { get; set; }

    /// <summary>Артист</summary>
    public string Artist { get; set; }

    /// <summary>Жанр</summary>
    public string Genre { get; set; }

    /// <summary>Последнее воспроизведение</summary>
    public DateTime? LastPlay { get; set; }

    /// <summary>Воспроизведено</summary>
    public int Reproduced { get; set; }

    /// <summary>Альбом</summary>
    public string Album { get; set; }

    /// <summary>Создано</summary>
    public DateTime Created { get; set; }

    /// <summary>Добавлено</summary>
    public DateTime Added { get; set; }

    /// <summary>Обновлено</summary>
    public DateTime Updated { get; set; }

    /// <summary>Битрейт</summary>
    public int Bitrate { get; set; }

    /// <summary>Размер</summary>
    public long Size { get; set; }

    /// <summary>Дата последнего изменения</summary>
    public DateTime LastWrite { get; set; }

    /// <summary>Имеется ли рисунок</summary>
    public bool HasPicture { get; set; }

    /// <summary>Текст</summary>
    public string OriginalText { get; set; }

    /// <summary>Перевод</summary>
    public string TranslateText { get; set; }

    public override string ToString() => $"{Artist} - {Title}";
}

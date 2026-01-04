namespace Sunrise.Model;

/// <summary>Трек</summary>
public class Track
{
    private Dictionary<string, object>? _extensions;

    /// <summary>Идентификатор</summary>
    public int Id { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    /// <summary>Путь до файла</summary>
    public string Path { get; set; }

    /// <summary>Выбран</summary>
    public bool Picked { get; set; }

    /// <summary>Название</summary>
    public string? Title { get; set; }

    /// <summary>Год</summary>
    public int? Year { get; set; }

    /// <summary>Длительность</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Рейтинг</summary>
    public byte Rating { get; set; }

    /// <summary>Артист</summary>
    public string? Artist { get; set; }

    /// <summary>Артисты</summary>
    public string? Artists { get; set; }

    /// <summary>Жанр</summary>
    public string? Genre { get; set; }

    /// <summary>Последнее воспроизведение</summary>
    public DateTime? LastPlay { get; set; }

    /// <summary>Воспроизведено</summary>
    public int Reproduced { get; set; }

    /// <summary>Воспроизведено в текущем приложении</summary>
    public int SelfReproduced { get; set; }

    /// <summary>Альбом</summary>
    public string? Album { get; set; }

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

    /// <summary>Рисунок</summary>
    public TrackPicture? Picture { get; set; }

    /// <summary>Рисунок</summary>
    public object? PictureIcon { get; set; }

    public object? this[string? key]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            return (_extensions ??= []).TryGetValue(key, out object value) ? value : null;
        }
        set
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value is not null)
                (_extensions ??= [])[key] = value;
            else if (_extensions is not null)
            {
                _extensions.Remove(key);

                if (_extensions.Count == 0)
                    _extensions = null;
            }
        }
    }

    public override string ToString() => $"{Artist} - {Title} {Year}";
}

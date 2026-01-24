using Sunrise.Model.Schemes;

namespace Sunrise.Model;

public enum PlaylistParameter
{
    /// <summary>Выбран</summary>
    Picked = Tracks.Picked,

    /// <summary>Название</summary>
    Title = Tracks.Title,

    /// <summary>Год</summary>
    Year = Tracks.Year,

    /// <summary>Длительность</summary>
    Duration = Tracks.Duration,

    /// <summary>Рейтинг</summary>
    Rating = Tracks.Rating,

    /// <summary>Артист</summary>
    Artist = Tracks.Artist,

    /// <summary>Жанр</summary>
    Genre = Tracks.Genre,

    /// <summary>Последнее воспроизведение</summary>
    LastPlay = Tracks.LastPlay,

    /// <summary>Воспроизведено</summary>
    Reproduced = Tracks.Reproduced,

    /// <summary>Альбом</summary>
    Album = Tracks.Album,

    /// <summary>Добавлено</summary>
    Added = Tracks.Added,

    /// <summary>Дата последнего изменения</summary>
    LastWrite = Tracks.LastWrite,
}

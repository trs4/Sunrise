using Sunrise.Model.Resources;
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

public static class PlaylistParameterExtensions
{
    public static string GetName(this PlaylistParameter parameter) => parameter switch
    {
        PlaylistParameter.Picked => Texts.Picked,
        PlaylistParameter.Title => Texts.Title,
        PlaylistParameter.Year => Texts.Year,
        PlaylistParameter.Duration => Texts.Duration,
        PlaylistParameter.Rating => Texts.Rating,
        PlaylistParameter.Artist => Texts.Artist,
        PlaylistParameter.Genre => Texts.Genre,
        PlaylistParameter.LastPlay => Texts.LastPlay,
        PlaylistParameter.Reproduced => Texts.Reproduced,
        PlaylistParameter.Album => Texts.Album,
        PlaylistParameter.Added => Texts.Added,
        PlaylistParameter.LastWrite => Texts.LastWrite,
        _ => throw new NotSupportedException(parameter.ToString()),
    };
}

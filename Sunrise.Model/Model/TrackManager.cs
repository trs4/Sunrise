namespace Sunrise.Model;

internal sealed class TrackManager
{
    public static bool TryCreate(FileInfo file, DateTime added, out Track? track)
    {
        try
        {
            track = Create(file, added);
            return track is not null;
        }
        catch
        {
            track = null;
            return false;
        }
    }

    public static Track? Create(FileInfo file, DateTime added)
    {
        using var tfile = TagLib.File.Create(file.FullName);

        if (tfile is null)
            return null;

        var properties = tfile.Properties;
        var pictures = tfile.Tag.Pictures;
        int year = (int)tfile.Tag.Year;

        var track = new Track()
        {
            Guid = Guid.NewGuid(),
            Path = file.FullName,
            Title = tfile.Tag.Title,
            Year = year > 0 ? year : null,
            Artist = tfile.Tag.JoinedPerformers ?? tfile.Tag.JoinedAlbumArtists,
            Genre = tfile.Tag.JoinedGenres,
            Album = tfile.Tag.Album,
            Created = file.CreationTime,
            Added = added,
            Size = file.Length,
            LastWrite = file.LastWriteTime,
        };

        if (string.IsNullOrWhiteSpace(track.Title))
            track.Title = GetTitle(file);

        if (properties is not null)
        {
            track.Duration = properties.Duration;
            track.Bitrate = properties.AudioBitrate;
        }

        if (pictures.Length > 0)
        {
            var picture = pictures[0];

            track.Picture = new TrackPicture()
            {
                MimeType = picture.MimeType,
                Data = picture.Data.Data,
            };

            track.HasPicture = true;
        }

        return track;
    }

    private static string GetTitle(FileInfo file)
    {
        string title = Path.GetFileNameWithoutExtension(file.Name);

        // Обрезаем цифры и точки, если есть после пробел
        // 00 test
        int index = 0;

        for (; index < title.Length; index++)
        {
            char s = title[index];

            if (char.IsDigit(s) || s == ' ' || char.IsPunctuation(s))
                continue;

            break;
        }

        if (index > 2)
            title = title.Substring(index);

        return title;
    }

}

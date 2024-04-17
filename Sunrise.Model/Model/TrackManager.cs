using Sunrise.Model.TagLib.Mpeg;

namespace Sunrise.Model;

internal sealed class TrackManager
{
    public static bool TryCreate(FileInfo file, DateTime added, out Track track)
    {
        if (!".mp3".Equals(file.Extension, StringComparison.OrdinalIgnoreCase))
        {
            track = new();
            return false; // %%TODO .m4a
        }

        try
        {
            track = Create(file, added);
            return true;
        }
        catch
        {
            track = new();
            return false;
        }
    }

    public static Track Create(FileInfo file, DateTime added)
    {
        using var tfile = new AudioFile(file.FullName);
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

}

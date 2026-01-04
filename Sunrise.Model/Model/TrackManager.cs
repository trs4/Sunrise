using System.Collections.Concurrent;
using Sunrise.Model.TagLib;

namespace Sunrise.Model;

internal sealed class TrackManager
{
    private static readonly string[] _artistSeparators = ["&", " ft ", " ft. ", " feat ", " feat. ", ",", " and ", " и ", " x ", " х ",
        " vs ", " vs. ", "+", "/\\", "/"];

    public static bool TryCreate(FileInfo file, DateTime added, out Track? track, ConcurrentDictionary<string, string> artistCache)
    {
        try
        {
            track = Create(file, added, artistCache);
            return track is not null;
        }
        catch
        {
            track = null;
            return false;
        }
    }

    public static Track? Create(FileInfo file, DateTime added, ConcurrentDictionary<string, string> artistCache)
    {
        string extension = file.Extension;

        if (string.IsNullOrEmpty(extension))
            return null;

        extension = extension.Substring(1);

        if (!SupportedMimeType.AllAudioExtensions.Contains(extension))
            return null;

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
            Updated = added,
            Size = file.Length,
            LastWrite = file.LastWriteTime,
        };

        if (string.IsNullOrWhiteSpace(track.Title))
            track.Title = GetTitle(file);

        if (!string.IsNullOrEmpty(track.Artist))
        {
            track.Artist = track.Artist.Trim();

            if (track.Artist.Length > 0)
                track.Artists = artistCache.GetOrAdd(track.Artist, a => GetArtists(file, a));
        }

        if (string.IsNullOrEmpty(track.Artist))
            track.Artist = null;

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

    private static string GetArtists(FileInfo file, string artist)
    {
        string folderName = file.Directory?.Name ?? string.Empty;
        var artists = new List<string>() { artist };

        foreach (string separator in _artistSeparators)
            SplitArtists(ref artists, separator, folderName);

        return artists.Count == 1 ? artists[0] : string.Join('|', artists);
    }

    private static void SplitArtists(ref List<string> artists, string separator, string folderName)
    {
        var tempArtists = new List<string>();

        foreach (string artist in artists)
        {
            int currentIndex = 0;
            int trimIndex = 0;
            bool found = false;

            while (true)
            {
                int index = artist.Length <= currentIndex + separator.Length ? -1 : artist.IndexOf(separator, currentIndex, StringComparison.OrdinalIgnoreCase);

                if (index == -1)
                {
                    if (found && artist.Length > currentIndex)
                        tempArtists.Add(artist.Substring(currentIndex).TrimStart());

                    break;
                }

                bool notFolder = (trimIndex + folderName.Length) < index
                    || !artist.Substring(trimIndex).StartsWith(folderName, StringComparison.OrdinalIgnoreCase);

                if (notFolder)
                    tempArtists.Add(artist.Substring(currentIndex, index - currentIndex).Trim());
                else
                {
                    bool add = artist.Substring(trimIndex + folderName.Length).StartsWith(separator, StringComparison.OrdinalIgnoreCase);

                    if (add)
                    {
                        string tempArtist = artist.Substring(trimIndex, folderName.Length);

                        if (!tempArtists.Contains(tempArtist))
                            tempArtists.Add(tempArtist);
                    }

                    currentIndex = trimIndex = trimIndex + folderName.Length + separator.Length;

                    if (add)
                        found = true;

                    continue;
                }

                currentIndex = index + separator.Length;
                found = true;

                if (notFolder)
                    trimIndex = currentIndex;
            }

            if (!found)
                tempArtists.Add(artist);
        }

        artists = tempArtists;
    }

}

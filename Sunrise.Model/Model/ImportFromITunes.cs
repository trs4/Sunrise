using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Sunrise.Model;

public static class ImportFromITunes
{
    public static async Task LoadAsync(Player player, string filePath, IProgress? progressOwner = null, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var xDocument = XDocument.Load(filePath);
        var elements = xDocument.Root?.Element("dict")?.Elements().ToList();

        if (elements is null)
            return;

        var tracks = new ConcurrentDictionary<int, Track>();
        string appName = null;
        var now = DateTime.Now;

        for (int i = 0; i < elements.Count; i++)
        {
            var element = elements[i];

            if (element.Name.LocalName != "key")
                continue;

            string name = element.Value;

            if (name == "Library Persistent ID")
            {
                var valueElement = elements[i + 1];

                if (valueElement.Name.LocalName == "string")
                    appName = valueElement.Value;
            }
            else if (name == "Tracks")
                await ParseTracksAsync(elements[i + 1], tracks, appName, now, player, progressOwner, token);
            else if (name == "Playlists")
                await ParsePlaylistsAsync(elements[i + 1], tracks, appName, now, player, progressOwner, token);
        }
    }

    private static async Task ParseTracksAsync(XElement xElement, ConcurrentDictionary<int, Track> tracks, string appName, DateTime now,
        Player player, IProgress? progressOwner, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(appName))
            return;

        var elements = xElement.Elements().ToList();
        var elementPairs = new List<(XElement KeyElement, XElement TrackElement)>(elements.Count / 2);

        for (int i = 0; i < elements.Count; i += 2)
            elementPairs.Add((elements[i], elements[i + 1]));

        var options = new ParallelOptions()
        {
            MaxDegreeOfParallelism = Math.Max((int)(Environment.ProcessorCount * 0.8), 1),
            CancellationToken = token,
        };

        Parallel.ForEach(elementPairs, options, p => ProcessTrack(p.KeyElement, p.TrackElement, tracks, appName, now));

        if (!tracks.IsEmpty)
            await player.AddAsync((IReadOnlyCollection<Track>)tracks.Values, progressOwner: progressOwner, withAppName: appName, token: token);
    }

    private static void ProcessTrack(XElement keyElement, XElement trackElement,
        ConcurrentDictionary<int, Track> tracks, string appName, DateTime now)
    {
        if (keyElement.Name.LocalName != "key" || trackElement.Name.LocalName != "dict" || !int.TryParse(keyElement.Value, out int id))
            return;

        var trackProperties = trackElement.Elements().ToList();
        FileInfo file = null;
        var added = now;
        int playCount = 0;
        DateTime? lastPlay = null;
        byte rating = 0;

        for (int j = 0; j < trackProperties.Count; j += 2)
        {
            string name = trackProperties[j].Value;
            var valueElement = trackProperties[j + 1];

            if (name == "Date Added")
            {
                if (DateTime.TryParse(valueElement.Value, out var value))
                    added = value;
            }
            else if (name == "Location")
            {
                string location = valueElement.Value;
                const string fileLocalhost = "file://localhost/";

                if (location.StartsWith(fileLocalhost))
                    location = location.Substring(fileLocalhost.Length);

                location = Uri.UnescapeDataString(location);
                file = new FileInfo(location);
            }
            else if (name == "Play Count")
            {
                if (int.TryParse(valueElement.Value, out int value) && value > 0)
                    playCount = value;
            }
            else if (name == "Play Date UTC")
            {
                if (DateTime.TryParse(valueElement.Value, out var value))
                    lastPlay = value;
            }
            else if (name == "Rating")
            {
                if (int.TryParse(valueElement.Value, out int value))
                    rating = (byte)(value / 20);
            }
        }

        if (file is null || !file.Exists)
            return;

        if (!TrackManager.TryCreate(file, added, out var track) || track is null)
            return;

        track.LastPlay = lastPlay;
        track.Rating = rating;
        track[appName] = playCount;
        tracks[id] = track;
    }

    private static async Task ParsePlaylistsAsync(XElement xElement, ConcurrentDictionary<int, Track> tracks, string appName, DateTime now,
        Player player, IProgress? progressOwner, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(appName))
            return;

        var playlists = new List<Playlist>();

        foreach (var playlistElement in xElement.Elements())
        {
            var playlist = new Playlist()
            {
                Guid = Guid.NewGuid(),
                Created = now,
                Tracks = [],
                Categories = [],
            };

            var playlistProperties = playlistElement.Elements().ToList();
            bool skip = false;

            for (int j = 0; j < playlistProperties.Count; j += 2)
            {
                string name = playlistProperties[j].Value;
                var valueElement = playlistProperties[j + 1];

                if (name == "Name")
                    playlist.Name = valueElement.Value;
                else if (name == "Smart Info" || name == "Smart Criteria" || name == "Visible" || name == "Distinguished Kind")
                {
                    skip = true;
                    break;
                }
                else if (name == "Playlist Items")
                {
                    foreach (var trackElement in valueElement.Elements())
                    {
                        if (int.TryParse(trackElement.Element("integer")?.Value, out int trackId) && tracks.TryGetValue(trackId, out var track))
                            playlist.Tracks.Add(track);
                    }
                }
            }

            if (skip || string.IsNullOrWhiteSpace(playlist.Name) || playlist.Tracks.Count == 0)
                continue;

            playlists.Add(playlist);
        }

        if (playlists.Count > 0)
            await player.AddAsync(playlists, progressOwner: progressOwner, token: token);
    }

}

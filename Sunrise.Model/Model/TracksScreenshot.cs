namespace Sunrise.Model;

public sealed class TracksScreenshot
{
    private Dictionary<Guid, Track>? _tracksByGuid;
    private Dictionary<string, Track>? _tracksByPath;
    private Dictionary<string, Dictionary<string, List<Track>>>? _tracksByArtist;
    private Dictionary<string, List<Track>>? _tracksByGenre;

    internal TracksScreenshot(List<Track> tracks)
    {
        Tracks = tracks ?? throw new ArgumentNullException(nameof(tracks));
        var tracksById = new Dictionary<int, Track>(tracks.Count);

        foreach (var track in tracks)
            tracksById.Add(track.Id, track);

        TracksById = tracksById;
    }

    public List<Track> Tracks { get; }

    public Dictionary<int, Track> TracksById { get; }

    public Dictionary<Guid, Track> TracksByGuid => _tracksByGuid ??= CreateTracksByGuid();

    public Dictionary<string, Track> TracksByPath => _tracksByPath ??= CreateTracksByPath();

    public Dictionary<string, Dictionary<string, List<Track>>> TracksByArtist => _tracksByArtist ??= CreateTracksByArtist();

    public Dictionary<string, List<Track>> TracksByGenre => _tracksByGenre ??= CreateTracksByGenre();

    private Dictionary<Guid, Track> CreateTracksByGuid()
    {
        var tracksByGuid = new Dictionary<Guid, Track>(Tracks.Count);

        foreach (var track in Tracks)
            tracksByGuid[track.Guid] = track;

        return tracksByGuid;
    }

    private Dictionary<string, Track> CreateTracksByPath()
    {
        var tracksByPath = new Dictionary<string, Track>(Tracks.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var track in Tracks)
            tracksByPath[track.Path] = track;

        return tracksByPath;
    }

    private Dictionary<string, Dictionary<string, List<Track>>> CreateTracksByArtist()
    {
        var tracksByArtist = new Dictionary<string, Dictionary<string, List<Track>>>(StringComparer.OrdinalIgnoreCase);

        foreach (var track in Tracks)
        {
            string artist = track.Artist;

            if (string.IsNullOrWhiteSpace(artist))
                continue;

            if (!tracksByArtist.TryGetValue(artist, out var tracksByAlbums))
                tracksByArtist.Add(artist, tracksByAlbums = new(StringComparer.OrdinalIgnoreCase));

            string album = track.Album;

            if (string.IsNullOrWhiteSpace(album))
                album = string.Empty;

            if (!tracksByAlbums.TryGetValue(album, out var tracks))
                tracksByAlbums.Add(album, tracks = []);

            tracks.Add(track);
        }

        return tracksByArtist;
    }

    private Dictionary<string, List<Track>> CreateTracksByGenre()
    {
        var tracksByGenre = new Dictionary<string, List<Track>>(StringComparer.OrdinalIgnoreCase);

        foreach (var track in Tracks)
        {
            string genre = track.Genre;

            if (string.IsNullOrWhiteSpace(genre))
                continue;

            if (!tracksByGenre.TryGetValue(genre, out var tracks))
                tracksByGenre.Add(genre, tracks = []);

            tracks.Add(track);
        }

        return tracksByGenre;
    }

    public void Remove(Track? track)
    {
        if (track is null)
            return;

        int trackId = track.Id;

        for (int i = Tracks.Count - 1; i >= 0; i--)
        {
            if (Tracks[i].Id == trackId)
            {
                Tracks.RemoveAt(i);
                break;
            }
        }

        TracksById.Remove(trackId);
        ClearCache();
    }

    public void Remove(IReadOnlyCollection<int>? trackIds)
    {
        if (trackIds is null || trackIds.Count == 0)
            return;

        var ids = (trackIds as HashSet<int>) ?? [.. trackIds];

        for (int i = Tracks.Count - 1; i >= 0; i--)
        {
            if (ids.Contains(Tracks[i].Id))
            {
                Tracks.RemoveAt(i);
                break;
            }
        }

        foreach (int trackId in ids)
            TracksById.Remove(trackId);

        ClearCache();
    }

    public void ClearCache()
    {
        _tracksByGuid = null;
        _tracksByPath = null;
        _tracksByArtist = null;
        _tracksByGenre = null;
    }

    public override string ToString() => $"Count: {Tracks.Count}";
}

namespace Sunrise.Model;

public sealed class TracksScreenshot
{
    private Dictionary<string, Track> _allTracksByPath;
    private Dictionary<string, Dictionary<string, List<Track>>> _allTracksByArtist;
    private Dictionary<string, List<Track>> _allTracksByGenre;

    internal TracksScreenshot(List<Track> allTracks, Dictionary<int, Track> allTracksById)
    {
        AllTracks = allTracks ?? throw new ArgumentNullException(nameof(allTracks));
        AllTracksById = allTracksById ?? throw new ArgumentNullException(nameof(allTracksById));
    }

    public List<Track> AllTracks { get; }

    public Dictionary<int, Track> AllTracksById { get; }

    public Dictionary<string, Track> AllTracksByPath
        => _allTracksByPath ??= CreateAllTracksByPath();

    public Dictionary<string, Dictionary<string, List<Track>>> AllTracksByArtist
        => _allTracksByArtist ??= CreateAllTracksByArtist();

    public Dictionary<string, List<Track>> AllTracksByGenre
        => _allTracksByGenre ??= CreateAllTracksByGenre();

    private Dictionary<string, Track> CreateAllTracksByPath()
    {
        var allTracksByPath = new Dictionary<string, Track>(AllTracks.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var track in AllTracks)
            allTracksByPath[track.Path] = track;

        return allTracksByPath;
    }

    private Dictionary<string, Dictionary<string, List<Track>>> CreateAllTracksByArtist()
    {
        var allTracksByArtist = new Dictionary<string, Dictionary<string, List<Track>>>(StringComparer.OrdinalIgnoreCase);

        foreach (var track in AllTracks)
        {
            string artist = track.Artist;

            if (string.IsNullOrWhiteSpace(artist))
                continue;

            if (!allTracksByArtist.TryGetValue(artist, out var tracksByAlbums))
                allTracksByArtist.Add(artist, tracksByAlbums = new(StringComparer.OrdinalIgnoreCase));

            string album = track.Album;

            if (string.IsNullOrWhiteSpace(album))
                album = string.Empty;

            if (!tracksByAlbums.TryGetValue(album, out var tracks))
                tracksByAlbums.Add(album, tracks = []);

            tracks.Add(track);
        }

        return allTracksByArtist;
    }

    private Dictionary<string, List<Track>> CreateAllTracksByGenre()
    {
        var allTracksByGenre = new Dictionary<string, List<Track>>(StringComparer.OrdinalIgnoreCase);

        foreach (var track in AllTracks)
        {
            string genre = track.Genre;

            if (string.IsNullOrWhiteSpace(genre))
                continue;

            if (!allTracksByGenre.TryGetValue(genre, out var tracks))
                allTracksByGenre.Add(genre, tracks = []);

            tracks.Add(track);
        }

        return allTracksByGenre;
    }

    public override string ToString() => $"Count: {AllTracks.Count}";
}

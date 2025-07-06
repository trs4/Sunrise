namespace Sunrise.Model;

public sealed class SearchResults
{
    internal SearchResults(List<Track> tracks, List<(string Name, Dictionary<string, List<Track>> TracksByAlbums)> artists,
        List<(string Name, string Artist, List<Track> Tracks)> albums, List<(string Name, List<Track> Tracks)> genres)
    {
        Tracks = tracks;
        Artists = artists;
        Albums = albums;
        Genres = genres;
    }

    public List<Track> Tracks { get; }

    public List<(string Name, Dictionary<string, List<Track>> TracksByAlbums)> Artists { get; }

    public List<(string Name, string Artist, List<Track> Tracks)> Albums { get; }

    public List<(string Name, List<Track> Tracks)> Genres { get; }
}

namespace Sunrise.Model;

public sealed class TracksScreenshot
{
    internal TracksScreenshot(List<Track> allTracks, Dictionary<int, Track> allTracksById, Dictionary<string, Track> allTracksByPath)
    {
        AllTracks = allTracks ?? throw new ArgumentNullException(nameof(allTracks));
        AllTracksById = allTracksById ?? throw new ArgumentNullException(nameof(allTracksById));
        AllTracksByPath = allTracksByPath ?? throw new ArgumentNullException(nameof(allTracksByPath));
    }

    public List<Track> AllTracks { get; }

    public Dictionary<int, Track> AllTracksById { get; }

    public Dictionary<string, Track> AllTracksByPath { get; }

    public override string ToString() => $"Count: {AllTracks.Count}";
}

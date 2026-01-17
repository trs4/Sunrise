using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;
using Sunrise.Model.Common;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public sealed class ArtistViewModel : TrackSourceViewModel
{
    private readonly Dictionary<string, List<Track>> _tracksByAlbums;
    private readonly int _tracksCount;

    private ArtistViewModel(ArtistsRubricViewModel rubric, string name, Dictionary<string, List<Track>> tracksByAlbums, int tracksCount)
        : base(rubric, name, string.Format(Texts.ArtistDescriptionFormat, tracksByAlbums.Count, tracksCount))
    {
        _tracksByAlbums = tracksByAlbums;
        _tracksCount = tracksCount;
    }

    public static ArtistViewModel Create(ArtistsRubricViewModel rubric, string name, Dictionary<string, List<Track>> tracksByAlbums)
    {
        int tracksCount = tracksByAlbums.Sum(p => p.Value.Count);
        return new ArtistViewModel(rubric, name, tracksByAlbums, tracksCount);
    }

    public override RubricTypes Type => RubricTypes.Artist;

    public List<Track> GetTracks()
    {
        var tracks = new List<Track>(_tracksCount);

        foreach (var albumTracks in _tracksByAlbums.Values)
            tracks.AddRange(albumTracks);

        tracks.Sort((a, b) => NaturalSortComparer.Instance.Compare(a.Title, b.Title));
        return tracks;
    }

    protected override Track? GetTrackWithPicture()
        => _tracksByAlbums.Values.SelectMany(s => s).FirstOrDefault(t => t.HasPicture);

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null) => GetTracks();

    public override IReadOnlyList<Track>? GetCurrentTracks() => GetTracks();

    public override string ToString() => Name;
}

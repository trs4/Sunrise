using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public sealed class AlbumViewModel : TrackSourceViewModel
{
    public AlbumViewModel(AlbumsRubricViewModel rubric, string name, string artist, List<Track> tracks)
        : base(rubric, name, artist)
    {
        Artist = artist;
        Tracks = tracks;
    }

    public string Artist { get; }

    public List<Track> Tracks { get; }

    public override RubricTypes Type => RubricTypes.Album;

    protected override Track? GetTrackWithPicture() => Tracks.FirstOrDefault(t => t.HasPicture);

    public override string ToString() => $"{Artist} - {Name}";

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default)
        => new(Tracks);

    public override IReadOnlyList<Track>? GetCurrentTracks() => Tracks;
}

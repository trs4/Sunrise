using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public sealed class GenreViewModel : TrackSourceViewModel
{
    public GenreViewModel(GenresRubricViewModel rubric, string name, List<Track> tracks)
        : base(rubric, name, string.Format(Texts.SongsFormat, tracks.Count))
        => Tracks = tracks;

    public List<Track> Tracks { get; }

    public override RubricTypes Type => RubricTypes.Genre;

    protected override Track? GetTrackWithPicture() => Tracks.FirstOrDefault(t => t.HasPicture);

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default)
        => new(Tracks);

    public override IReadOnlyList<Track>? GetCurrentTracks() => Tracks;

    public override string ToString() => Name;
}

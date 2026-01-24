using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public sealed class SearchRubricViewModel : RubricViewModel
{
    private readonly IReadOnlyList<Track> _tracks;

    public SearchRubricViewModel(Player player, IReadOnlyList<Track> tracks)
        : base(player, null, Texts.Search)
        => _tracks = tracks;

    public override RubricTypes Type => RubricTypes.Search;

    public override bool IsDependent => true;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default)
        => new(_tracks);

    public override IReadOnlyList<Track>? GetCurrentTracks() => _tracks;
}

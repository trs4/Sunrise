using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

/// <summary>История воспроизведения</summary>
public sealed class HistoryRubricViewModel : RubricViewModel
{
    private readonly List<Track> _tracks = [];

    public HistoryRubricViewModel(Player player) : base(player, null, Texts.History) { }

    public override RubricTypes Type => RubricTypes.History;

    public override bool IsDependent => true;

    public void Add(Track track) => _tracks.Add(track);

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default)
        => new(_tracks);

    public override IReadOnlyList<Track>? GetCurrentTracks() => _tracks;
}

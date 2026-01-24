using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public sealed class RecentlyAddedRubricViewModel : RubricViewModel
{
    private const int _maxCount = 50;
    private TracksScreenshot? _screenshot;
    private List<Track>? _tracks;

    public RecentlyAddedRubricViewModel(Player player) : base(player, null, Texts.RecentlyAdded) { }

    public override RubricTypes Type => RubricTypes.RecentlyAdded;

    public override bool IsDependent => true;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override async ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default)
    {
        var screenshot = await Player.GetTracksAsync(token);

        if (!ReferenceEquals(_screenshot, screenshot))
        {
            _screenshot = screenshot;
            return _tracks = [.. screenshot.Tracks.OrderByDescending(t => t.Added).Take(_maxCount)];
        }

        return _tracks ?? [];
    }

    public override IReadOnlyList<Track>? GetCurrentTracks() => _tracks;
}

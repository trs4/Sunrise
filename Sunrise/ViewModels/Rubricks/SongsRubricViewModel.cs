using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Common;
using Sunrise.Model.Resources;
using Sunrise.Utils;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public sealed class SongsRubricViewModel : RubricViewModel
{
    private TracksScreenshot? _screenshot;
    private List<Track>? _tracks;

    public SongsRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Song)), Texts.Songs) { }

    public override RubricTypes Type => RubricTypes.Songs;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override async ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default)
    {
        var screenshot = await Player.GetTracksAsync(token);

        if (!ReferenceEquals(_screenshot, screenshot))
        {
            _screenshot = screenshot;
            return _tracks = [.. screenshot.Tracks.OrderBy(t => t.Artist, NaturalSortComparer.Instance).ThenBy(t => t.Title, NaturalSortComparer.Instance)];
        }

        return _tracks ?? [];
    }

    public override IReadOnlyList<Track>? GetCurrentTracks() => _tracks;
}

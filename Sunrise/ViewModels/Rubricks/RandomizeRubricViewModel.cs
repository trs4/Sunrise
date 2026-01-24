using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public sealed class RandomizeRubricViewModel : RubricViewModel
{
    private readonly IReadOnlyList<Track>? _inputTracks;
    private TracksScreenshot? _screenshot;
    private Track[]? _tracks = [];

    public RandomizeRubricViewModel(Player player, IReadOnlyList<Track>? tracks = null)
        : base(player, null, Texts.Randomize)
        => _inputTracks = tracks;

    public override RubricTypes Type => RubricTypes.Randomize;

    public override bool IsDependent => true;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override async ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default)
    {
        var screenshot = await Player.GetTracksAsync(token);

        if (!ReferenceEquals(_screenshot, screenshot))
        {
            _screenshot = screenshot;
            return _tracks = RandomHelper.CreateRandomizeTracks(_inputTracks ?? screenshot.Tracks);
        }

        return _tracks ?? [];
    }

    public override IReadOnlyList<Track>? GetCurrentTracks() => _tracks;
}

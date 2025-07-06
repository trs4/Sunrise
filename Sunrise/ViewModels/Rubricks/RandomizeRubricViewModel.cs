using System.Collections.Generic;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public sealed class RandomizeRubricViewModel : RubricViewModel
{
    private readonly IReadOnlyList<Track>? _inputTracks;
    private TracksScreenshot? _screenshot;
    private Track[]? _tracks = [];

    public RandomizeRubricViewModel(Player player, IReadOnlyList<Track>? tracks = null)
        : base(player, null, Texts.Randomize)
        => _inputTracks = tracks;

    public override bool IsDependent => true;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
    {
        if (!ReferenceEquals(_screenshot, screenshot))
        {
            _screenshot = screenshot;
            return _tracks = RandomHelper.CreateRandomizeTracks(_inputTracks ?? screenshot.AllTracks);
        }

        return _tracks ?? [];
    }

    public override IReadOnlyList<Track>? GetCurrentTracks() => _tracks;
}

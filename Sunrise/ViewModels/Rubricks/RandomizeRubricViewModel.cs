using System.Collections.Generic;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public sealed class RandomizeRubricViewModel : RubricViewModel
{
    private readonly Track[] _tracks;

    public RandomizeRubricViewModel(MainViewModel mainViewModel)
        : base(mainViewModel.TrackPlay.Player, null, Texts.Randomize)
        => _tracks = mainViewModel.CreateRandomizeTracks();

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => _tracks;
}

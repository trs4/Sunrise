using System.Collections.Generic;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public sealed class SearchRubricViewModel : RubricViewModel
{
    private readonly IReadOnlyList<Track> _tracks;

    public SearchRubricViewModel(Player player, IReadOnlyList<Track> tracks)
        : base(player, null, Texts.Search)
        => _tracks = tracks;

    public override bool IsDependent => true;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null) => _tracks;

    public override IReadOnlyList<Track>? GetCurrentTracks() => _tracks;
}

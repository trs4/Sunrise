using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public sealed class RecentlyAddedRubricViewModel : RubricViewModel
{
    private const int _maxCount = 50;

    public RecentlyAddedRubricViewModel(Player player) : base(player, null, Texts.RecentlyAdded) { }

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override IEnumerable<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => screenshot.AllTracks.OrderByDescending(t => t.Added).Take(_maxCount);
}

using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public sealed class RecentlyAddedRubricViewModel : RubricViewModel
{
    private const int _maxCount = 50;
    private TracksScreenshot? _screenshot;
    private List<Track> _tracks = [];

    public RecentlyAddedRubricViewModel(Player player) : base(player, null, Texts.RecentlyAdded) { }

    public override bool IsDependent => true;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
    {
        if (!ReferenceEquals(_screenshot, screenshot))
        {
            _screenshot = screenshot;
            return _tracks = [.. screenshot.AllTracks.OrderByDescending(t => t.Added).Take(_maxCount)];
        }

        return _tracks;
    }

}

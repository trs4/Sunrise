using System.Linq;
using Sunrise.Views;

namespace Sunrise.ViewModels;

public sealed class ListTrackPlayStrategy : TrackPlayStrategy
{
    public ListTrackPlayStrategy(MainViewModel owner) : base(owner) { }

    public override TrackViewModel? GetFirst() => Owner.Tracks?.FirstOrDefault();

    public override TrackViewModel? GetPrev(TrackViewModel? currentTrack) => DataGridRowsManager.GetPrev(Owner.Tracks, currentTrack);

    public override TrackViewModel? GetNext(TrackViewModel? currentTrack) => DataGridRowsManager.GetNext(Owner.Tracks, currentTrack);
}
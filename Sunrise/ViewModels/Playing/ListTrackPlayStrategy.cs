using Sunrise.Views;

namespace Sunrise.ViewModels;

public sealed class ListTrackPlayStrategy : TrackPlayStrategy
{
    public ListTrackPlayStrategy(MainViewModel owner) : base(owner) { }

    public override TrackViewModel? GetFirst() => DataGridRowsManager.GetFirstRow<TrackViewModel>(Owner.Owner);

    public override TrackViewModel? GetPrev(TrackViewModel? currentTrack) => DataGridRowsManager.GetPrevRow(Owner.Owner, currentTrack);

    public override TrackViewModel? GetNext(TrackViewModel? currentTrack) => DataGridRowsManager.GetNextRow(Owner.Owner, currentTrack);
}
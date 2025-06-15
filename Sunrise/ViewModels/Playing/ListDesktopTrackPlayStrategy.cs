using Sunrise.Views;

namespace Sunrise.ViewModels;

public sealed class ListDesktopTrackPlayStrategy : TrackPlayStrategy
{
    public ListDesktopTrackPlayStrategy(MainDesktopViewModel owner) : base(owner) { }

    public new MainDesktopViewModel Owner => (MainDesktopViewModel)base.Owner;

    public override TrackViewModel? GetFirst() => DataGridRowsManager.GetFirstRow<TrackViewModel>(Owner.Owner);

    public override TrackViewModel? GetPrev(TrackViewModel? currentTrack) => DataGridRowsManager.GetPrevRow(Owner.Owner, currentTrack);

    public override TrackViewModel? GetNext(TrackViewModel? currentTrack) => DataGridRowsManager.GetNextRow(Owner.Owner, currentTrack);
}
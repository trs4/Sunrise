using System.Linq;
using Sunrise.Views;

namespace Sunrise.ViewModels;

public sealed class ListTrackPlayStrategy : TrackPlayStrategy
{
    public ListTrackPlayStrategy(MainViewModel owner) : base(owner) { }

    public override TrackViewModel? GetFirst()
    {
        var window = Owner.Owner;

        if (window is not null)
            return DataGridRowsManager.GetFirstRow<TrackViewModel>(window);

        return Owner.Tracks?.FirstOrDefault();
    }

    public override TrackViewModel? GetPrev(TrackViewModel? currentTrack)
    {
        var window = Owner.Owner;

        if (window is not null)
            return DataGridRowsManager.GetPrevRow(window, currentTrack);

        return DataGridRowsManager.GetPrev(Owner.Tracks, currentTrack);
    }

    public override TrackViewModel? GetNext(TrackViewModel? currentTrack)
    {
        var window = Owner.Owner;

        if (window is not null)
            return DataGridRowsManager.GetNextRow(window, currentTrack);

        return DataGridRowsManager.GetNext(Owner.Tracks, currentTrack);
    }

}
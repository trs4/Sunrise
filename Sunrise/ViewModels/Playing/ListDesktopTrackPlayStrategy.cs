using System.Threading.Tasks;
using Sunrise.Views;

namespace Sunrise.ViewModels;

public sealed class ListDesktopTrackPlayStrategy : TrackPlayStrategy
{
    public ListDesktopTrackPlayStrategy(MainDesktopViewModel owner) : base(owner) { }

    public new MainDesktopViewModel Owner => (MainDesktopViewModel)base.Owner;

    public override ValueTask<TrackViewModel?> GetFirstAsync()
        => new(DataGridRowsManager.GetFirstRow<TrackViewModel>(Owner.Owner));

    public override ValueTask<TrackViewModel?> GetPrevAsync(TrackViewModel? currentTrack)
        => new(DataGridRowsManager.GetPrevRow(Owner.Owner, currentTrack));

    public override ValueTask<TrackViewModel?> GetNextAsync(TrackViewModel? currentTrack)
        => new(DataGridRowsManager.GetNextRow(Owner.Owner, currentTrack));

    public override bool Equals(bool randomPlay, RubricViewModel? ownerRubric, TrackSourceViewModel? ownerTrackSource) => !randomPlay;
}
using System;
using System.Threading.Tasks;

namespace Sunrise.ViewModels;

public abstract class TrackPlayStrategy
{
    protected TrackPlayStrategy(MainViewModel owner)
        => Owner = owner ?? throw new ArgumentNullException(nameof(owner));

    public static TrackPlayStrategy Create(MainViewModel owner, bool randomPlay = false,
        RubricViewModel? ownerRubric = null, TrackSourceViewModel? ownerTrackSource = null)
    {
        if (randomPlay)
            return new RandomTrackPlayStrategy(owner, ownerRubric, ownerTrackSource);

        return owner is MainDesktopViewModel desktopOwner
            ? new ListDesktopTrackPlayStrategy(desktopOwner) : new ListTrackPlayStrategy(owner, ownerRubric, ownerTrackSource);
    }

    public MainViewModel Owner { get; }

    public abstract ValueTask<TrackViewModel?> GetFirstAsync();

    public abstract ValueTask<TrackViewModel?> GetPrevAsync(TrackViewModel? currentTrack);

    public abstract ValueTask<TrackViewModel?> GetNextAsync(TrackViewModel? currentTrack);

    public abstract bool Equals(bool randomPlay, RubricViewModel? ownerRubric, TrackSourceViewModel? ownerTrackSource);
}

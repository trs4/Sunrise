using System;

namespace Sunrise.ViewModels;

public abstract class TrackPlayStrategy
{
    protected TrackPlayStrategy(MainViewModel owner)
        => Owner = owner ?? throw new ArgumentNullException(nameof(owner));

    public static TrackPlayStrategy Create(MainViewModel owner, bool randomPlay = false)
    {
        if (randomPlay)
            return new RandomTrackPlayStrategy(owner);

        return owner is MainDesktopViewModel desktopOwner
            ? new ListDesktopTrackPlayStrategy(desktopOwner) : new ListTrackPlayStrategy(owner);
    }

    public MainViewModel Owner { get; }

    public abstract TrackViewModel? GetFirst();

    public abstract TrackViewModel? GetPrev(TrackViewModel? currentTrack);

    public abstract TrackViewModel? GetNext(TrackViewModel? currentTrack);
}

using System;
using System.Linq;

namespace Sunrise.ViewModels;

public sealed class RandomTrackPlayStrategy : TrackPlayStrategy
{
    private TrackViewModel[]? _tracks;

    public RandomTrackPlayStrategy(MainViewModel owner) : base(owner) { }

    private TrackViewModel[] Tracks => _tracks ??= Owner.CreateRandomizeTrackViewModels();

    public override TrackViewModel? GetFirst() => Tracks.FirstOrDefault();

    public override TrackViewModel? GetPrev(TrackViewModel? currentTrack)
    {
        if (currentTrack is null)
            return null;

        var tracks = Tracks;
        int index = Array.IndexOf(tracks, currentTrack) - 1;
        return index >= 0 && index < tracks.Length ? tracks[index] : null;
    }

    public override TrackViewModel? GetNext(TrackViewModel? currentTrack)
    {
        if (currentTrack is null)
            return null;

        var tracks = Tracks;
        int index = Array.IndexOf(tracks, currentTrack) + 1;
        return index >= 0 && index < tracks.Length ? tracks[index] : null;
    }

}

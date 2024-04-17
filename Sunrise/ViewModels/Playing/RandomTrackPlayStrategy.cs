using System;
using System.Linq;
using System.Security.Cryptography;

namespace Sunrise.ViewModels;

public sealed class RandomTrackPlayStrategy : TrackPlayStrategy
{
    private TrackViewModel[]? _tracks;

    public RandomTrackPlayStrategy(MainViewModel owner) : base(owner) { }

    private TrackViewModel[] Tracks => _tracks ??= CreateRandomizeTracks();

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

    private TrackViewModel[] CreateRandomizeTracks()
    {
        var tracks = Owner.Tracks;
        int count = tracks.Count;
        var randomizeTracks = new TrackViewModel[count];

        for (int i = 0; i < count; i++)
            randomizeTracks[i] = tracks[i];

        RandomNumberGenerator.Shuffle(randomizeTracks.AsSpan());
        return randomizeTracks;
    }

}

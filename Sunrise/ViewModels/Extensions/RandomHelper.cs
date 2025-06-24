using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Sunrise.Model;

namespace Sunrise.ViewModels;

internal static class RandomHelper
{
    public static Track[] CreateRandomizeTracks(IReadOnlyList<Track> tracks)
    {
        int count = tracks.Count;
        var randomizeTracks = new Track[count];

        for (int i = 0; i < count; i++)
            randomizeTracks[i] = tracks[i];

        RandomNumberGenerator.Shuffle(randomizeTracks.AsSpan());
        return randomizeTracks;
    }

}

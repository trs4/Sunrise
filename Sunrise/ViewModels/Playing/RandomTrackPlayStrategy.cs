using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public sealed class RandomTrackPlayStrategy : TrackPlayStrategy
{
    private readonly RubricViewModel? _ownerRubric;
    private readonly TrackSourceViewModel? _ownerTrackSource;
    private IReadOnlyList<Track>? _tracks;

    public RandomTrackPlayStrategy(MainViewModel owner,
        RubricViewModel? ownerRubric = null, TrackSourceViewModel? ownerTrackSource = null)
        : base(owner)
    {
        _ownerRubric = ownerRubric;
        _ownerTrackSource = ownerTrackSource;
    }

    public override async ValueTask<TrackViewModel?> GetFirstAsync()
    {
        var tracks = await GetTracksAsync();
        var track = tracks.Count > 0 ? tracks[0] : null;
        return Owner.GetTrackViewModelWithCheck(track);
    }

    public override async ValueTask<TrackViewModel?> GetPrevAsync(TrackViewModel? currentTrack)
    {
        if (currentTrack is null)
            return null;

        var tracks = await GetTracksAsync();
        int index = GetCurrentIndex(tracks, currentTrack) - 1;
        return index >= 0 && index < tracks.Count ? Owner.GetTrackViewModelWithCheck(tracks[index]) : null;
    }

    public override async ValueTask<TrackViewModel?> GetNextAsync(TrackViewModel? currentTrack)
    {
        if (currentTrack is null)
            return null;

        var tracks = await GetTracksAsync();
        int index = GetCurrentIndex(tracks, currentTrack) + 1;
        return index >= 0 && index < tracks.Count ? Owner.GetTrackViewModelWithCheck(tracks[index]) : null;
    }

    private async ValueTask<IReadOnlyList<Track>> GetTracksAsync() => _tracks ??= await InitTracksAsync();

    private async ValueTask<IReadOnlyList<Track>> InitTracksAsync()
    {
        if (_ownerRubric is not null)
        {
            var tracks = await _ownerRubric.GetTracksAsync(_ownerTrackSource);
            return RandomHelper.CreateRandomizeTracks(tracks);
        }

        return Owner.CreateRandomizeTracks();
    }

    private static int GetCurrentIndex(IReadOnlyList<Track> tracks, TrackViewModel? currentTrack)
    {
        if (tracks is Track[] array)
            return Array.IndexOf(array, currentTrack?.Track) + 1;

        if (tracks is not List<Track> list)
            list = [.. tracks];

        return list.IndexOf(currentTrack?.Track);
    }

    public override bool Equals(bool randomPlay, RubricViewModel? ownerRubric, TrackSourceViewModel? ownerTrackSource)
    {
        if (!randomPlay)
            return false;

        return _ownerRubric == ownerRubric && _ownerTrackSource == ownerTrackSource;
    }

}

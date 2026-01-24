using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Views;

namespace Sunrise.ViewModels;

public sealed class ListTrackPlayStrategy : TrackPlayStrategy
{
    private readonly RubricViewModel? _ownerRubric;
    private readonly TrackSourceViewModel? _ownerTrackSource;
    private IReadOnlyList<Track>? _tracks;

    public ListTrackPlayStrategy(MainViewModel owner,
        RubricViewModel? ownerRubric = null, TrackSourceViewModel? ownerTrackSource = null)
        : base(owner)
    {
        _ownerRubric = ownerRubric;
        _ownerTrackSource = ownerTrackSource;
    }

    public override async ValueTask<TrackViewModel?> GetFirstAsync()
    {
        if (_ownerRubric is not null)
        {
            var tracks = await GetTracksAsync();
            var track = tracks.Count > 0 ? tracks[0] : null;
            return Owner.GetTrackViewModelWithCheck(track);
        }

        return Owner.Tracks?.FirstOrDefault();
    }

    public override async ValueTask<TrackViewModel?> GetPrevAsync(TrackViewModel? currentTrack)
    {
        if (_ownerRubric is not null)
        {
            var tracks = await GetTracksAsync();
            var track = DataGridRowsManager.GetPrev((IList)tracks, currentTrack?.Track);
            return Owner.GetTrackViewModelWithCheck(track);
        }

        return DataGridRowsManager.GetPrev(Owner.Tracks, currentTrack);
    }

    public override async ValueTask<TrackViewModel?> GetNextAsync(TrackViewModel? currentTrack)
    {
        if (_ownerRubric is not null)
        {
            var tracks = await GetTracksAsync();
            var track = DataGridRowsManager.GetNext((IList)tracks, currentTrack?.Track);
            return Owner.GetTrackViewModelWithCheck(track);
        }

        return DataGridRowsManager.GetNext(Owner.Tracks, currentTrack);
    }

    private async ValueTask<IReadOnlyList<Track>> GetTracksAsync() => _tracks ??= await InitTracksAsync();

    private async ValueTask<IReadOnlyList<Track>> InitTracksAsync()
    {
        var ownerRubric = _ownerRubric;
        return ownerRubric is null ? [] : await ownerRubric.GetTracksAsync(_ownerTrackSource);
    }

    public override bool Equals(bool randomPlay, RubricViewModel? ownerRubric, TrackSourceViewModel? ownerTrackSource)
    {
        if (randomPlay)
            return false;

        return _ownerRubric == ownerRubric && _ownerTrackSource == ownerTrackSource;
    }

}
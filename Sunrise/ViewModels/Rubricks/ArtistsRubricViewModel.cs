using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Common;
using Sunrise.Model.Resources;
using Sunrise.Utils;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public sealed class ArtistsRubricViewModel : RubricViewModel
{
    private TracksScreenshot? _screenshot;
    private List<ArtistViewModel>? _trackSources;
    private IReadOnlyList<Track>? _currentTracks;

    public ArtistsRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Artist)), Texts.Artists) { }

    public override RubricTypes Type => RubricTypes.Artists;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot)
    {
        if (_trackSources is not null && ReferenceEquals(screenshot, _screenshot))
            return _trackSources;

        var trackSources = new List<ArtistViewModel>(screenshot.TracksByArtist.Count);

        foreach (var pair in screenshot.TracksByArtist)
            trackSources.Add(ArtistViewModel.Create(this, pair.Key, pair.Value));

        trackSources.Sort((a, b) => NaturalSortComparer.Instance.Compare(a.Name, b.Name));
        _trackSources = trackSources;
        _screenshot = screenshot;
        return trackSources;
    }

    public override ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default)
        => new(_currentTracks = (trackSource as ArtistViewModel)?.GetTracks() ?? []);

    public override IReadOnlyList<Track>? GetCurrentTracks() => _currentTracks;
}

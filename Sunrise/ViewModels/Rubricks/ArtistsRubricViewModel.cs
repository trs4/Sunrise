using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;
using Sunrise.ViewModels.Artists;

namespace Sunrise.ViewModels;

public sealed class ArtistsRubricViewModel : RubricViewModel
{
    private TracksScreenshot? _screenshot;
    private List<ArtistViewModel>? _trackSources;

    public ArtistsRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Artist)), Texts.Artists) { }

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot)
    {
        if (_trackSources is not null && ReferenceEquals(screenshot, _screenshot))
            return _trackSources;

        var trackSources = new List<ArtistViewModel>(screenshot.AllTracksByArtist.Count);

        foreach (var pair in screenshot.AllTracksByArtist)
        {
            int tracksCount = pair.Value.Sum(p => p.Value.Count);
            trackSources.Add(new ArtistViewModel(this, pair.Key, pair.Value, tracksCount));
        }

        trackSources.Sort((a, b) => string.Compare(a.Name, b.Name, true));
        _trackSources = trackSources;
        _screenshot = screenshot;
        return trackSources;
    }

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => (trackSource as ArtistViewModel)?.GetTracks() ?? [];
}

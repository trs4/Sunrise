using System.Collections.Generic;
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
            trackSources.Add(new ArtistViewModel(pair.Key, pair.Value, this));

        trackSources.Sort((a, b) => string.Compare(a.Name, b.Name, true));
        _trackSources = trackSources;
        _screenshot = screenshot;
        return trackSources;
    }

    public override List<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => (trackSource as ArtistViewModel)?.GetTracks() ?? [];
}

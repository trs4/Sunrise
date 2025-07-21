using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public sealed class GenresRubricViewModel : RubricViewModel
{
    private TracksScreenshot? _screenshot;
    private List<GenreViewModel>? _trackSources;
    private IReadOnlyList<Track>? _currentTracks;

    public GenresRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Genre)), Texts.Genres) { }

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot)
    {
        if (_trackSources is not null && ReferenceEquals(screenshot, _screenshot))
            return _trackSources;

        var trackSources = new List<GenreViewModel>(screenshot.TracksByGenre.Count);

        foreach (var pair in screenshot.TracksByGenre)
        {
            var tracks = pair.Value.OrderBy(t => t.Title).ToList();
            trackSources.Add(new GenreViewModel(this, pair.Key, tracks));
        }

        trackSources.Sort((a, b) => string.Compare(a.Name, b.Name, true));
        _trackSources = trackSources;
        _screenshot = screenshot;
        return trackSources;
    }

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => _currentTracks = (trackSource as GenreViewModel)?.Tracks ?? [];

    public override IReadOnlyList<Track>? GetCurrentTracks() => _currentTracks;
}

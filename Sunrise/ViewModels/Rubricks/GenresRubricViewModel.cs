using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;
using Sunrise.ViewModels.Genres;

namespace Sunrise.ViewModels;

public sealed class GenresRubricViewModel : RubricViewModel
{
    private TracksScreenshot? _screenshot;
    private List<GenreViewModel>? _trackSources;

    public GenresRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Genre)), Texts.Genres) { }

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot)
    {
        if (_trackSources is not null && ReferenceEquals(screenshot, _screenshot))
            return _trackSources;

        var trackSources = new List<GenreViewModel>(screenshot.AllTracksByGenre.Count);

        foreach (var pair in screenshot.AllTracksByGenre)
            trackSources.Add(new GenreViewModel(pair.Key, pair.Value, this));

        trackSources.Sort((a, b) => string.Compare(a.Name, b.Name, true));
        _trackSources = trackSources;
        _screenshot = screenshot;
        return trackSources;
    }

    public override IEnumerable<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => ((trackSource as GenreViewModel)?.Tracks ?? []).OrderBy(t => t.Title);
}

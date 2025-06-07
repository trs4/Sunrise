using System.Collections.Generic;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public sealed class SongsRubricViewModel : RubricViewModel
{
    public SongsRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Song)), Texts.Songs) { }

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override List<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null) => screenshot.AllTracks;
}

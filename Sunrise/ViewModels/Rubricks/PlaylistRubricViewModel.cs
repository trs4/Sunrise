using System.Collections.Generic;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public sealed class PlaylistRubricViewModel : RubricViewModel
{
    private readonly Playlist _playlist;

    public PlaylistRubricViewModel(Player player, Playlist playlist)
        : base(player, null, playlist.Name)
        => _playlist = playlist;

    public override bool IsDependent => true;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => _playlist.Tracks;
}

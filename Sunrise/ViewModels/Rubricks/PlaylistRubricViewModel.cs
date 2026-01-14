using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public class PlaylistRubricViewModel : RubricViewModel
{
    private bool? _isPlaying;
    private object? _icon;
    private bool _iconLoaded;
    private bool _editing;
    private List<Track>? _currentTracks;

    public PlaylistRubricViewModel(Player player, Playlist playlist)
        : base(player, null, playlist.Name)
        => Playlist = playlist;

    public Playlist Playlist { get; }

    /// <summary>Воспроизводится</summary>
    public bool? IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    /// <summary>Иконка</summary>
    public override object? Icon
    {
        get
        {
            if (!_iconLoaded)
            {
                var track = Playlist.Tracks.OrderByDescending(t => t.Reproduced).FirstOrDefault(t => t.HasPicture);

                if (track is not null)
                    TrackIconHelper.SetPicture(Player, track, icon => Icon = icon);

                _iconLoaded = true;
            }

            return _icon;
        }
        set => SetProperty(ref _icon, value);
    }

    public bool Editing
    {
        get => _editing;
        set => SetProperty(ref _editing, value);
    }

    public override bool IsDependent => true;

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot) => null;

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => _currentTracks = Playlist.Tracks;

    public override IReadOnlyList<Track>? GetCurrentTracks() => _currentTracks;
}

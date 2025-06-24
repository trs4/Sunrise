using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public class PlaylistViewModel : ObservableObject
{
    private object? _icon;
    private bool _iconLoaded;
    private string _name;
    private bool _editing;

    public PlaylistViewModel(Playlist playlist, Player player)
    {
        Playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        _name = playlist.Name;
    }

    public Playlist Playlist { get; }

    public Player Player { get; }

    /// <summary>Иконка</summary>
    public object? Icon
    {
        get
        {
            if (!_iconLoaded)
            {
                var track = Playlist.Tracks.FirstOrDefault(t => t.HasPicture);

                if (track is not null)
                    TrackIconHelper.SetPicture(Player, track, icon => Icon = icon);

                _iconLoaded = true;
            }

            return _icon;
        }
        set => SetProperty(ref _icon, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool Editing
    {
        get => _editing;
        set => SetProperty(ref _editing, value);
    }

    public override string ToString() => _name;
}

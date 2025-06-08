using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels.Playlists;

public class PlaylistViewModel : ObservableObject
{
    private string _name;
    private bool _editing;

    public PlaylistViewModel(Playlist playlist)
    {
        Playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));
        _name = playlist.Name;
    }

    public Playlist Playlist { get; }

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

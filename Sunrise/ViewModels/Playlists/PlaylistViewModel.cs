using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels.Playlists;

public class PlaylistViewModel : ObservableObject
{
    public PlaylistViewModel(Playlist playlist)
        => Playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));

    public Playlist Playlist { get; }

    public override string ToString() => Playlist.Name;
}

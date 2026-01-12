using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.Model;
using Sunrise.Utils;
using Sunrise.ViewModels;
using Sunrise.ViewModels.Interfaces;

namespace Sunrise.Views;

public partial class PlaylistsTabView : UserControl, IPlaylistsView
{
    public PlaylistsTabView()
        => InitializeComponent();

    private async void RecentlyAddedPlaylist_Tapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = e.GetDataContextWithCheck<PlaylistViewModel>();

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.SelectedPlaylist = playlistViewModel;
        mainViewModel.IsPlaylistsVisible = false;

        var rubricViewModel = mainViewModel.GetPlaylistViewModel(playlistViewModel.Playlist);
        mainViewModel.TrackPlay.ChangeOwnerRubric(rubricViewModel);
        await mainViewModel.ChangeTracksAsync(rubricViewModel);
    }

    private async void Track_Tapped(object? sender, TappedEventArgs e)
        => await PlayTrack(e);

    private async Task PlayTrack(TappedEventArgs e)
    {
        var trackViewModel = e.GetDataContextWithCheck<TrackViewModel>();

        if (trackViewModel is null || DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.IsShortTrackVisible = true;
        var trackPlay = mainViewModel.TrackPlay;
        var playlist = mainViewModel.SelectedPlaylist?.Playlist;

        if (playlist is not null)
        {
            var rubricViewModel = mainViewModel.GetPlaylistViewModel(playlist);
            mainViewModel.TrackPlay.ChangeOwnerRubric(rubricViewModel);
            trackPlay.ChangeOwnerRubric(rubricViewModel);
        }

        if (trackViewModel.IsPlaying == true)
            return;

        await trackPlay.PlayAsync(trackViewModel);
    }

    private async void Playlist_Tapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = e.GetDataContextWithCheck<PlaylistViewModel>();

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.IsPlaylistsVisible = false;

        var rubricViewModel = mainViewModel.GetPlaylistViewModel(playlistViewModel.Playlist);
        mainViewModel.TrackPlay.ChangeOwnerRubric(rubricViewModel);
        await mainViewModel.ChangeTracksAsync(rubricViewModel);

        var selectedTrack = mainViewModel.Tracks.FirstOrDefault(t => t.IsPlaying ?? false);

        if (selectedTrack is not null)
            mainViewModel.PlaylistsView?.ScrollIntoView(selectedTrack.Track);
    }

    private void TrackIcon_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.TrackPlay.PlayCommand.Execute(null);
        e.Handled = true;
    }

    #region IPlaylistsView

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainViewModel mainViewModel)
            mainViewModel.PlaylistsView = this;
    }

    void IPlaylistsView.ScrollIntoView(Track track)
    {
        if (DataContext is not MainViewModel mainViewModel)
            return;

        int id = track.Id;
        var tracks = mainViewModel.Tracks;
        int index = -1;
        TrackViewModel selectedTrackViewModel = null;

        for (int i = 0; i < tracks.Count; i++)
        {
            var trackViewModel = tracks[i];

            if (trackViewModel.Track.Id == id)
            {
                index = i;
                selectedTrackViewModel = trackViewModel;
                break;
            }
        }

        if (index == -1)
            return;

        UIDispatcher.RunBackground(() =>
        {
            tracksBox.ScrollIntoView(index);
            mainViewModel.SelectedTrack = selectedTrackViewModel;
        });
    }

    void IPlaylistsView.ScrollIntoView(Playlist playlist)
    {
        if (DataContext is not MainViewModel mainViewModel)
            return;

        int id = playlist.Id;
        var playlists = mainViewModel.Playlists;
        int index = -1;
        PlaylistViewModel? selectedPlaylistViewModel = null;

        for (int i = 0; i < playlists.Count; i++)
        {
            var playlistViewModel = playlists[i];

            if (playlistViewModel.Playlist.Id == id)
            {
                index = i;
                selectedPlaylistViewModel = playlistViewModel;
                break;
            }
        }

        if (index == -1)
            return;

        UIDispatcher.RunBackground(() =>
        {
            playlistsBox.ScrollIntoView(index);
            mainViewModel.SelectedPlaylist = selectedPlaylistViewModel;
        });
    }

    #endregion
}
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class PlaylistsTabView : UserControl
{
    public PlaylistsTabView()
        => InitializeComponent();

    private async void RecentlyAddedPlaylist_Tapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = e.GetDataContext<PlaylistViewModel>();

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
        var trackViewModel = e.GetDataContext<TrackViewModel>();

        if (trackViewModel is null || DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.IsShortTrackVisible = true;
        var trackPlay = mainViewModel.TrackPlay;
        var playlist = mainViewModel.SelectedPlaylist?.Playlist;

        if (playlist is not null)
        {
            var rubricViewModel = mainViewModel.GetPlaylistViewModel(playlist);
            trackPlay.ChangeOwnerRubric(rubricViewModel);
        }

        if (trackViewModel.IsPlaying == true)
            return;

        await trackPlay.PlayAsync(trackViewModel);
    }

    private async void Playlist_Tapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = e.GetDataContext<PlaylistViewModel>();

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.IsPlaylistsVisible = false;

        var rubricViewModel = mainViewModel.GetPlaylistViewModel(playlistViewModel.Playlist);
        mainViewModel.TrackPlay.ChangeOwnerRubric(rubricViewModel);
        await mainViewModel.ChangeTracksAsync(rubricViewModel);
    }

    private void TrackIcon_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.TrackPlay.PlayCommand.Execute(null);
        e.Handled = true;
    }

}
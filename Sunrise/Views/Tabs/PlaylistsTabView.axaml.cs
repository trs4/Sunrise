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
        await mainViewModel.ChangeTracksAsync(playlistViewModel.Playlist);
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
        var rubricViewModel = new PlaylistRubricViewModel(trackPlay.Player, mainViewModel.SelectedPlaylist?.Playlist);
        trackPlay.ChangeOwnerRubric(rubricViewModel);

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
        await mainViewModel.ChangeTracksAsync(playlistViewModel.Playlist);
    }


    //private async Task PlayTrack(TappedEventArgs e, Func<MainDeviceViewModel, RubricViewModel> getRubric)
    //{
    //    var trackViewModel = e.GetDataContext<TrackViewModel>();

    //    if (trackViewModel is null || DataContext is not MainDeviceViewModel mainViewModel)
    //        return;

    //    mainViewModel.IsShortTrackVisible = true;
    //    var trackPlay = mainViewModel.TrackPlay;
    //    trackPlay.ChangeOwnerRubric(getRubric(mainViewModel));

    //    if (trackViewModel.IsPlaying == true)
    //        return;

    //    await trackPlay.PlayAsync(trackViewModel);
    //}

    //private void TrackIcon_Tapped(object? sender, TappedEventArgs e)
    //{
    //    if (DataContext is not MainDeviceViewModel mainViewModel)
    //        return;

    //    mainViewModel.TrackPlay.PlayCommand.Execute(null);
    //    e.Handled = true;
    //}

}
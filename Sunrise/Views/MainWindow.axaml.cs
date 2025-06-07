using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Sunrise.ViewModels;
using Sunrise.ViewModels.Playlists;

namespace Sunrise.Views;

public partial class MainWindow : Window
{
    public MainWindow()
        => InitializeComponent();

    private static T? GetDataContext<T>(RoutedEventArgs e)
        where T : class
        => (e.Source as StyledElement)?.DataContext as T ?? (e.Source as ContentPresenter)?.Content as T;

    private static T? GetSelectedItem<T>(SelectionChangedEventArgs e)
        where T : class
        => e.AddedItems?.Count > 0 ? e.AddedItems[0] as T : null;

    private async void Rubricks_Tapped(object? sender, TappedEventArgs e)
    {
        var rubricViewModel = GetDataContext<RubricViewModel>(e);

        if (rubricViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        await mainViewModel.ChangeTracksAsync(rubricViewModel);
    }

    private void Playlist_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var playlistViewModel = GetSelectedItem<PlaylistViewModel>(e);

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.SelectedPlaylist = playlistViewModel;
    }

    private async void Playlist_Tapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = GetDataContext<PlaylistViewModel>(e);

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        await mainViewModel.ChangeTracksAsync(playlistViewModel.Playlist);
    }

    private async void TrackSource_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var trackSourceViewModel = GetSelectedItem<TrackSourceViewModel>(e);
        await TrySetTrackSourceAsync(trackSourceViewModel);
    }

    private async void TrackSource_Tapped(object? sender, TappedEventArgs e)
    {
        var trackSourceViewModel = GetDataContext<TrackSourceViewModel>(e);
        await TrySetTrackSourceAsync(trackSourceViewModel);
    }

    private async void TrackSource_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var trackSourceViewModel = GetDataContext<TrackSourceViewModel>(e);
        var mainViewModel = await TrySetTrackSourceAsync(trackSourceViewModel);

        if (mainViewModel is null)
            return;

        var trackViewModel = mainViewModel.Tracks.FirstOrDefault();

        if (trackViewModel is not null && trackViewModel.IsPlaying != true)
            mainViewModel.TrackPlay.Play(trackViewModel);
    }

    private async Task<MainViewModel?> TrySetTrackSourceAsync(TrackSourceViewModel? trackSourceViewModel)
    {
        if (trackSourceViewModel is null || DataContext is not MainViewModel mainViewModel)
            return null;

        mainViewModel.SelectedTrackSource = trackSourceViewModel;
        await mainViewModel.ChangeTracksAsync(trackSourceViewModel);
        return mainViewModel;
    }

}

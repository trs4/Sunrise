using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Sunrise.ViewModels;
using Sunrise.ViewModels.Categories;
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

    private void Category_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var categoryViewModel = GetSelectedItem<CategoryViewModel>(e);

        if (categoryViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.SelectedCategory = categoryViewModel;
    }

    private void Category_Tapped(object? sender, TappedEventArgs e)
    {
        var categoryViewModel = GetDataContext<CategoryViewModel>(e);

        if (categoryViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        //await mainViewModel.ChangeTracksAsync(playlistViewModel.Playlist);
    }

    private void Category_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var categoryViewModel = GetDataContext<CategoryViewModel>(e);

        if (categoryViewModel is null)
            return;

        categoryViewModel.Editing = true;
    }

    private async void Category_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Tab)
            await OnCategoryNameChanged(e);
    }

    private async void Category_LostFocus(object? sender, RoutedEventArgs e)
        => await OnCategoryNameChanged(e);

    private async Task OnCategoryNameChanged(RoutedEventArgs e)
    {
        var categoryViewModel = GetDataContext<CategoryViewModel>(e);

        if (categoryViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        if (!await mainViewModel.TrackPlay.Player.ChangeCategoryNameAsync(categoryViewModel.Category, categoryViewModel.Name))
            categoryViewModel.Name = categoryViewModel.Category.Name;

        categoryViewModel.Editing = false;
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

    private void Playlist_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = GetDataContext<PlaylistViewModel>(e);

        if (playlistViewModel is null)
            return;

        playlistViewModel.Editing = true;
    }

    private async void Playlist_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Tab)
            await OnPlaylistNameChanged(e);
    }

    private async void Playlist_LostFocus(object? sender, RoutedEventArgs e)
        => await OnPlaylistNameChanged(e);

    private async Task OnPlaylistNameChanged(RoutedEventArgs e)
    {
        var playlistViewModel = GetDataContext<PlaylistViewModel>(e);

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        if (!await mainViewModel.TrackPlay.Player.ChangePlaylistNameAsync(playlistViewModel.Playlist, playlistViewModel.Name))
            playlistViewModel.Name = playlistViewModel.Playlist.Name;

        playlistViewModel.Editing = false;
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
            await mainViewModel.TrackPlay.PlayAsync(trackViewModel);
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

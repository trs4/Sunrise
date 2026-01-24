using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class MainWindow : Window
{
    public MainWindow()
        => InitializeComponent();

    private async void Rubricks_Tapped(object? sender, TappedEventArgs e)
    {
        var rubricViewModel = e.GetDataContextWithCheck<RubricViewModel>();

        if (rubricViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        await mainViewModel.ChangeTracksAsync(rubricViewModel);
    }

    #region Playlists

    private void Playlists_Tapped(object? sender, TappedEventArgs e)
    {
        if (!e.CanClick() || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.IsPlaylistsVisible = !mainViewModel.IsPlaylistsVisible;
    }

    private async void Playlist_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var playlistViewModel = e.GetSelectedItem<PlaylistRubricViewModel>();

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.SelectedPlaylist = playlistViewModel;
        await mainViewModel.ChangeTracksAsync(playlistViewModel);
    }

    private async void Playlist_Tapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = e.GetDataContextWithCheck<PlaylistRubricViewModel>();

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        await mainViewModel.ChangeTracksAsync(playlistViewModel);
    }

    private void Playlist_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = e.GetDataContext<PlaylistRubricViewModel>();

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
        var playlistViewModel = e.GetDataContext<PlaylistRubricViewModel>();

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        if (!await mainViewModel.TrackPlay.Player.ChangePlaylistNameAsync(playlistViewModel.Playlist, playlistViewModel.Name))
            playlistViewModel.Name = playlistViewModel.Playlist.Name;

        playlistViewModel.Editing = false;
    }

    #endregion
    #region Categories

    private void Categories_Tapped(object? sender, TappedEventArgs e)
    {
        if (!e.CanClick() || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.IsCategoriesVisible = !mainViewModel.IsCategoriesVisible;
    }

    private void Category_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var categoryViewModel = e.GetSelectedItem<CategoryViewModel>();

        if (categoryViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.SelectedCategory = categoryViewModel;
    }

    private void Category_Tapped(object? sender, TappedEventArgs e)
    {
        var categoryViewModel = e.GetDataContextWithCheck<CategoryViewModel>();

        if (categoryViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        //await mainViewModel.ChangeTracksAsync(playlistViewModel.Playlist);
    }

    private void Category_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var categoryViewModel = e.GetDataContext<CategoryViewModel>();

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
        var categoryViewModel = e.GetDataContext<CategoryViewModel>();

        if (categoryViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        if (!await mainViewModel.TrackPlay.Player.ChangeCategoryNameAsync(categoryViewModel.Category, categoryViewModel.Name))
            categoryViewModel.Name = categoryViewModel.Category.Name;

        categoryViewModel.Editing = false;
    }

    #endregion

    private async void TrackSource_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var trackSourceViewModel = e.GetSelectedItem<TrackSourceViewModel>();
        await TrySetTrackSourceAsync(trackSourceViewModel);
    }

    private async void TrackSource_Tapped(object? sender, TappedEventArgs e)
    {
        var trackSourceViewModel = e.GetDataContextWithCheck<TrackSourceViewModel>();
        await TrySetTrackSourceAsync(trackSourceViewModel);
    }

    private async void TrackSource_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var trackSourceViewModel = e.GetDataContext<TrackSourceViewModel>();
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

        await mainViewModel.ChangeTracksAsync(trackSourceViewModel);
        return mainViewModel;
    }

}

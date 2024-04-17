using System.Linq;
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

    private void DataGrid_Loaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
            return;

        var titleColumn = dataGrid.Columns.First(c => c.Tag?.ToString() == nameof(TrackViewModel.Title));
        titleColumn.IsReadOnly = true;

        var yearColumn = dataGrid.Columns.First(c => c.Tag?.ToString() == nameof(TrackViewModel.Year));
        yearColumn.IsReadOnly = true;

        var artistColumn = dataGrid.Columns.First(c => c.Tag?.ToString() == nameof(TrackViewModel.Artist));
        artistColumn.IsReadOnly = true;

        var genreColumn = dataGrid.Columns.First(c => c.Tag?.ToString() == nameof(TrackViewModel.Genre));
        genreColumn.IsReadOnly = true;

        var albumColumn = dataGrid.Columns.First(c => c.Tag?.ToString() == nameof(TrackViewModel.Album));
        albumColumn.IsReadOnly = true;
    }

    private static T? GetDataContext<T>(TappedEventArgs e)
        where T : class
        => (e.Source as StyledElement)?.DataContext as T ?? (e.Source as ContentPresenter)?.Content as T;

    private async void Rubricks_Tapped(object? sender, TappedEventArgs e)
    {
        var rubricViewModel = GetDataContext<RubricViewModel>(e);

        if (rubricViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        var tracks = await rubricViewModel.GetTracks();
        mainViewModel.ChangeTracks(rubricViewModel, tracks);
    }

    private void Playlist_Tapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = GetDataContext<PlaylistViewModel>(e);

        if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        var playlist = playlistViewModel.Playlist;
        mainViewModel.ChangeTracks(playlist, playlist.Tracks);
    }

    private void DataGrid_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
            return;

        if (e.PointerPressedEventArgs.Handled && e.Column is DataGridCheckBoxColumn) // fix
        {
            e.PointerPressedEventArgs.Handled = false;
            dataGrid.CommitEdit();
        }
    }

}

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Sunrise.Views;

public partial class CategoriesTabView : UserControl
{
    public CategoriesTabView()
        => InitializeComponent();

    private async void Category_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Tab)
            await OnCategoryNameChanged(e);
    }

    private async void Category_LostFocus(object? sender, RoutedEventArgs e)
        => await OnCategoryNameChanged(e);

#pragma warning disable CA1822 // Mark members as static
    private async Task OnCategoryNameChanged(RoutedEventArgs e)
#pragma warning restore CA1822 // Mark members as static
    {
        //var playlistViewModel = e.GetDataContext<PlaylistViewModel>();

        //if (playlistViewModel is null || DataContext is not MainViewModel mainViewModel)
        //    return;

        //if (!await mainViewModel.TrackPlay.Player.ChangePlaylistNameAsync(playlistViewModel.Playlist, playlistViewModel.Name))
        //    playlistViewModel.Name = playlistViewModel.Playlist.Name;

        //playlistViewModel.Editing = false;

        await Task.Delay(1);
    }

}
using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;
using Sunrise.ViewModels.Cards;

namespace Sunrise;

public partial class InPlaylistDeviceCardView : UserControl
{
    public InPlaylistDeviceCardView()
        => InitializeComponent();

    private async void AddInPlaylist_Tapped(object? sender, TappedEventArgs e)
    {
        var playlistViewModel = e.GetDataContextWithCheck<PlaylistRubricViewModel>();

        if (playlistViewModel is null || DataContext is not InPlaylistDeviceCardViewModel viewModel)
            return;

        var currentTrack = viewModel.Owner.CurrentTrack?.Track;

        if (currentTrack is null)
            return;

        var tracks = await playlistViewModel.Playlist.GetTracksAsync(viewModel.Owner.Player);

        if (tracks.Count == 0 || tracks[^1] != currentTrack)
        {
            await viewModel.Owner.Player.AddTrackInPlaylistAsync(playlistViewModel.Playlist, currentTrack);
            tracks.Add(currentTrack);
        }

        viewModel.CloseCard();
    }

}
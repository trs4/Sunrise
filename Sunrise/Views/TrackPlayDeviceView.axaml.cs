using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TrackPlayDeviceView : UserControl
{
    public TrackPlayDeviceView()
        => InitializeComponent();

    private void Track_Tapped(object? sender, TappedEventArgs e)
    {
        if (!TryGetCurrentTrack(e, out var viewModel, out _))
            return;

        double position = e.GetPosition(progressBar).X / progressBar.Bounds.Width;
        viewModel!.ChangePosition(position);
    }

    private void Track_PointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);

        if (!point.Properties.IsLeftButtonPressed || !TryGetCurrentTrack(e, out var viewModel, out _))
            return;

        double position = e.GetPosition(progressBar).X / progressBar.Bounds.Width;
        viewModel!.ChangePosition(position);
    }

    private bool TryGetCurrentTrack(RoutedEventArgs e, out TrackPlayViewModel? viewModel, out TrackViewModel? currentTrack)
    {
        viewModel = DataContext as TrackPlayViewModel;
        currentTrack = viewModel?.CurrentTrack;
        return viewModel is not null && currentTrack is not null && ((e.Source as ContentPresenter)?.Content as ContentPresenter)?.Content is not Image;
    }

    private async void TrackTitle_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not TrackPlayViewModel viewModel)
            return;

        var currentTrack = viewModel.CurrentTrack;

        if (currentTrack is null)
            return;

        var mainViewModel = (MainDeviceViewModel)viewModel.Owner;
        var trackPlay = mainViewModel.TrackPlay;

        if (mainViewModel.TracksOwner is not ArtistViewModel artistViewModel || artistViewModel.Name != currentTrack.Artist)
        {
            var ownerRubric = mainViewModel.Artists;
            var tracksScreenshot = await trackPlay.Player.GetTracksAsync();

            if (!tracksScreenshot.TracksByArtist.TryGetValue(currentTrack.Artist, out var tracksByAlbums))
                return;

            var ownerTrackSource = ArtistViewModel.Create(ownerRubric, currentTrack.Artist, tracksByAlbums);
            trackPlay.ChangeOwnerRubric(ownerRubric, ownerTrackSource);
            await mainViewModel.ChangeTracksAsync(ownerTrackSource);
        }

        mainViewModel.HideTrackPage();
    }

}

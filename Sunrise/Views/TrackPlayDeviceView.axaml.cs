using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TrackPlayDeviceView : UserControl
{
    public TrackPlayDeviceView()
        => InitializeComponent();

    private void Volume_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not TrackPlayViewModel viewModel)
            return;

        double position = e.GetPosition(volumeSlider).X / volumeSlider.Bounds.Width;
        viewModel.Volume = Math.Round(position * 100);
    }

    private void Track_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not TrackPlayViewModel viewModel || ((e.Source as ContentPresenter)?.Content as ContentPresenter)?.Content is Image)
            return;

        var currentTrack = viewModel.CurrentTrack;

        if (currentTrack is null)
            return;

        double position = e.GetPosition(progressBar).X / progressBar.Bounds.Width;
        viewModel.ChangePosition(position);
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

using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TrackPlayDeviceView : UserControl
{
    private double _startPositionY;

    public TrackPlayDeviceView()
        => InitializeComponent();

    private void TrackIcon_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var parent = (Control)Parent!;
        var point = e.GetCurrentPoint(parent);
        _startPositionY = point.Position.Y;
    }

    private void TrackIcon_PointerMoved(object? sender, PointerEventArgs e)
    {
        var parent = (Control)Parent!;
        var point = e.GetCurrentPoint(parent);

        if (!point.Properties.IsLeftButtonPressed)
            return;

        double offset = point.Position.Y - _startPositionY;

        if (offset < 0)
        {
            offset = -Math.Log(-offset, 1.05);
            Opacity = 1;
        }
        else
            Opacity = 1 - 0.2 * (point.Position.Y / parent.Bounds.Height);

        Margin = new Avalonia.Thickness(0, offset, 0, -offset);
    }

    private void TrackIcon_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var parent = (Control)Parent!;
        var point = e.GetCurrentPoint(parent);

        if (point.Position.Y > parent.Bounds.Height * 0.9 && DataContext is TrackPlayViewModel viewModel)
        {
            var mainViewModel = (MainDeviceViewModel)viewModel.Owner;
            mainViewModel.HideTrackPage();
        }

        Opacity = 1;
        Margin = default;
        _startPositionY = 0;
    }

    private void Track_Tapped(object? sender, TappedEventArgs e)
    {
        if (!TryGetCurrentTrack(e, out var viewModel, out _))
            return;

        double position = e.GetPosition(progressBar).X / progressBar.Bounds.Width;
        viewModel!.ChangePositionDelay(position);
    }

    private void Track_PointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);

        if (!point.Properties.IsLeftButtonPressed || !TryGetCurrentTrack(e, out var viewModel, out _))
            return;

        double position = e.GetPosition(progressBar).X / progressBar.Bounds.Width;
        viewModel!.ChangePositionDelay(position);
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

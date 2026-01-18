using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TrackPlayView : UserControl
{
    public TrackPlayView()
        => InitializeComponent();

    private void Volume_Tapped(object? sender, TappedEventArgs e)
    {
        if (!e.CanClick() || DataContext is not TrackPlayDesktopViewModel viewModel)
            return;

        double position = e.GetPosition(volumeSlider).X / volumeSlider.Bounds.Width;
        viewModel.Volume = Math.Round(position * 100);
    }

    private void Volume_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not TrackPlayDesktopViewModel viewModel)
            return;

        if (e.Delta.Y < 0)
            viewModel.Volume--;
        else
            viewModel.Volume++;
    }

    private void Volume_PointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);

        if (!point.Properties.IsLeftButtonPressed || DataContext is not TrackPlayDesktopViewModel viewModel)
            return;

        double position = e.GetPosition(volumeSlider).X / volumeSlider.Bounds.Width;
        viewModel.Volume = Math.Round(position * 100);
    }

    private void Track_Tapped(object? sender, TappedEventArgs e)
    {
        if (!e.CanClick() || !TryGetCurrentTrack(e, out var viewModel, out _))
            return;

        double position = e.GetPosition(progressBar).X / progressBar.Bounds.Width;
        viewModel!.ChangePositionDelay(position);
    }

    private void Track_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!TryGetCurrentTrack(e, out var viewModel, out var currentTrack))
            return;

        double position = 1 / currentTrack!.Duration.TotalSeconds;

        if (e.Delta.Y < 0)
            position = -position;

        position += viewModel!.Player.Media.Position;
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

}

using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TrackPlayView : UserControl
{
    public TrackPlayView()
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

}

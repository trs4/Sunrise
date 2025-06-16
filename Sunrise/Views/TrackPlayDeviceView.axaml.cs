using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TrackPlayDeviceView : UserControl
{
    public TrackPlayDeviceView()
        => InitializeComponent();

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

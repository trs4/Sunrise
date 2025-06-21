using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TracksTabView : UserControl
{
    public TracksTabView()
        => InitializeComponent();

    private async void Track_Tapped(object? sender, TappedEventArgs e)
    {
        var trackViewModel = e.GetDataContext<TrackViewModel>();

        if (trackViewModel is null || trackViewModel.IsPlaying == true || DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.IsShortTrackVisible = true;
        await mainViewModel.TrackPlay.PlayAsync(trackViewModel);
    }

    private void TrackIcon_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.TrackPlay.PlayCommand.Execute(null);
        e.Handled = true;
    }

}
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

        mainViewModel.IsTrackVisible = true;
        await mainViewModel.TrackPlay.PlayAsync(trackViewModel);
    }

}
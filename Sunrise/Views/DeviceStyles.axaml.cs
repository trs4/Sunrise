using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class DeviceStyles : ResourceDictionary
{
    private void TrackIcon_Tapped(object? sender, TappedEventArgs e)
    {
        var mainViewModel = e.FindDataContextWithCheck<MainDeviceViewModel>();

        if (mainViewModel is null)
            return;

        mainViewModel.TrackPlay.PlayCommand.Execute(null);
        e.Handled = true;
    }

    private async void TrackTransition_Tapped(object? sender, TappedEventArgs e)
    {
        var trackTransitionViewModel = e.FindDataContextWithCheck<TrackTransitionViewModel>();

        if (trackTransitionViewModel is null)
            return;

        await trackTransitionViewModel.OnTapAsync();
        e.Handled = true;
    }

}

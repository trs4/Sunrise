using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class DeviceStyles : ResourceDictionary
{
    private void TrackIcon_Tapped(object? sender, TappedEventArgs e)
    {
        var mainViewModel = e.FindDataContext<MainDeviceViewModel>();

        if (mainViewModel is null)
            return;

        mainViewModel.TrackPlay.PlayCommand.Execute(null);
        e.Handled = true;
    }

}

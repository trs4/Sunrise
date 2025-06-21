using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TrackPlayShortDeviceView : UserControl
{
    public TrackPlayShortDeviceView()
        => InitializeComponent();

    private void Track_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not TrackPlayViewModel viewModel || viewModel.Owner is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.ShowTrackPage();
    }

}

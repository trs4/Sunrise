using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TracksTabView : UserControl
{
    public TracksTabView()
        => InitializeComponent();

    private async void RecentlyAddedTrack_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainDeviceViewModel mainViewModel)
            await PlayHelper.PlayRecentlyAddedTrackAsync(mainViewModel, e);
    }

    private async void Track_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainDeviceViewModel mainViewModel)
            await PlayHelper.PlayTrackAsync(mainViewModel, e);
    }

    private async void TrackSource_Tapped(object? sender, TappedEventArgs e)
    {
        var trackSourceViewModel = e.GetDataContext<TrackSourceViewModel>();

        if (trackSourceViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.TrackPlay.ChangeOwnerRubric(trackSourceViewModel);
        await mainViewModel.ChangeTracksAsync(trackSourceViewModel);
    }

}
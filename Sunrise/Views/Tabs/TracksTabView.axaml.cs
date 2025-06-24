using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class TracksTabView : UserControl
{
    public TracksTabView()
        => InitializeComponent();

    private async void RecentlyAddedTrack_Tapped(object? sender, TappedEventArgs e)
        => await PlayTrack(e, mainViewModel => mainViewModel.RecentlyAddedRubric);

    private async void Track_Tapped(object? sender, TappedEventArgs e)
        => await PlayTrack(e, mainViewModel => mainViewModel.TrackSourceHistory.LastOrDefault() as RubricViewModel
            ?? mainViewModel.SelectedRubrick);

    private async void TrackSource_Tapped(object? sender, TappedEventArgs e)
    {
        var trackSourceViewModel = e.GetDataContext<TrackSourceViewModel>();

        if (trackSourceViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.TrackPlay.ChangeOwnerRubric(trackSourceViewModel);
        await mainViewModel.ChangeTracksAsync(trackSourceViewModel);
    }

    private async Task PlayTrack(TappedEventArgs e, Func<MainDeviceViewModel, RubricViewModel> getRubric)
    {
        var trackViewModel = e.GetDataContext<TrackViewModel>();

        if (trackViewModel is null || DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.IsShortTrackVisible = true;
        var trackPlay = mainViewModel.TrackPlay;
        trackPlay.ChangeOwnerRubric(getRubric(mainViewModel));

        if (trackViewModel.IsPlaying == true)
            return;

        await trackPlay.PlayAsync(trackViewModel);
    }

    private void TrackIcon_Tapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.TrackPlay.PlayCommand.Execute(null);
        e.Handled = true;
    }

}
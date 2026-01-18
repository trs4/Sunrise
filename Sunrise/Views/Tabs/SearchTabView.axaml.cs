using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class SearchTabView : UserControl
{
    public SearchTabView()
        => InitializeComponent();

    private async void Track_Tapped(object? sender, TappedEventArgs e)
    {
        if (!e.CanClick() || DataContext is not MainDeviceViewModel mainViewModel)
            return;

        await PlayHelper.PlaySearchTrackAsync(mainViewModel, e);
    }

    private async void TrackSource_Tapped(object? sender, TappedEventArgs e)
    {
        var trackSourceViewModel = e.GetDataContextWithCheck<TrackSourceViewModel>();

        if (trackSourceViewModel is null || DataContext is not MainDeviceViewModel mainViewModel)
            return;

        mainViewModel.SelectedTab = DeviceTabs.Tracks;
        mainViewModel.SelectedRubrick = trackSourceViewModel.Rubric;
        mainViewModel.SelectedTrackSource = trackSourceViewModel;
        mainViewModel.TrackPlay.ChangeOwnerRubric(trackSourceViewModel.Rubric, trackSourceViewModel);
        await mainViewModel.ChangeTracksAsync(trackSourceViewModel);
    }

}
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;

namespace Sunrise.ViewModels;

internal static class PlayHelper
{
    public static Task PlayTrackAsync(MainDeviceViewModel mainViewModel, TappedEventArgs e)
        => PlayTrack(mainViewModel, e, mainViewModel =>
        {
            var trackSourceHistory = mainViewModel.TrackSourceHistory;
            var ownerTrackSource = trackSourceHistory.Count >= 2 ? trackSourceHistory[^1] as TrackSourceViewModel : null;

            var ownerRubric = ownerTrackSource is not null
                ? trackSourceHistory[^2]
                : trackSourceHistory.Count == 1 ? trackSourceHistory[0] : null;

            return (ownerRubric ?? mainViewModel.SelectedRubrick, ownerTrackSource);
        });

    public static Task PlayRecentlyAddedTrackAsync(MainDeviceViewModel mainViewModel, TappedEventArgs e)
        => PlayTrack(mainViewModel, e, mainViewModel => (mainViewModel.RecentlyAddedRubric, null));

    public static Task PlaySearchTrackAsync(MainDeviceViewModel mainViewModel, TappedEventArgs e)
        => PlayTrack(mainViewModel, e, mainViewModel =>
        {
            var tracks = mainViewModel.SearchTracks.Select(t => t.Track).ToList();
            var rubricViewModel = new SearchRubricViewModel(mainViewModel.TrackPlay.Player, tracks);
            return (rubricViewModel, null);
        });

    private static async Task PlayTrack(MainDeviceViewModel mainViewModel, TappedEventArgs e,
        Func<MainDeviceViewModel, (RubricViewModel, TrackSourceViewModel?)> getOwners)
    {
        var trackViewModel = e.GetDataContext<TrackViewModel>();

        if (trackViewModel is null)
            return;
         
        var (ownerRubric, ownerTrackSource) = getOwners(mainViewModel);
        mainViewModel.IsShortTrackVisible = true;
        var trackPlay = mainViewModel.TrackPlay;
        trackPlay.ChangeOwnerRubric(ownerRubric, ownerTrackSource);

        if (trackViewModel.IsPlaying == true)
            return;

        await trackPlay.PlayAsync(trackViewModel);
    }

}

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Sunrise.Model;
using Sunrise.Utils;
using Sunrise.ViewModels;
using Sunrise.ViewModels.Interfaces;

namespace Sunrise.Views;

public partial class TracksTabView : UserControl, ITracksView
{
    public TracksTabView()
        => InitializeComponent();

    private async void RecentlyAddedTrack_Tapped(object? sender, TappedEventArgs e)
    {
        if (!e.CanClick() || DataContext is not MainDeviceViewModel mainViewModel)
            return;

        await PlayHelper.PlayRecentlyAddedTrackAsync(mainViewModel, e);
    }

    private async void Track_Tapped(object? sender, TappedEventArgs e)
    {
        if (!e.CanClick() || DataContext is not MainDeviceViewModel mainViewModel)
            return;

        await PlayHelper.PlayTrackAsync(mainViewModel, e);
    }

    private async void TrackSource_Tapped(object? sender, TappedEventArgs e)
    {
        var trackSourceViewModel = e.GetDataContextWithCheck<TrackSourceViewModel>();

        if (trackSourceViewModel is null || DataContext is not MainViewModel mainViewModel)
            return;

        mainViewModel.TrackPlay.ChangeOwnerRubric(trackSourceViewModel);
        await mainViewModel.ChangeTracksAsync(trackSourceViewModel);
    }

    private async void TrackSourceCaption_Tapped(object? sender, TappedEventArgs e)
    {
        if (!e.CanClick() || DataContext is not MainViewModel mainViewModel)
            return;

        var selectedTrackSource = mainViewModel.SelectedTrackSource;

        if (selectedTrackSource is AlbumViewModel albumViewModel) // Select artist
        {
            var trackPlay = mainViewModel.TrackPlay;
            var ownerRubric = mainViewModel.Artists;
            var tracksScreenshot = await trackPlay.Player.GetTracksAsync();

            if (!tracksScreenshot.TracksByArtist.TryGetValue(albumViewModel.Artist, out var tracksByAlbums))
                return;

            var ownerTrackSource = ArtistViewModel.Create(ownerRubric, albumViewModel.Artist, tracksByAlbums);
            trackPlay.ChangeOwnerRubric(ownerRubric, ownerTrackSource);
            await mainViewModel.ChangeTracksAsync(ownerTrackSource);
        }
    }

    #region ITracksView

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainViewModel mainViewModel)
            mainViewModel.TracksView = this;
    }

    void ITracksView.ScrollIntoView(Track track)
    {
        if (DataContext is not MainViewModel mainViewModel)
            return;

        int id = track.Id;
        var tracks = mainViewModel.Tracks;
        int index = -1;
        TrackViewModel selectedTrackViewModel = null;

        for (int i = 0; i < tracks.Count; i++)
        {
            var trackViewModel = tracks[i];

            if (trackViewModel.Track.Id == id)
            {
                index = i;
                selectedTrackViewModel = trackViewModel;
                break;
            }
        }

        if (index == -1)
            return;

        UIDispatcher.RunBackground(() =>
        {
            tracksBox.ScrollIntoView(index);
            mainViewModel.SelectedTrack = selectedTrackViewModel;
        });
    }

    void ITracksView.ScrollIntoView(string trackSource)
    {
        if (DataContext is not MainViewModel mainViewModel)
            return;

        var trackSources = mainViewModel.TrackSources;
        int index = -1;
        TrackSourceViewModel? selectedTrackSourceViewModel = null;

        for (int i = 0; i < trackSources.Count; i++)
        {
            var trackSourceViewModel = trackSources[i];

            if (trackSourceViewModel.Name == trackSource)
            {
                index = i;
                selectedTrackSourceViewModel = trackSourceViewModel;
                break;
            }
        }

        if (index == -1)
            return;

        UIDispatcher.RunBackground(() =>
        {
            trackSourcesBox.ScrollIntoView(index);
            mainViewModel.SelectedTrackSource = selectedTrackSourceViewModel;
        });
    }

    #endregion
}
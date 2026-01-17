using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public abstract class TrackTransitionViewModel : ObservableObject
{
    protected TrackTransitionViewModel(TrackPlayDeviceViewModel owner, string name, Track track)
    {
        Owner = owner;
        Name = name;
        Track = track;
    }

    public TrackPlayDeviceViewModel Owner { get; }

    public string Name { get; }

    public Track Track { get; }

    public abstract Task OnTapAsync();

    public override string ToString() => Name;
}

public class InPlaylistTrackTransitionViewModel : TrackTransitionViewModel
{
    public InPlaylistTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.InPlaylist, track) { }

    public override Task OnTapAsync()
    {
        Owner.ShowCard = true;
        return Task.CompletedTask;
    }

}

public class LyricsTrackTransitionViewModel : TrackTransitionViewModel
{
    public LyricsTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.Text, track) { }

    public override Task OnTapAsync()
    {
        Owner.ShowLyrics = !Owner.ShowLyrics;
        return Task.CompletedTask;
    }

}

public class ArtistTrackTransitionViewModel : TrackTransitionViewModel
{
    public ArtistTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.Artist, track) { }

    public override async Task OnTapAsync()
    {
        var mainViewModel = Owner.Owner;
        var ownerRubric = mainViewModel.Artists;
        var tracksScreenshot = await Owner.Player.GetTracksAsync();

        if (!tracksScreenshot.TracksByArtist.TryGetValue(Track.Artist, out var tracksByAlbums))
            return;

        mainViewModel.TrackSourceHistory.Clear();
        mainViewModel.TrackSourceHistory.Add(ownerRubric);

        var ownerTrackSource = ArtistViewModel.Create(ownerRubric, Track.Artist, tracksByAlbums);
        mainViewModel.SelectedRubrick = ownerRubric;
        mainViewModel.SelectedTab = DeviceTabs.Tracks;
        mainViewModel.HideTrackPage();

        Owner.ChangeOwnerRubric(ownerTrackSource);
        await mainViewModel.ChangeTracksAsync(ownerTrackSource);
    }

}

public class AlbumTrackTransitionViewModel : TrackTransitionViewModel
{
    public AlbumTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.Album, track) { }

    public override async Task OnTapAsync()
    {
        var mainViewModel = Owner.Owner;
        var ownerRubric = mainViewModel.Albums;
        var tracksScreenshot = await Owner.Player.GetTracksAsync();

        if (!tracksScreenshot.TracksByArtist.TryGetValue(Track.Artist, out var tracksByAlbums)
            || !tracksByAlbums.TryGetValue(Track.Album, out var tracks))
        {
            return;
        }

        mainViewModel.TrackSourceHistory.Clear();
        mainViewModel.TrackSourceHistory.Add(ownerRubric);

        var ownerTrackSource = new AlbumViewModel(ownerRubric, Track.Album, Track.Artist, tracks);
        mainViewModel.SelectedRubrick = ownerRubric;
        mainViewModel.SelectedTab = DeviceTabs.Tracks;
        mainViewModel.HideTrackPage();

        Owner.ChangeOwnerRubric(ownerTrackSource);
        await mainViewModel.ChangeTracksAsync(ownerTrackSource);
    }

}

public class CurrentRubricTrackTransitionViewModel : TrackTransitionViewModel
{
    public CurrentRubricTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track, RubricViewModel rubric)
        : base(owner, rubric.Name, track)
        => Rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));

    public RubricViewModel Rubric { get; }

    public override Task OnTapAsync() => Owner.Owner.OnNextListAsync();
}

public class HistoryTrackTransitionViewModel : TrackTransitionViewModel
{
    public HistoryTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.History, track) { }

    public override Task OnTapAsync()
    {
        var mainViewModel = Owner.Owner;
        var ownerRubric = mainViewModel.History;
        mainViewModel.SelectedTab = DeviceTabs.Tracks;
        mainViewModel.HideTrackPage();

        Owner.ChangeOwnerRubric(ownerRubric);
        return mainViewModel.ChangeTracksAsync(ownerRubric);
    }

}

public class InformationTrackTransitionViewModel : TrackTransitionViewModel
{
    public InformationTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.Information, track) { }

    public override Task OnTapAsync()
    {





        // %%TODO
        return Task.CompletedTask;
    }

}

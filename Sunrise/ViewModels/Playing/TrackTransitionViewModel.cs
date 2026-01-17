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


        return Task.CompletedTask;
    }

}

public class LyricsTrackTransitionViewModel : TrackTransitionViewModel
{
    public LyricsTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.Text, track) { }

    public override Task OnTapAsync()
    {


        return Task.CompletedTask;
    }

}

public class ArtistTrackTransitionViewModel : TrackTransitionViewModel
{
    public ArtistTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.Artist, track) { }

    public override Task OnTapAsync()
    {


        return Task.CompletedTask;
    }

}

public class AlbumTrackTransitionViewModel : TrackTransitionViewModel
{
    public AlbumTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.Album, track) { }

    public override Task OnTapAsync()
    {


        return Task.CompletedTask;
    }

}

public class CurrentRubricTrackTransitionViewModel : TrackTransitionViewModel
{
    public CurrentRubricTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track, RubricViewModel rubric)
        : base(owner, rubric.Name, track)
        => Rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));

    public RubricViewModel Rubric { get; }

    public override Task OnTapAsync()
    {


        return Task.CompletedTask;
    }

}

public class HistoryTrackTransitionViewModel : TrackTransitionViewModel
{
    public HistoryTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.History, track) { }

    public override Task OnTapAsync()
    {
        var mainViewModel = Owner.Owner;
        var rubricViewModel = mainViewModel.History;
        mainViewModel.SelectedTab = DeviceTabs.Tracks;
        mainViewModel.HideTrackPage();

        Owner.ChangeOwnerRubric(rubricViewModel);
        return mainViewModel.ChangeTracksAsync(rubricViewModel);
    }

}

public class InformationTrackTransitionViewModel : TrackTransitionViewModel
{
    public InformationTrackTransitionViewModel(TrackPlayDeviceViewModel owner, Track track) : base(owner, Texts.Information, track) { }

    public override Task OnTapAsync()
    {






        return Task.CompletedTask;
    }

}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.ViewModels.Cards;

namespace Sunrise.ViewModels;

public sealed class TrackPlayDeviceViewModel : TrackPlayViewModel
{
    private bool _showLyrics;
    private DeviceCardViewModel? _cardDialog;

    public TrackPlayDeviceViewModel() { } // For designer

    public TrackPlayDeviceViewModel(MainViewModel owner, Player player) : base(owner, player) { }

    public new MainDeviceViewModel Owner => (MainDeviceViewModel)base.Owner;

    public bool ShowLyrics
    {
        get => _showLyrics;
        set => SetProperty(ref _showLyrics, value);
    }

    public DeviceCardViewModel? CardDialog
    {
        get => _cardDialog;
        set => SetProperty(ref _cardDialog, value);
    }

    public ObservableCollection<TrackTransitionViewModel> Transitions { get; } = [];

    protected override void PlayCore(TrackViewModel trackViewModel, bool toStart = false)
    {
        if (!Owner.IsShortTrackVisible && !Owner.IsTrackVisible)
            Owner.IsShortTrackVisible = true;

        base.PlayCore(trackViewModel, toStart);
    }

    protected override void OnChangeTrack(TrackViewModel trackViewModel)
    {
        base.OnChangeTrack(trackViewModel);
        ShowLyrics = false;
    }

    protected override async ValueTask OnTracksEndedAsync()
    {
        Stop();
        var track = await Strategy.GetFirstAsync();

        if (track is not null)
            Change(track);
    }

    private void FillTransitions(Track track)
    {
        var currentRubric = CurrentRubric;
        Transitions.Add(new InPlaylistTrackTransitionViewModel(this, track));

        if (!string.IsNullOrEmpty(track.Lyrics) || !string.IsNullOrEmpty(track.Translate))
            Transitions.Add(new LyricsTrackTransitionViewModel(this, track));

        if (!string.IsNullOrEmpty(track.Artist))
            Transitions.Add(new ArtistTrackTransitionViewModel(this, track));

        if (!string.IsNullOrEmpty(track.Album))
            Transitions.Add(new AlbumTrackTransitionViewModel(this, track));

        if (currentRubric is not null)
            Transitions.Add(new CurrentRubricTrackTransitionViewModel(this, track, currentRubric));

        Transitions.Add(new HistoryTrackTransitionViewModel(this, track));
        Transitions.Add(new InformationTrackTransitionViewModel(this, track));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(CurrentTrack))
        {
            Transitions.Clear();
            var currentTrack = CurrentTrack;

            if (currentTrack is not null)
                FillTransitions(currentTrack.Track);
        }
    }

}

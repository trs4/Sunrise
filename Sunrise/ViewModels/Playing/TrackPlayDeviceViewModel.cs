using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public sealed class TrackPlayDeviceViewModel : TrackPlayViewModel
{
    private bool _isChanging;
    private PlaylistRubricViewModel? _selectedPlaylist;
    private string _changingText = Texts.Change;
    private bool _isSelectPlaylist;

    public TrackPlayDeviceViewModel() { } // For designer

    public TrackPlayDeviceViewModel(MainViewModel owner, Player player)
        : base(owner, player)
    {
        ChangeTrackCommand = new RelayCommand(OnChangeTrack);
        AddTrackInPlaylistCommand = new AsyncRelayCommand(OnAddTrackInPlaylistAsync);
        DeleteTrackCommand = new AsyncRelayCommand(OnDeleteTrackAsync);
    }

    public new MainDeviceViewModel Owner => (MainDeviceViewModel)base.Owner;

    public IRelayCommand ChangeTrackCommand { get; }

    public bool IsChanging
    {
        get => _isChanging;
        set => SetProperty(ref _isChanging, value);
    }

    public PlaylistRubricViewModel? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set => SetProperty(ref _selectedPlaylist, value);
    }

    public string ChangingText
    {
        get => _changingText;
        set => SetProperty(ref _changingText, value);
    }

    public bool IsSelectPlaylist
    {
        get => _isSelectPlaylist;
        set => SetProperty(ref _isSelectPlaylist, value);
    }

    public IRelayCommand AddTrackInPlaylistCommand { get; }

    public IRelayCommand DeleteTrackCommand { get; }

    public ObservableCollection<TrackTransitionViewModel> Transitions { get; } = [];

    protected override void PlayCore(TrackViewModel trackViewModel, bool toStart = false)
    {
        var owner = (MainDeviceViewModel)Owner;

        if (!owner.IsShortTrackVisible && !owner.IsTrackVisible)
            owner.IsShortTrackVisible = true;

        base.PlayCore(trackViewModel, toStart);
    }

    private void OnChangeTrack()
    {
        IsChanging = !IsChanging;
        ChangingText = IsChanging ? Texts.Cancel : Texts.Change;
        IsSelectPlaylist = false;
    }

    public void CancelChangeTrack()
    {
        IsChanging = false;
        IsSelectPlaylist = false;
        SelectedPlaylist = null;
        ChangingText = Texts.Change;
    }

    private async Task OnAddTrackInPlaylistAsync()
    {
        if (IsSelectPlaylist)
        {
            var currentTrack = CurrentTrack?.Track;
            var selectedPlaylist = _selectedPlaylist;

            if (currentTrack is not null && selectedPlaylist is not null)
            {
                var tracks = selectedPlaylist.Playlist.Tracks;

                if (tracks.Count == 0 || tracks[^1] != currentTrack)
                {
                    await Player.AddTrackInPlaylistAsync(selectedPlaylist.Playlist, currentTrack);
                    tracks.Add(currentTrack);
                }
            }

            CancelChangeTrack();
        }
        else
        {
            SelectedPlaylist = Owner.Playlists.FirstOrDefault();
            IsSelectPlaylist = true;
        }
    }

    private async Task OnDeleteTrackAsync()
    {
        var currentTrack = CurrentTrack;

        if (currentTrack is null)
            return;

        await GoToNextTrackAsync();
        await Player.DeleteTrackAsync(currentTrack.Track);
        await Owner.RemoveAsync(currentTrack.Track);
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

using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public sealed class TrackPlayDeviceViewModel : TrackPlayViewModel
{
    private bool _isChanging;
    private PlaylistViewModel? _selectedPlaylist;
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

    public IRelayCommand ChangeTrackCommand { get; }

    public bool IsChanging
    {
        get => _isChanging;
        set => SetProperty(ref _isChanging, value);
    }

    public PlaylistViewModel? SelectedPlaylist
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

}

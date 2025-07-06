using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels;

public sealed class TrackPlayDeviceViewModel : TrackPlayViewModel
{
    private bool _isChanging;
    private string _changingText = Texts.Change;
    private bool _isSelectPlaylist;

    public TrackPlayDeviceViewModel() { } // For designer

    public TrackPlayDeviceViewModel(MainViewModel owner, Player player)
        : base(owner, player)
    {
        ChangeTrackCommand = new RelayCommand(OnChangeTrack);
        AddTrackInPlaylistCommand = new RelayCommand(OnAddTrackInPlaylist);
        DeleteTrackCommand = new AsyncRelayCommand(OnDeleteTrackAsync);
    }

    public IRelayCommand ChangeTrackCommand { get; }

    public bool IsChanging
    {
        get => _isChanging;
        set => SetProperty(ref _isChanging, value);
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
        ChangingText = Texts.Change;
        IsSelectPlaylist = false;
    }

    private void OnAddTrackInPlaylist()
        => IsSelectPlaylist = true;

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

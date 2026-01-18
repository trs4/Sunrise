using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels.Cards;

public class InformationDeviceCardViewModel : DeviceCardViewModel
{
    private bool _hasPlaylist;

    public InformationDeviceCardViewModel(TrackPlayDeviceViewModel owner)
        : base(owner)
    {
        DeleteFromPlaylistCommand = new AsyncRelayCommand(OnDeleteFromPlaylistAsync);
        DeleteFromMediaCommand = new AsyncRelayCommand(OnDeleteFromMediaAsync);
        HasPlaylist = owner.CurrentRubric is PlaylistRubricViewModel;
    }

    public IRelayCommand DeleteFromPlaylistCommand { get; }

    public IRelayCommand DeleteFromMediaCommand { get; }

    public bool HasPlaylist
    {
        get => _hasPlaylist;
        set => SetProperty(ref _hasPlaylist, value);
    }

    private async Task OnDeleteFromPlaylistAsync()
    {
        if (await MessageBoxManager.GetMessageBoxStandard(string.Empty, Texts.DeleteFromPlaylist + "?", ButtonEnum.OkCancel).ShowAsync() != ButtonResult.Ok)
            return;

        var currentTrack = Owner.CurrentTrack?.Track;
        var currentPlaylist = (Owner.CurrentRubric as PlaylistRubricViewModel)?.Playlist;

        if (currentTrack is null || currentPlaylist is null)
            return;

        await Owner.GoToNextTrackAsync();
        await Owner.Player.DeleteTrackInPlaylistAsync(currentPlaylist, currentTrack);
    }

    private async Task OnDeleteFromMediaAsync()
    {
        if (await MessageBoxManager.GetMessageBoxStandard(string.Empty, Texts.DeleteFromMedia + "?", ButtonEnum.OkCancel).ShowAsync() != ButtonResult.Ok)
            return;

        var currentTrack = Owner.CurrentTrack?.Track;

        if (currentTrack is null)
            return;

        await Owner.GoToNextTrackAsync();
        await Owner.Player.DeleteTrackAsync(currentTrack);
        await Owner.Owner.RemoveAsync(currentTrack);
    }

}

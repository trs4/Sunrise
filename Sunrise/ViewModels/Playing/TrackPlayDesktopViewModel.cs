using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Services;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public sealed class TrackPlayDesktopViewModel : TrackPlayViewModel
{
    public TrackPlayDesktopViewModel() { } // For designer

    public TrackPlayDesktopViewModel(MainViewModel owner, Player player)
        : base(owner, player)
        => ImportFromITunesCommand = new AsyncRelayCommand(OnImportFromITunesAsync);

    public IRelayCommand ImportFromITunesCommand { get; }

    private async Task OnImportFromITunesAsync(CancellationToken token)
    {
        string? filePath = null;

        if (!UIDispatcher.Run(() => AppServices.Get<ISystemDialogsService>().ShowSelectFile(out filePath)))
            return;

        await ImportFromITunes.LoadAsync(Player, filePath, token: token);
        Player.ClearTracks();
        Player.ClearPlaylists();
        await Owner.ReloadTracksAsync(token);
    }

}

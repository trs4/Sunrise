using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Model.Model;
using Sunrise.Services;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public sealed class TrackPlayDesktopViewModel : TrackPlayViewModel
{
    private double _volume = 15d;

    public TrackPlayDesktopViewModel() { } // For designer

    public TrackPlayDesktopViewModel(MainViewModel owner, Player player)
        : base(owner, player)
    {
        ExportCommand = new AsyncRelayCommand(OnExportAsync);
        ImportFromITunesCommand = new AsyncRelayCommand(OnImportFromITunesAsync);
        Player.Media.Volume = _volume;
    }

    public new MainDesktopViewModel Owner => (MainDesktopViewModel)base.Owner;

    public IRelayCommand ExportCommand { get; }

    public IRelayCommand ImportFromITunesCommand { get; }

    public double Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Volume))
        {
            if (_volume < 0)
                Volume = 0;
            else if (_volume > 100d)
                Volume = 100d;

            Player.Media.Volume = _volume;
        }
    }

    private async Task OnExportAsync(CancellationToken token)
    {
        string? filePath = null;

        if (!UIDispatcher.Run(() => AppServices.Get<ISystemDialogsService>().SaveFile(out filePath)))
            return;

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await MediaExporter.ExportAsync(Player, stream, token);
    }

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

    protected override ValueTask OnTracksEndedAsync()
    {
        Clear();
        return default;
    }

}

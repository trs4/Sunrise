using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Converters;
using Sunrise.Model;
using Sunrise.Model.Communication;
using Sunrise.Model.Discovery;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Columns;

namespace Sunrise.ViewModels;

public sealed class MainDesktopViewModel : MainViewModel
{
    private IDisposable? _discoveryClient;
    private DiscoveryDeviceInfo _deviceInfo;
    private string _device;
    private bool _deviceLocked;
    private string _deviceStatus;

    public MainDesktopViewModel() { } // For designer

    public MainDesktopViewModel(Player player)
        : base(player)
    {
        Dispatcher = new(player);
        AddFolderCommand = new AsyncRelayCommand(AddFolderAsync);
        DoubleClickCommand = new AsyncRelayCommand<TrackViewModel>(OnDoubleClickAsync);
        DisconnectDeviceCommand = new AsyncRelayCommand(OnDisconnectDeviceAsync);
        SynchronizeDeviceCommand = new AsyncRelayCommand(OnSynchronizeDeviceAsync);
        ClearDeviceCommand = new AsyncRelayCommand(OnClearDeviceAsync);
        InitTracksColumns();

        _discoveryClient = DiscoveryClient.Search(OnDeviceDetected);
    }

    public Window Owner { get; internal set; }

    public SyncDispatcher Dispatcher { get; }

    public IRelayCommand AddFolderCommand { get; }

    public IRelayCommand DoubleClickCommand { get; }

    public ObservableCollection<ColumnViewModel> TracksColumns { get; } = [];

    public string Device
    {
        get => _device;
        set => SetProperty(ref _device, value);
    }

    public bool DeviceLocked
    {
        get => _deviceLocked;
        set => SetProperty(ref _deviceLocked, value);
    }

    public string DeviceStatus
    {
        get => _deviceStatus;
        set => SetProperty(ref _deviceStatus, value);
    }

    public IRelayCommand DisconnectDeviceCommand { get; }

    public IRelayCommand SynchronizeDeviceCommand { get; }

    public IRelayCommand ClearDeviceCommand { get; }

    private void OnDeviceDetected(DiscoveryDeviceInfo deviceInfo)
    {
        var discoveryClient = _discoveryClient;

        if (discoveryClient is not null)
        {
            discoveryClient.Dispose();
            _discoveryClient = null;
        }

        _deviceInfo = deviceInfo;
        Device = deviceInfo.DeviceName;

        bool settingsDisplayed = SettingsDisplayed;

        if (settingsDisplayed)
        {
            InitInfo();
            Info += Environment.NewLine + $"DeviceDetected {deviceInfo.IPAddress} {deviceInfo.Port}";
        }
    }

    private async Task OnDisconnectDeviceAsync(CancellationToken token)
    {
        if (_deviceInfo is null)
            return;

        //await Dispatcher.ClearAsync(token);
        await Task.Delay(1, token);
    }

    private async Task OnSynchronizeDeviceAsync(CancellationToken token)
    {
        if (_deviceInfo is null)
            return;

        await Dispatcher.SynchronizeAsync(token);
    }

    private async Task OnClearDeviceAsync(CancellationToken token)
    {
        if (_deviceInfo is null)
            return;

        await Dispatcher.ClearAsync(token);
    }

    protected override TrackPlayViewModel CreateTrackPlay(Player player) => new TrackPlayDesktopViewModel(this, player);

    protected override Task SelectTracksAsync(object tracksOwner, bool changeTracks = true, CancellationToken token = default)
    {
        var pickedColumn = TracksColumns.First(c => c.Name == nameof(TrackViewModel.Picked));
        pickedColumn.IsVisible = tracksOwner is SongsRubricViewModel;

        return base.SelectTracksAsync(tracksOwner, changeTracks, token);
    }

    private async Task AddFolderAsync(CancellationToken token)
    {
        await MediaFoldersViewModel.ShowAsync(Owner, TrackPlay.Player, token);

        if (TrackPlay.Player.IsTracksLoaded())
            return;

        await ReloadTracksAsync(token);
    }

    private async Task OnDoubleClickAsync(TrackViewModel? trackViewModel)
    {
        if (trackViewModel is not null)
            await TrackPlay.PlayItBeginAsync(trackViewModel);
    }

    private void InitTracksColumns()
    {
        TracksColumns.Add(new IsPlayingColumnViewModel());
        TracksColumns.Add(new CheckedColumnViewModel<TrackViewModel>(nameof(TrackViewModel.Picked), t => t.Picked, (t, v) => t.Picked = v));

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Title), t => t.Title ?? string.Empty)
        {
            Caption = Texts.Title,
            Width = 230,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, int?>(nameof(TrackViewModel.Year), t => t.Year)
        {
            Caption = Texts.Year,
            Width = 50,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Duration), t => DurationConverter.Convert(t.Duration))
        {
            Caption = Texts.Duration,
            Width = 50,
        });

        TracksColumns.Add(new RatingColumnViewModel());

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Artist), t => t.Artist ?? string.Empty)
        {
            Caption = Texts.Artist,
            Width = 200,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Genre), t => t.Genre ?? string.Empty)
        {
            Caption = Texts.Genre,
            Width = 100,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, int>(nameof(TrackViewModel.Reproduced), t => t.Reproduced)
        {
            Caption = Texts.Reproduced,
            Width = 100,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Album), t => t.Album ?? string.Empty)
        {
            Caption = Texts.Album,
            Width = 200,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Created), t => t.Created.ToString("g"))
        {
            Caption = Texts.Created,
            Width = 120,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Added), t => t.Added.ToString("g"))
        {
            Caption = Texts.Added,
            Width = 100,
            IsVisible = false,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, int>(nameof(TrackViewModel.Bitrate), t => (int)t.Bitrate)
        {
            Caption = Texts.Bitrate,
            Width = 100,
            IsVisible = false,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, long>(nameof(TrackViewModel.Size), t => t.Size)
        {
            Caption = Texts.Size,
            Width = 100,
            IsVisible = false,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(FileInfo.Extension), t => t.Extension)
        {
            Caption = Texts.Extension,
            Width = 50,
            IsVisible = false,
        });
    }

    protected override async Task DeletePlaylistAsync()
    {
        var selectedPlaylist = SelectedPlaylist;

        if (!await TrackPlay.Player.DeletePlaylistAsync(selectedPlaylist?.Playlist))
            return;

        SelectedPlaylist = null;
        Playlists.Remove(selectedPlaylist);
        await SelectSongsAsync();
    }

    protected override async Task DeleteCategoryAsync()
    {
        var selectedCategory = SelectedCategory;

        if (!await TrackPlay.Player.DeleteCategoryAsync(selectedCategory?.Category))
            return;

        SelectedCategory = null;
        Categories.Remove(selectedCategory);
    }

    public override Task OnNextListAsync() => Task.CompletedTask;

    public override void OnExit()
    {
        _discoveryClient?.Dispose();
        Owner.Close();
    }

}

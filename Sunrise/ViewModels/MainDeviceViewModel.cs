using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sunrise.Model;
using Sunrise.Model.Common;
using Sunrise.Model.Communication;
using Sunrise.Model.Discovery;
using Sunrise.Model.Resources;
using Sunrise.Services;

namespace Sunrise.ViewModels;

public sealed class MainDeviceViewModel : MainViewModel, IDisposable
{
    private const int _recentlyAddedTracksCount = 10;
    private const int _recentlyAddedPlaylistsCount = 10;
    private int _selectedTabIndex;
    private bool _isShortTrackVisible;
    private bool _isTrackVisible;
    private bool _isTrackListVisible;
    private string _backCaption = Texts.Back;
    private string _backPlaylistCaption = Texts.Back;
    private string? _trackSourceCaption;
    private string? _playlistCaption;
    private string? _playlistDescription;
    private bool _isPlaylistCaptionVisible;
    private bool _isPlaylistChanging;
    private string _changingPlaylistText = Texts.Change;
    private bool _isCategoryChanging;
    private string _changingCategoryText = Texts.Change;
    private bool[] _selectedCategories;
    private string _connectPlayerCaption = Texts.Connect;
    private string? _syncIp;
#pragma warning disable CA2213 // Disposable fields should be disposed
    private DiscoveryServer? _discoveryServer;
    private SyncClient? _client;
#pragma warning restore CA2213 // Disposable fields should be disposed

    public MainDeviceViewModel() { } // For designer

    public MainDeviceViewModel(Player player)
        : base(player)
    {
        BackCommand = new AsyncRelayCommand(BackAsync);
        RandomPlayRunCommand = new AsyncRelayCommand(OnRandomPlayRunAsync);
        RecentlyAddedCommand = new AsyncRelayCommand(OnRecentlyAddedAsync);
        RecentlyAddedPlaylistsCommand = new AsyncRelayCommand(OnRecentlyAddedPlaylistsAsync);
        ChangePlaylistCommand = new RelayCommand(OnChangePlaylist);
        ApplyPlaylistCommand = new AsyncRelayCommand(OnApplyPlaylistAsync);
        ApplyCategoryCommand = new AsyncRelayCommand(OnApplyCategoryAsync);
        ChangeCategoryCommand = new RelayCommand(OnChangeCategory);
        ConnectPlayerCommand = new AsyncRelayCommand(OnConnectPlayerAsync);

        if (Network.Exist())
            StartDiscoveryServer();
    }

    private void StartDiscoveryServer()
    {
        string deviceName = AppServices.Get<IAppEnvironment>().MachineName;
        var discoveryServer = _discoveryServer = new DiscoveryServer(deviceName, OnDeviceDetected);
        discoveryServer.Start();
    }

    private void OnDeviceDetected(DiscoveryDeviceInfo deviceInfo)
    {
        if (SettingsDisplayed)
            WriteInfo($"DeviceDetected {deviceInfo.DeviceName} - {deviceInfo.IPAddress}:{deviceInfo.Port}");

        try
        {
            _client?.Dispose();
            _client = null;
            string deviceName = AppServices.Get<IAppEnvironment>().MachineName;
            var client = _client = SyncClient.Create(deviceName, TrackPlay.Player, deviceInfo.IPAddress, deviceInfo.Port, ReloadTracksAsync, OnException);
            client.Connect();

            if (SettingsDisplayed)
                WriteInfo("Connected");
        }
        catch (Exception e)
        {
            WriteInfo(e);
        }
    }

    public new TrackPlayDeviceViewModel TrackPlay => (TrackPlayDeviceViewModel)base.TrackPlay;

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public DeviceTabs SelectedTab
    {
        get => (DeviceTabs)_selectedTabIndex;
        set => SelectedTabIndex = (int)value;
    }

    public bool IsShortTrackVisible
    {
        get => _isShortTrackVisible;
        set => SetProperty(ref _isShortTrackVisible, value);
    }

    public bool IsTrackVisible
    {
        get => _isTrackVisible;
        set => SetProperty(ref _isTrackVisible, value);
    }

    public bool IsTrackListVisible
    {
        get => _isTrackListVisible;
        set => SetProperty(ref _isTrackListVisible, value);
    }

    public string BackCaption
    {
        get => _backCaption;
        set => SetProperty(ref _backCaption, value);
    }

    public string BackPlaylistCaption
    {
        get => _backPlaylistCaption;
        set => SetProperty(ref _backPlaylistCaption, value);
    }

    public string? TrackSourceCaption
    {
        get => _trackSourceCaption;
        set => SetProperty(ref _trackSourceCaption, value);
    }

    public string? PlaylistCaption
    {
        get => _playlistCaption;
        set => SetProperty(ref _playlistCaption, value);
    }

    public string? PlaylistDescription
    {
        get => _playlistDescription;
        set => SetProperty(ref _playlistDescription, value);
    }

    public bool IsPlaylistCaptionVisible
    {
        get => _isPlaylistCaptionVisible;
        set => SetProperty(ref _isPlaylistCaptionVisible, value);
    }

    public ObservableCollection<CategoryViewModel> ChangedCategories { get; } = [];

    public bool IsPlaylistChanging
    {
        get => _isPlaylistChanging;
        set => SetProperty(ref _isPlaylistChanging, value);
    }

    public string ChangingPlaylistText
    {
        get => _changingPlaylistText;
        set => SetProperty(ref _changingPlaylistText, value);
    }

    public IRelayCommand ApplyCategoryCommand { get; }

    public IRelayCommand ChangeCategoryCommand { get; }

    public IRelayCommand ConnectPlayerCommand { get; }

    public string ConnectPlayerCaption
    {
        get => _connectPlayerCaption;
        set => SetProperty(ref _connectPlayerCaption, value);
    }

    public bool IsCategoryChanging
    {
        get => _isCategoryChanging;
        set => SetProperty(ref _isCategoryChanging, value);
    }

    public string ChangingCategoryText
    {
        get => _changingCategoryText;
        set => SetProperty(ref _changingCategoryText, value);
    }

    public CategoryViewModel? SelectedChangedCategory { get; private set; }

    public string? SyncIP
    {
        get => _syncIp;
        set => SetProperty(ref _syncIp, value);
    }

    public IRelayCommand BackCommand { get; }

    public IRelayCommand RandomPlayRunCommand { get; }

    public IRelayCommand RecentlyAddedCommand { get; }

    public IRelayCommand RecentlyAddedPlaylistsCommand { get; }

    public IRelayCommand ChangePlaylistCommand { get; }

    public IRelayCommand ApplyPlaylistCommand { get; }

    public RecentlyAddedRubricViewModel RecentlyAddedRubric { get; private set; }

    public ObservableCollection<TrackViewModel> RecentlyAddedTracks { get; } = [];

    public ObservableCollection<PlaylistRubricViewModel> RecentlyAddedPlaylists { get; } = [];

    public List<RubricViewModel> TrackSourceHistory { get; } = [];

    #region Search

    private bool _isSearchTracksVisible;
    private bool _isSearchArtistsVisible;
    private bool _isSearchAlbumsVisible;
    private bool _isSearchGenresVisible;

    public bool IsSearchTracksVisible
    {
        get => _isSearchTracksVisible;
        set => SetProperty(ref _isSearchTracksVisible, value);
    }

    public ObservableCollection<TrackViewModel> SearchTracks { get; } = [];

    public bool IsSearchArtistsVisible
    {
        get => _isSearchArtistsVisible;
        set => SetProperty(ref _isSearchArtistsVisible, value);
    }

    public ObservableCollection<ArtistViewModel> SearchArtists { get; } = [];

    public bool IsSearchAlbumsVisible
    {
        get => _isSearchAlbumsVisible;
        set => SetProperty(ref _isSearchAlbumsVisible, value);
    }

    public ObservableCollection<AlbumViewModel> SearchAlbums { get; } = [];

    public bool IsSearchGenresVisible
    {
        get => _isSearchGenresVisible;
        set => SetProperty(ref _isSearchGenresVisible, value);
    }

    public ObservableCollection<GenreViewModel> SearchGenres { get; } = [];

    #endregion

    protected override TrackPlayViewModel CreateTrackPlay(Player player) => new TrackPlayDeviceViewModel(this, player);

    protected override bool CanChangeTracks(RubricViewModel tracksOwner)
    {
        if (TrackSourceHistory.Count == 0 || !ReferenceEquals(TrackSourceHistory[^1], tracksOwner))
            return true;

        if (!ReferenceEquals(TrackPlay.OwnerRubric, tracksOwner))
            return true;

        return false;
    }

    protected override async Task SelectTracksAsync(RubricViewModel tracksOwner, bool changeTracks = true, CancellationToken token = default)
    {
        string trackSourceCaption = null;

        if (changeTracks)
            AddTrackSourceHistory(tracksOwner);

        await base.SelectTracksAsync(tracksOwner, changeTracks, token);

        if (tracksOwner is SongsRubricViewModel)
        {
            if (RecentlyAddedRubric is null)
                await FillRecentlyAddedTracksAsync(token);
        }
        else if (tracksOwner is PlaylistRubricViewModel)
        {
        }
        else if (tracksOwner is TrackSourceViewModel trackSourceViewModel)
        {
            IsTrackSourcesVisible = false;
            IsTrackListVisible = true;
            BackCaption = trackSourceViewModel.Rubric.Name;
            trackSourceCaption = trackSourceViewModel.ToString();
        }
        else if (tracksOwner.IsDependent && changeTracks)
        {
            var prevTracksOwner = TrackSourceHistory.Count > 1 ? TrackSourceHistory[^2] : null;

            if (prevTracksOwner is PlaylistRubricViewModel)
            {
                BackPlaylistCaption = tracksOwner.Name;
                PlaylistCaption = null;
                PlaylistDescription = null;
                IsPlaylistsVisible = false;
            }
            else
            {
                IsTrackListVisible = true;
                BackCaption = tracksOwner.Name;
            }
        }

        if (tracksOwner is not PlaylistRubricViewModel)
        {
            TrackSourceCaption = trackSourceCaption;

            if (!tracksOwner.IsDependent)
                IsTrackListVisible = false;
        }
    }

    protected override bool CanAddRubricTracks(RubricViewModel rubricViewModel)
        => rubricViewModel is SongsRubricViewModel || rubricViewModel.IsDependent;

    protected override void ChangePlaylist(Playlist playlist, IReadOnlyList<Track> tracks)
    {
        BackPlaylistCaption = Texts.Playlists;
        PlaylistCaption = playlist.Name;
        PlaylistDescription = string.Format(Texts.SongsFormat, tracks.Count);
        IsPlaylistCaptionVisible = true;
        IsPlaylistsVisible = false;
    }

    private void AddTrackSourceHistory(RubricViewModel tracksOwner)
    {
        if (TrackSourceHistory.Count > 0)
        {
            var lastTrackSource = TrackSourceHistory[^1];

            if (ReferenceEquals(lastTrackSource, tracksOwner))
                return;

            if (lastTrackSource.Type == tracksOwner.Type)
                TrackSourceHistory.RemoveAt(TrackSourceHistory.Count - 1);
        }

        TrackSourceHistory.Add(tracksOwner);
    }

    private async ValueTask FillRecentlyAddedTracksAsync(CancellationToken token)
    {
        var player = TrackPlay.Player;
        RecentlyAddedRubric = new RecentlyAddedRubricViewModel(player);
        RecentlyAddedTracks.Clear();
        var tracks = await RecentlyAddedRubric.GetTracksAsync(token: token);

        foreach (var track in tracks.Take(_recentlyAddedTracksCount))
            RecentlyAddedTracks.Add(GetTrackViewModel(track));
    }

    public override void ChangePlaylists(IEnumerable<Playlist> playlists)
    {
        base.ChangePlaylists(playlists);
        FillRecentlyAddedPlaylists();
    }

    private void FillRecentlyAddedPlaylists()
    {
        RecentlyAddedPlaylists.Clear();

        foreach (var playlistViewModel in Playlists.OrderByDescending(p => p.Playlist.Created).Take(_recentlyAddedPlaylistsCount))
            RecentlyAddedPlaylists.Add(playlistViewModel);
    }

    public void ShowTrackPage()
    {
        IsShortTrackVisible = false;
        IsTrackVisible = true;
    }

    public void HideTrackPage()
    {
        IsShortTrackVisible = true;
        IsTrackVisible = false;
    }

    public bool CanBack()
        => CanGoToPlaylists() || TrackSourceHistory.Count > 1;

    public async Task<bool> BackAsync(CancellationToken token = default)
    {
        if (!CanBack())
            return false;

        if (CanGoToPlaylists())
        {
            GoToPlaylists();
            return true;
        }

        var currentTracksOwner = TrackSourceHistory[^1];
        TrackSourceHistory.RemoveAt(TrackSourceHistory.Count - 1);
        var tracksOwner = TrackSourceHistory.Count > 0 ? TrackSourceHistory[^1] : null;
        TrackPlay.ChangeOwnerRubric((RubricViewModel)null);

        if (currentTracksOwner is PlaylistRubricViewModel playlistRubricViewModel)
        {
            GoToPlaylists();
            PlaylistsView?.ScrollIntoView(playlistRubricViewModel.Playlist);
        }
        else
        {
            if (tracksOwner is not not PlaylistRubricViewModel)
                IsTrackListVisible = TrackSourceHistory.Count > 1;

            await ChangeTracksAsync(tracksOwner, token: token);

            if (currentTracksOwner is TrackSourceViewModel currentTrackSourceViewModel)
                TracksView?.ScrollIntoView(currentTrackSourceViewModel.Name);
        }

        return true;
    }

    private bool CanGoToPlaylists()
        => SelectedTab == DeviceTabs.Playlists && (TrackSourceHistory.Count == 0 || TrackSourceHistory[^1] is not PlaylistRubricViewModel);

    private void GoToPlaylists()
    {
        IsPlaylistCaptionVisible = false;
        IsPlaylistsVisible = true;
    }

    private static IReadOnlyList<Track>? GetCurrentTracks(object tracksOwner)
        => tracksOwner is RubricViewModel rubricViewModel && rubricViewModel.IsDependent ? rubricViewModel.GetCurrentTracks() : null;

    private async Task OnRandomPlayRunAsync(CancellationToken token)
    {
        var tracksOwner = TrackSourceHistory.Count > 0 ? TrackSourceHistory[^1] : null;
        var tracks = GetCurrentTracks(tracksOwner);
        var rubricViewModel = new RandomizeRubricViewModel(TrackPlay.Player, tracks);
        await ChangeTracksAsync(rubricViewModel, changeTracks: tracks is null, token: token);
        var rubricTracks = rubricViewModel.GetCurrentTracks();
        var track = rubricTracks is null || rubricTracks.Count == 0 ? null : rubricTracks[0];

        if (track is null)
            return;

        var trackViewModel = GetTrackViewModel(track);
        TrackPlay.ChangeOwnerRubric(rubricViewModel);
        await TrackPlay.PlayAsync(trackViewModel);

        if (tracksOwner is PlaylistRubricViewModel playlistRubricViewModel)
            BackPlaylistCaption = playlistRubricViewModel.Name;

        if (tracks is null)
            ShowTrackPage();
        else
            IsShortTrackVisible = true;
    }

    private Task OnRecentlyAddedAsync(CancellationToken token)
    {
        var rubricViewModel = RecentlyAddedRubric;
        TrackPlay.ChangeOwnerRubric(rubricViewModel);
        return ChangeTracksAsync(rubricViewModel, token: token);
    }

    private async Task OnRecentlyAddedPlaylistsAsync(CancellationToken token)
    {
        var playlistViewModel = RecentlyAddedPlaylists.FirstOrDefault();

        if (playlistViewModel is null)
            return;

        SelectedPlaylist = playlistViewModel;
        IsPlaylistsVisible = false;

        TrackPlay.ChangeOwnerRubric(playlistViewModel);
        await ChangeTracksAsync(playlistViewModel, token: token);
    }

    protected async override void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        base.OnPropertyChanging(e);

        if (e.PropertyName == nameof(SelectedTabIndex))
        {
            if (SelectedTab == DeviceTabs.Categories)
            {
                int count = Categories.Count;
                bool[] selectedCategories = new bool[count];

                for (int i = 0; i < count; i++)
                    selectedCategories[i] = Categories[i].IsChecked;

                try
                {
                    if (_selectedCategories is null)
                    {
                        if (selectedCategories.Length == 0 || selectedCategories.All(c => !c))
                            return;
                    }
                    else if (_selectedCategories.Length != selectedCategories.Length)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (_selectedCategories[i] != selectedCategories[i])
                                return;
                        }
                    }
                }
                finally
                {
                    _selectedCategories = selectedCategories;
                }

                var playlists = await TrackPlay.Player.GetPlaylistsAsync();
                var filteredPlaylists = FilterPlaylistsByCategories(playlists);
                ChangePlaylists(filteredPlaylists);
            }
        }
    }

    private IReadOnlyCollection<Playlist> FilterPlaylistsByCategories(Dictionary<string, Playlist> playlists)
    {
        if (Categories.Count == 0 || Categories.All(c => !c.IsChecked))
            return playlists.Values;

        var filteredPlaylists = new List<Playlist>(playlists.Count);

        foreach (var playlist in playlists.Values)
        {
            foreach (var category in Categories)
            {
                int categoryId = category.Category.Id;

                if (playlist.Categories.Any(c => c.Id == categoryId))
                {
                    filteredPlaylists.Add(playlist);
                    break;
                }
            }
        }

        return filteredPlaylists;
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedRubrick))
        {
            var rubricViewModel = SelectedRubrick;

            if (rubricViewModel is not null && !ReferenceEquals(TracksOwner, rubricViewModel) && !TrackSourceHistory.Contains(rubricViewModel))
            {
                TrackSourceHistory.Clear();
                await ChangeTracksAsync(rubricViewModel);
            }
        }
        else if (e.PropertyName == nameof(SelectedTabIndex))
        {
            if (SelectedTab == DeviceTabs.Tracks)
            {
                RubricViewModel? tracksOwner = null;

                for (int i = TrackSourceHistory.Count - 1; i >= 0; i--)
                {
                    var rubricViewModel = TrackSourceHistory[i];

                    if (rubricViewModel is not PlaylistRubricViewModel && (rubricViewModel is TrackSourceViewModel || !rubricViewModel.IsDependent))
                    {
                        tracksOwner = rubricViewModel;
                        break;
                    }
                    else
                        TrackSourceHistory.RemoveAt(i);
                }

                await ChangeTracksAsync(tracksOwner);
            }
            else if (SelectedTab == DeviceTabs.Playlists)
            {
                if (TrackPlay.OwnerRubric is PlaylistRubricViewModel rubricViewModel)
                    await ChangeTracksAsync(rubricViewModel);
            }
        }
        else if (e.PropertyName == nameof(SearchText))
            await UpdateSearchResultsAsync();
        else if (e.PropertyName == nameof(IsPlaylistsVisible))
        {
            if (!IsPlaylistsVisible)
                CancelChangePlaylist();
        }
    }

    private async ValueTask UpdateSearchResultsAsync()
    {
        var result = await TrackPlay.Player.SearchAsync(SearchText);
        IsSearchTracksVisible = result.Tracks.Count > 0;
        SearchTracks.Clear();

        foreach (var track in result.Tracks)
            SearchTracks.Add(GetTrackViewModel(track));

        IsSearchArtistsVisible = result.Artists.Count > 0;
        SearchArtists.Clear();

        foreach (var (name, tracksByAlbums) in result.Artists)
            SearchArtists.Add(ArtistViewModel.Create(Artists, name, tracksByAlbums));

        IsSearchAlbumsVisible = result.Albums.Count > 0;
        SearchAlbums.Clear();

        foreach (var (name, artist, tracks) in result.Albums)
            SearchAlbums.Add(new AlbumViewModel(Albums, name, artist, tracks));

        IsSearchGenresVisible = result.Genres.Count > 0;
        SearchGenres.Clear();

        foreach (var (name, tracks) in result.Genres)
            SearchGenres.Add(new GenreViewModel(Genres, name, tracks));
    }

    public async override Task OnNextListAsync(CancellationToken token = default)
    {
        var currentRubric = TrackPlay.CurrentRubric;
        var currentTrackSource = TrackPlay.CurrentTrackSource ?? TrackPlay.CurrentRubric;

        if (currentRubric is null || currentTrackSource is null)
            return;

        var prevTracksOwner = currentTrackSource;

        if (currentRubric is RandomizeRubricViewModel)
            prevTracksOwner = TrackSourceHistory.Count > 0 ? TrackSourceHistory[^1] : null;

        if (prevTracksOwner is PlaylistRubricViewModel)
        {
            SelectedTab = DeviceTabs.Playlists;
            IsPlaylistsVisible = false;
        }
        else
        {
            if (!IsTrackVisible && !IsTrackSourcesVisible && ReferenceEquals(TrackSourceHistory[^1], currentRubric)
                && ReferenceEquals(SelectedRubrick, currentRubric) && ReferenceEquals(SelectedTrackSource, currentTrackSource))
            {
                return;
            }
            else if (currentRubric is SearchRubricViewModel)
            {
                HideTrackPage();
                return;
            }

            SelectedTab = DeviceTabs.Tracks;

            if (!currentRubric.IsDependent)
                SelectedRubrick = currentRubric;
        }

        OnExit();
        TrackPlay.ChangeOwnerRubric(currentRubric, TrackPlay.OwnerTrackSource);
        await SelectTracksAsync(currentTrackSource, token: token);
        SelectedTrack = TrackPlay.CurrentTrack;
    }

    protected override async ValueTask OnRemoveTrackAsync(int trackId, CancellationToken token)
    {
        await FillRecentlyAddedTracksAsync(token);
        await UpdateSearchResultsAsync();
    }

    protected override async ValueTask OnRemovePlaylistAsync(int playlistId, CancellationToken token)
    {
        FillRecentlyAddedPlaylists();
        await UpdateSearchResultsAsync();
    }

    private void OnChangePlaylist()
    {
        IsPlaylistChanging = !IsPlaylistChanging;
        ChangingPlaylistText = IsPlaylistChanging ? Texts.Cancel : Texts.Change;
        var playlist = SelectedPlaylist?.Playlist;

        if (IsPlaylistChanging && playlist is not null)
        {
            foreach (var category in Categories)
            {
                int categoryId = category.Category.Id;
                bool isChecked = playlist.Categories.Any(c => c.Id == categoryId);
                ChangedCategories.Add(new CategoryViewModel(category.Category, isChecked));
            }
        }
    }

    private async Task OnApplyPlaylistAsync(CancellationToken token)
    {
        var currentPlaylist = SelectedPlaylist;

        if (currentPlaylist is not null)
        {
            var playlist = currentPlaylist.Playlist;
            await TrackPlay.Player.ChangePlaylistNameAsync(playlist, PlaylistCaption, token);
            PlaylistCaption = currentPlaylist.Name = playlist.Name;

            foreach (var category in ChangedCategories)
            {
                int categoryId = category.Category.Id;

                if (category.IsChecked)
                {
                    if (!playlist.Categories.Any(c => c.Id == categoryId))
                        await TrackPlay.Player.AddCategoryInPlaylistAsync(playlist, category.Category, token);
                }
                else if (playlist.Categories.Any(c => c.Id == categoryId))
                    await TrackPlay.Player.DeleteCategoryInPlaylistAsync(playlist, category.Category, token);
            }
        }

        CancelChangePlaylist();
    }

    private void CancelChangePlaylist()
    {
        IsPlaylistChanging = false;
        ChangedCategories.Clear();
        ChangingPlaylistText = Texts.Change;
    }

    protected override async Task DeletePlaylistAsync(CancellationToken token)
    {
        var currentPlaylist = SelectedPlaylist;

        if (currentPlaylist is null)
            return;

        TrackPlay.Clear(); // Stop
        GoToPlaylists();
        await TrackPlay.Player.DeletePlaylistAsync(currentPlaylist.Playlist, token);
        await RemoveAsync(currentPlaylist.Playlist, token);
    }

    protected override async Task DeleteCategoryAsync(CancellationToken token)
    {
        var selectedChangedCategory = SelectedChangedCategory;

        if (selectedChangedCategory is not null)
        {
            if (!await TrackPlay.Player.DeleteCategoryAsync(selectedChangedCategory.Category, token))
                return;

            SelectedCategory = SelectedChangedCategory = null;
            Categories.Remove(selectedChangedCategory);
        }
    }

    public async Task OnApplyCategoryAsync(CancellationToken token = default)
    {
        var selectedChangedCategory = SelectedChangedCategory;

        if (selectedChangedCategory is not null)
        {
            selectedChangedCategory.Editing = false;
            await TrackPlay.Player.ChangeCategoryNameAsync(selectedChangedCategory.Category, selectedChangedCategory.Name, token);
            selectedChangedCategory.Name = selectedChangedCategory.Category.Name;
        }

        SelectedChangedCategory = null;
        CancelChangeCategory();
    }

    private void OnChangeCategory()
    {
        var selectedCategory = SelectedCategory;

        if (IsCategoryChanging)
        {
            foreach (var category in Categories)
                category.Editing = false;
        }
        else if (selectedCategory is not null)
        {
            selectedCategory.Editing = true;
            SelectedChangedCategory = selectedCategory;
        }

        IsCategoryChanging = !IsCategoryChanging;
        ChangingCategoryText = IsCategoryChanging ? Texts.Cancel : Texts.Change;
    }

    private void CancelChangeCategory()
    {
        IsCategoryChanging = false;
        ChangingCategoryText = Texts.Change;

        foreach (var category in Categories)
            category.Editing = false;
    }

    private async Task OnConnectPlayerAsync(CancellationToken token)
    {
        bool settingsDisplayed = SettingsDisplayed;
        var client = _client;

        try
        {
            if (client is null)
            {
                Network.ClearCache();

                if (!string.IsNullOrWhiteSpace(_syncIp))
                {
                    ConnectPlayerCaption = Texts.Disconnect;
                    var ipAddress = IPAddress.Parse(_syncIp);
                    var deviceInfo = new DiscoveryDeviceInfo(_syncIp, ipAddress, SyncServiceManager.Port);
                    OnDeviceDetected(deviceInfo);
                }
                else
                {
                    if (!Network.Exist())
                    {
                        await MessageBoxManager.GetMessageBoxStandard(string.Empty, Texts.WiFiDisabled, ButtonEnum.Ok).ShowAsync();
                        return;
                    }

                    if (_discoveryServer is null)
                        StartDiscoveryServer();
                }
            }
            else
            {
                ConnectPlayerCaption = Texts.Connect;
                client.Dispose();
                _client = null;
            }
        }
        catch (Exception e)
        {
            if (settingsDisplayed)
                Info += Environment.NewLine + e.ToString();
        }
    }

    public override void OnExit()
        => HideTrackPage();

    public void Dispose()
    {
        Tasks.StartOnDefaultScheduler(() =>
        {
            _discoveryServer?.Dispose();
            _discoveryServer = null;

            _client?.Dispose();
            _client = null;
        });

        GC.SuppressFinalize(this);
    }

}

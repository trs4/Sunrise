using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sunrise.Model;
using Sunrise.Model.Common;
using Sunrise.Services;
using Sunrise.ViewModels.Interfaces;

namespace Sunrise.ViewModels;

public abstract class MainViewModel : ObservableObject
{
    private RubricViewModel _selectedRubrick;
    private bool _isTrackSourcesVisible;
    private TrackSourceViewModel? _selectedTrackSource;
    private bool _isReadOnlyTracks = true;
    private TrackViewModel? _selectedTrack;
    private readonly Dictionary<int, TrackViewModel> _trackMap = [];
    private readonly Dictionary<int, PlaylistRubricViewModel> _playlistMap = [];
    private string? _searchText;
    private bool _settingsDisplayed;
    private string? _info;

    protected MainViewModel() { } // For designer

    protected MainViewModel(Player player)
    {
        Artists = new ArtistsRubricViewModel(player);
        Albums = new AlbumsRubricViewModel(player);
        Songs = new SongsRubricViewModel(player);
        Genres = new GenresRubricViewModel(player);

        History = new HistoryRubricViewModel(player);

        TrackPlay = CreateTrackPlay(player);

        SettingsCommand = new RelayCommand(OnSettings);
        AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
        DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync);
        AddPlaylistCommand = new AsyncRelayCommand(AddPlaylistAsync);
        DeletePlaylistCommand = new AsyncRelayCommand(DeletePlaylistAsync);

        Rubricks = new([Artists, Albums, Songs, Genres]);
        _selectedRubrick = Songs;
    }

    public TrackPlayViewModel TrackPlay { get; }

    public ArtistsRubricViewModel Artists { get; }

    public AlbumsRubricViewModel Albums { get; }

    public SongsRubricViewModel Songs { get; }

    public GenresRubricViewModel Genres { get; }

    public HistoryRubricViewModel History { get; }

    public ObservableCollection<RubricViewModel> Rubricks { get; }

    public RubricViewModel SelectedRubrick
    {
        get => _selectedRubrick;
        set => SetProperty(ref _selectedRubrick, value);
    }

    #region Playlists

    private PlaylistRubricViewModel? _selectedPlaylist;
    private bool _isPlaylistsVisible = true;

    public ObservableCollection<PlaylistRubricViewModel> Playlists { get; } = [];

    public PlaylistRubricViewModel? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set => SetProperty(ref _selectedPlaylist, value);
    }

    public IRelayCommand AddPlaylistCommand { get; }

    public IRelayCommand DeletePlaylistCommand { get; }

    public bool IsPlaylistsVisible
    {
        get => _isPlaylistsVisible;
        set => SetProperty(ref _isPlaylistsVisible, value);
    }

    #endregion
    #region Categories

    private CategoryViewModel? _selectedCategory;
    private bool _isCategoriesVisible = true;

    public ObservableCollection<CategoryViewModel> Categories { get; } = [];

    public CategoryViewModel? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public IRelayCommand SettingsCommand { get; }

    public IRelayCommand AddCategoryCommand { get; }

    public IRelayCommand DeleteCategoryCommand { get; }

    public bool IsCategoriesVisible
    {
        get => _isCategoriesVisible;
        set => SetProperty(ref _isCategoriesVisible, value);
    }

    #endregion

    public RubricViewModel TracksOwner { get; set; }

    public ObservableCollection<TrackSourceViewModel> TrackSources { get; } = [];

    public TrackSourceViewModel? SelectedTrackSource
    {
        get => _selectedTrackSource;
        set => SetProperty(ref _selectedTrackSource, value);
    }

    public bool IsTrackSourcesVisible
    {
        get => _isTrackSourcesVisible;
        set => SetProperty(ref _isTrackSourcesVisible, value);
    }

    public ObservableCollection<TrackViewModel> Tracks { get; } = [];

    public TrackViewModel? SelectedTrack
    {
        get => _selectedTrack;
        set => SetProperty(ref _selectedTrack, value);
    }

    public bool IsReadOnlyTracks
    {
        get => _isReadOnlyTracks;
        set => SetProperty(ref _isReadOnlyTracks, value);
    }

    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public ITracksView TracksView { get; set; }

    public IPlaylistsView PlaylistsView { get; set; }

    public bool SettingsDisplayed
    {
        get => _settingsDisplayed;
        set => SetProperty(ref _settingsDisplayed, value);
    }

    public string? Info
    {
        get => _info;
        set => SetProperty(ref _info, value);
    }

    public void WriteInfo(string message)
    {
        InitInfo();
        Info += Environment.NewLine + message;
    }

    public void WriteInfo(Exception exception) => WriteInfo(ExceptionHandler.GetString(exception));

    protected abstract TrackPlayViewModel CreateTrackPlay(Player player);

    public async Task ReloadTracksAsync(CancellationToken token = default)
    {
        _trackMap.Clear();
        _playlistMap.Clear();
        var rubricViewModel = Songs;
        var playlists = await TrackPlay.Player.GetPlaylistsAsync(token);
        var categoriesScreenshot = await TrackPlay.Player.GetCategoriesAsync(token);
        await SelectTracksAsync(rubricViewModel, token: token);
        ChangePlaylists(playlists.Values);
        ChangeCategories(categoriesScreenshot.Categories);
    }

    protected async Task OnException(Exception exception)
    {
        string message = ExceptionHandler.GetString(exception);
        await MessageBoxManager.GetMessageBoxStandard(GetType().Name, message, ButtonEnum.Ok).ShowAsync();
    }

    public Task SelectSongsAsync(CancellationToken token = default)
        => ChangeTracksAsync(Songs, token: token);

    public Task ChangeTracksAsync(RubricViewModel tracksOwner, bool changeTracks = true, CancellationToken token = default)
    {
        if (!CanChangeTracks(tracksOwner))
            return Task.CompletedTask;

        return SelectTracksAsync(tracksOwner, changeTracks, token);
    }

    protected virtual bool CanChangeTracks(RubricViewModel tracksOwner) => !Equals(TracksOwner, tracksOwner);

    protected virtual async Task SelectTracksAsync(RubricViewModel tracksOwner, bool changeTracks = true, CancellationToken token = default)
    {
        TracksOwner = tracksOwner;
        IReadOnlyList<Track> tracks;

        if (tracksOwner is TrackSourceViewModel trackSourceViewModel)
            tracks = await trackSourceViewModel.Rubric.GetTracksAsync(trackSourceViewModel, token);
        else
        {
            var selectedTrackSource = SelectedTrackSource;
            var tracksScreenshot = await TrackPlay.Player.GetTracksAsync(token);
            var trackSources = tracksOwner.GetTrackSources(tracksScreenshot);
            IsTrackSourcesVisible = trackSources is not null;
            TrackSources.Clear();

            if (trackSources is not null)
            {
                foreach (var trackSource in trackSources)
                    TrackSources.Add(trackSource);

                if (selectedTrackSource is null)
                    SelectedTrackSource = selectedTrackSource = trackSources.Count > 0 ? trackSources[0] : null;
            }

            SelectedTrackSource = tracksOwner is SongsRubricViewModel ? null : selectedTrackSource;

            if (tracksOwner is PlaylistRubricViewModel playlistRubricViewModel)
            {
                tracks = await playlistRubricViewModel.Playlist.GetTracksAsync(TrackPlay.Player, token);
                ChangePlaylist(playlistRubricViewModel.Playlist, tracks);
            }
            else
                tracks = CanAddRubricTracks(tracksOwner) ? await tracksOwner.GetTracksAsync(selectedTrackSource, token) : [];
        }

        if (changeTracks)
        {
            Tracks.Clear();

            foreach (var track in tracks)
                Tracks.Add(GetTrackViewModel(track));
        }
    }

    protected virtual bool CanAddRubricTracks(RubricViewModel rubricViewModel) => true;

    protected virtual void ChangePlaylist(Playlist playlist, IReadOnlyList<Track> tracks) { }

    protected TrackViewModel GetTrackViewModel(Track track)
    {
        if (_trackMap.TryGetValue(track.Id, out var trackViewModel))
            return trackViewModel;

        trackViewModel = new TrackViewModel(track, TrackPlay.Player);
        _trackMap.Add(track.Id, trackViewModel);
        return trackViewModel;
    }

    public TrackViewModel? GetTrackViewModelWithCheck(Track? track)
        => track is null ? null : GetTrackViewModel(track);

    public PlaylistRubricViewModel GetPlaylistViewModel(Playlist playlist)
    {
        if (_playlistMap.TryGetValue(playlist.Id, out var playlistViewModel))
            return playlistViewModel;

        playlistViewModel = new PlaylistRubricViewModel(TrackPlay.Player, playlist);
        _playlistMap.Add(playlist.Id, playlistViewModel);
        return playlistViewModel;
    }

    public virtual void ChangePlaylists(IEnumerable<Playlist> playlists)
    {
        Playlists.Clear();

        foreach (var playlist in playlists.OrderBy(p => p.Name, NaturalSortComparer.Instance))
        {
            var playlistViewModel = GetPlaylistViewModel(playlist);
            Playlists.Add(playlistViewModel);
        }
    }

    public void ChangeCategories(IEnumerable<Category> categories)
    {
        Categories.Clear();

        foreach (var category in categories.OrderBy(p => p.Name, NaturalSortComparer.Instance))
        {
            var categoryViewModel = new CategoryViewModel(category);
            Categories.Add(categoryViewModel);
        }
    }

    private async Task AddCategoryAsync(CancellationToken token)
    {
        var category = await TrackPlay.Player.AddCategoryAsync(token);
        var categoryViewModel = new CategoryViewModel(category);
        Categories.Insert(0, categoryViewModel);
        SelectedCategory = categoryViewModel;
    }

    protected abstract Task DeleteCategoryAsync(CancellationToken token);

    private Task AddPlaylistAsync(CancellationToken token)
        => AddPlaylistAsync(null, token);

    public async Task AddPlaylistAsync(PlaylistCalculatedData? calculatedData, CancellationToken token)
    {
        var playlist = await TrackPlay.Player.AddPlaylistAsync(calculatedData, token);
        var playlistViewModel = GetPlaylistViewModel(playlist);
        Playlists.Insert(0, playlistViewModel);
        SelectedPlaylist = playlistViewModel;

        TrackPlay.ChangeOwnerRubric(playlistViewModel);
        await ChangeTracksAsync(playlistViewModel, token: token);
    }

    protected abstract Task DeletePlaylistAsync(CancellationToken token);

    public Track[] CreateRandomizeTracks()
    {
        int count = Tracks.Count;
        var randomizeTracks = new Track[count];

        for (int i = 0; i < count; i++)
            randomizeTracks[i] = Tracks[i].Track;

        RandomNumberGenerator.Shuffle(randomizeTracks.AsSpan());
        return randomizeTracks;
    }

    public async ValueTask RemoveAsync(Track? track, CancellationToken token = default)
    {
        if (track is null)
            return;

        int trackId = track.Id;
        _trackMap.Remove(trackId);

        for (int i = Tracks.Count - 1; i >= 0; i--)
        {
            if (Tracks[i].Track.Id == trackId)
                Tracks.RemoveAt(i);
        }

        await OnRemoveTrackAsync(trackId, token);
    }

    protected virtual ValueTask OnRemoveTrackAsync(int trackId, CancellationToken token) => default;

    public async ValueTask RemoveAsync(Playlist? playlist, CancellationToken token = default)
    {
        if (playlist is null)
            return;

        int playlistId = playlist.Id;
        _playlistMap.Remove(playlistId);

        for (int i = Playlists.Count - 1; i >= 0; i--)
        {
            if (Playlists[i].Playlist.Id == playlistId)
                Playlists.RemoveAt(i);
        }

        await OnRemovePlaylistAsync(playlistId, token);
    }

    protected virtual ValueTask OnRemovePlaylistAsync(int playlistId, CancellationToken token) => default;

    public abstract Task OnNextListAsync(CancellationToken token);

    private void OnSettings()
    {
        InitInfo();
        SettingsDisplayed = !SettingsDisplayed;
    }

    private void InitInfo()
        => Info ??= BuildInfo();

    private static string BuildInfo()
        => AppServices.Get<IAppEnvironment>().MachineName + Environment.NewLine
        + Environments.GetPlatformName() + Environment.NewLine
        + Network.GetMachineIPAddress() + Environment.NewLine + Environment.NewLine
        + string.Join(Environment.NewLine, Network.GetAvailableIPAddresses()
            .Select(p => $"{p.IPAddress} {p.NetworkInterface.Name} {p.NetworkInterface.Description}"));

    public abstract void OnExit();
}

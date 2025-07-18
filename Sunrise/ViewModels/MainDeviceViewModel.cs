﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Albums;
using Sunrise.ViewModels.Artists;
using Sunrise.ViewModels.Genres;

namespace Sunrise.ViewModels;

public sealed class MainDeviceViewModel : MainViewModel
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

    public MainDeviceViewModel() { } // For designer

    public MainDeviceViewModel(Player player)
        : base(player)
    {
        BackCommand = new AsyncRelayCommand(OnBackAsync);
        RandomPlayRunCommand = new AsyncRelayCommand(OnRandomPlayRunAsync);
        RecentlyAddedCommand = new AsyncRelayCommand(OnRecentlyAddedAsync);
        RecentlyAddedPlaylistsCommand = new AsyncRelayCommand(OnRecentlyAddedPlaylistsAsync);
        ChangePlaylistCommand = new RelayCommand(OnChangePlaylist);
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

    public IRelayCommand BackCommand { get; }

    public IRelayCommand RandomPlayRunCommand { get; }

    public IRelayCommand RecentlyAddedCommand { get; }

    public IRelayCommand RecentlyAddedPlaylistsCommand { get; }

    public IRelayCommand ChangePlaylistCommand { get; }

    public RecentlyAddedRubricViewModel RecentlyAddedRubric { get; private set; }

    public ObservableCollection<TrackViewModel> RecentlyAddedTracks { get; } = [];

    public ObservableCollection<PlaylistViewModel> RecentlyAddedPlaylists { get; } = [];

    public List<object> TrackSourceHistory { get; } = [];

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

    protected override async Task SelectTracksAsync(object tracksOwner, bool changeTracks = true, CancellationToken token = default)
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
        else if (tracksOwner is RubricViewModel rubricViewModel && rubricViewModel.IsDependent && changeTracks)
        {
            object prevTracksOwner = TrackSourceHistory.Count > 1 ? TrackSourceHistory[^2] : null;

            if (prevTracksOwner is PlaylistViewModel or PlaylistRubricViewModel)
            {
                BackPlaylistCaption = rubricViewModel.Name;
                PlaylistCaption = null;
                PlaylistDescription = null;
                IsPlaylistsVisible = false;
            }
            else
            {
                IsTrackListVisible = true;
                BackCaption = rubricViewModel.Name;
            }
        }

        if (tracksOwner is not PlaylistViewModel and not PlaylistRubricViewModel)
            TrackSourceCaption = trackSourceCaption;

        if (changeTracks)
            IsPlaylistCaptionVisible = tracksOwner is PlaylistViewModel or PlaylistRubricViewModel;
    }

    protected override bool CanAddRubricTracks(RubricViewModel rubricViewModel)
        => rubricViewModel is SongsRubricViewModel || rubricViewModel.IsDependent;

    protected override void ChangePlaylist(Playlist playlist)
    {
        BackPlaylistCaption = Texts.Playlists;
        PlaylistCaption = playlist.Name;
        PlaylistDescription = string.Format(Texts.SongsFormat, playlist.Tracks.Count);
        IsPlaylistsVisible = false;
    }

    private void AddTrackSourceHistory(object tracksOwner)
    {
        if (TrackSourceHistory.Count > 0 && ReferenceEquals(TrackSourceHistory[^1], tracksOwner))
            return;

        TrackSourceHistory.Add(tracksOwner);
    }

    private async ValueTask FillRecentlyAddedTracksAsync(CancellationToken token)
    {
        var player = TrackPlay.Player;
        var screenshot = await player.GetAllTracksAsync(token);
        RecentlyAddedRubric = new RecentlyAddedRubricViewModel(player);
        RecentlyAddedTracks.Clear();

        foreach (var track in RecentlyAddedRubric.GetTracks(screenshot).Take(_recentlyAddedTracksCount))
            RecentlyAddedTracks.Add(GetTrackViewModel(track));
    }

    public override void ChangePlaylists(IEnumerable<Playlist> playlists)
    {
        base.ChangePlaylists(playlists);

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

    private Task OnBackAsync()
    {
        if (TrackSourceHistory.Count == 0)
            return Task.CompletedTask;

        object currentTracksOwner = TrackSourceHistory[^1];
        TrackSourceHistory.RemoveAt(TrackSourceHistory.Count - 1);
        object tracksOwner = TrackSourceHistory.Count > 0 ? TrackSourceHistory[^1] : null;

        if (currentTracksOwner is PlaylistViewModel or PlaylistRubricViewModel)
        {
            IsPlaylistCaptionVisible = false;
            IsPlaylistsVisible = true;
            return Task.CompletedTask;
        }
        else
        {
            if (tracksOwner is not PlaylistViewModel and not PlaylistRubricViewModel)
                IsTrackListVisible = TrackSourceHistory.Count > 1;

            return ChangeTracksAsync(tracksOwner);
        }
    }

    private static IReadOnlyList<Track>? GetCurrentTracks(object tracksOwner)
    {
        if (tracksOwner is PlaylistViewModel playlistViewModel)
            return playlistViewModel.Playlist.Tracks;
        else if (tracksOwner is RubricViewModel rubricViewModel && rubricViewModel.IsDependent)
            return rubricViewModel.GetCurrentTracks();

        return null;
    }

    private async Task OnRandomPlayRunAsync()
    {
        object tracksOwner = TrackSourceHistory.Count > 0 ? TrackSourceHistory[^1] : null;
        var tracks = GetCurrentTracks(tracksOwner);
        var rubricViewModel = new RandomizeRubricViewModel(TrackPlay.Player, tracks);
        await ChangeTracksAsync(rubricViewModel, changeTracks: tracks is null);
        var rubricTracks = rubricViewModel.GetCurrentTracks();
        var track = rubricTracks is null || rubricTracks.Count == 0 ? null : rubricTracks[0];

        if (track is null)
            return;

        var trackViewModel = GetTrackViewModel(track);
        TrackPlay.ChangeOwnerRubric(rubricViewModel);
        await TrackPlay.PlayAsync(trackViewModel);

        if (tracksOwner is PlaylistViewModel playlistViewModel)
            BackPlaylistCaption = playlistViewModel.Name;

        if (tracks is null)
            ShowTrackPage();
        else
            IsShortTrackVisible = true;
    }

    private Task OnRecentlyAddedAsync()
    {
        var rubricViewModel = RecentlyAddedRubric;
        TrackPlay.ChangeOwnerRubric(rubricViewModel);
        return ChangeTracksAsync(rubricViewModel);
    }

    private async Task OnRecentlyAddedPlaylistsAsync()
    {
        var playlistViewModel = RecentlyAddedPlaylists.FirstOrDefault();

        if (playlistViewModel is null)
            return;

        SelectedPlaylist = playlistViewModel;
        IsPlaylistsVisible = false;
        await ChangeTracksAsync(playlistViewModel);
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
            if (SelectedTab == DeviceTabs.Playlists && TrackPlay.OwnerRubric is PlaylistRubricViewModel rubricViewModel)
                await ChangeTracksAsync(rubricViewModel);
        }
        else if (e.PropertyName == nameof(SearchText))
            await UpdateSearchResultsAsync();
        else if (e.PropertyName == nameof(IsTrackVisible))
        {
            if (!_isTrackVisible)
                TrackPlay.CancelChangeTrack();
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

    public async override Task OnNextListAsync()
    {
        var ownerRubric = TrackPlay.OwnerRubric;
        object tracksOwner = (object)TrackPlay.OwnerTrackSource ?? TrackPlay.OwnerRubric;

        if (ownerRubric is null || tracksOwner is null)
            return;

        object prevTracksOwner = tracksOwner;

        if (ownerRubric is RandomizeRubricViewModel)
            prevTracksOwner = TrackSourceHistory.Count > 0 ? TrackSourceHistory[^1] : null;

        if (prevTracksOwner is PlaylistViewModel or PlaylistRubricViewModel)
        {
            SelectedTab = DeviceTabs.Playlists;
            IsPlaylistsVisible = false;
        }
        else
        {
            if (!IsTrackVisible && ReferenceEquals(SelectedRubrick, ownerRubric))
                return;
            else if (ownerRubric is SearchRubricViewModel)
            {
                HideTrackPage();
                return;
            }

            SelectedTab = DeviceTabs.Tracks;

            if (!ownerRubric.IsDependent)
                SelectedRubrick = ownerRubric;
        }

        OnExit();
        TrackPlay.ChangeOwnerRubric(ownerRubric, TrackPlay.OwnerTrackSource);
        await SelectTracksAsync(tracksOwner);
        SelectedTrack = TrackPlay.CurrentTrack;
    }

    protected override async ValueTask OnRemoveAsync(int trackId, CancellationToken token)
    {
        await FillRecentlyAddedTracksAsync(token);
        await UpdateSearchResultsAsync();
    }

    private void OnChangePlaylist()
    {
        // %%TODO
    }

    public override void OnExit() => HideTrackPage();
}

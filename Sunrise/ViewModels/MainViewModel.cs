﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Utils;
using Sunrise.ViewModels.Categories;
using Sunrise.ViewModels.Playlists;

namespace Sunrise.ViewModels;

public abstract class MainViewModel : ObservableObject
{
    private RubricViewModel _selectedRubrick;
    private bool _isTrackSourcesVisible;
    private TrackSourceViewModel? _selectedTrackSource;
    private bool _isReadOnlyTracks = true;
    private readonly Dictionary<int, TrackViewModel> _trackMap = [];

    protected MainViewModel() { } // For designer

    protected MainViewModel(Player player)
    {
        Artists = new ArtistsRubricViewModel(player);
        Albums = new AlbumsRubricViewModel(player);
        Songs = new SongsRubricViewModel(player);
        Genres = new GenresRubricViewModel(player);

        TrackPlay = new TrackPlayViewModel(this, player);

        AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
        DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync);
        AddPlaylistCommand = new AsyncRelayCommand(AddPlaylistAsync);
        DeletePlaylistCommand = new AsyncRelayCommand(DeletePlaylistAsync);

        Rubricks = new([Artists, Albums, Songs, Genres]);
        _selectedRubrick = Songs;
    }

    public TrackPlayViewModel TrackPlay { get; }

    public RubricViewModel Artists { get; }

    public RubricViewModel Albums { get; }

    public RubricViewModel Songs { get; }

    public RubricViewModel Genres { get; }

    public ObservableCollection<RubricViewModel> Rubricks { get; }

    public RubricViewModel SelectedRubrick
    {
        get => _selectedRubrick;
        set => SetProperty(ref _selectedRubrick, value);
    }

    #region Playlists

    private bool _isPlaylistsVisible = true;

    public ObservableCollection<PlaylistViewModel> Playlists { get; } = [];

    public PlaylistViewModel? SelectedPlaylist { get; set; }

    public IRelayCommand AddPlaylistCommand { get; }

    public IRelayCommand DeletePlaylistCommand { get; }

    public bool IsPlaylistsVisible
    {
        get => _isPlaylistsVisible;
        set => SetProperty(ref _isPlaylistsVisible, value);
    }

    #endregion
    #region Categories

    private bool _isCategoriesVisible = true;

    public ObservableCollection<CategoryViewModel> Categories { get; } = [];

    public CategoryViewModel? SelectedCategory { get; set; }

    public IRelayCommand AddCategoryCommand { get; }

    public IRelayCommand DeleteCategoryCommand { get; }

    public bool IsCategoriesVisible
    {
        get => _isCategoriesVisible;
        set => SetProperty(ref _isCategoriesVisible, value);
    }

    #endregion

    public object TracksOwner { get; set; }

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

    public bool IsReadOnlyTracks
    {
        get => _isReadOnlyTracks;
        set => SetProperty(ref _isReadOnlyTracks, value);
    }

    public async Task ReloadTracksAsync(CancellationToken token = default)
    {
        _trackMap.Clear();
        var rubricViewModel = Songs;
        var playlists = await TrackPlay.Player.GetAllPlaylistsAsync(token);
        await ChangeTracksCoreAsync(rubricViewModel, token);
        ChangePlaylists(playlists.Values);
    }

    public Task SelectSongsAsync(CancellationToken token = default)
        => ChangeTracksAsync(Songs, token);

    public Task ChangeTracksAsync(object tracksOwner, CancellationToken token = default)
    {
        if (Equals(TracksOwner, tracksOwner))
            return Task.CompletedTask;

        return ChangeTracksCoreAsync(tracksOwner, token);
    }

    protected virtual async Task ChangeTracksCoreAsync(object tracksOwner, CancellationToken token)
    {
        TracksOwner = tracksOwner;
        IEnumerable<Track> tracks;

        if (tracksOwner is Playlist playlist)
        {
            tracks = playlist.Tracks;
            IsTrackSourcesVisible = false;
            TrackSources.Clear();
        }
        else if (tracksOwner is RubricViewModel rubricViewModel)
        {
            var screenshot = await TrackPlay.Player.GetAllTracksAsync(token);
            var trackSources = rubricViewModel.GetTrackSources(screenshot);
            IsTrackSourcesVisible = trackSources is not null;
            TrackSources.Clear();
            TrackSourceViewModel firstTrackSource = null;

            if (trackSources is not null)
            {
                foreach (var trackSource in trackSources)
                    TrackSources.Add(trackSource);

                SelectedTrackSource = firstTrackSource = trackSources.Count > 0 ? trackSources[0] : null;
            }

            tracks = rubricViewModel.GetTracks(screenshot, firstTrackSource);
        }
        else if (tracksOwner is TrackSourceViewModel trackSourceViewModel)
        {
            var screenshot = await TrackPlay.Player.GetAllTracksAsync(token);
            tracks = trackSourceViewModel.Rubric.GetTracks(screenshot, trackSourceViewModel);
        }
        else
        {
            tracks = [];
            IsTrackSourcesVisible = false;
            TrackSources.Clear();
        }

        Tracks.Clear();

        foreach (var track in tracks)
            Tracks.Add(GetTrackViewModel(track));
    }

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

    public void ChangePlaylists(IEnumerable<Playlist> playlists)
    {
        Playlists.Clear();

        foreach (var playlist in playlists.OrderBy(p => p.Name, NaturalStringComparer.Instance))
        {
            var playlistViewModel = new PlaylistViewModel(playlist);
            Playlists.Add(playlistViewModel);
        }
    }

    private async Task AddCategoryAsync()
    {
        var category = await TrackPlay.Player.AddCategoryAsync();
        var categoryViewModel = new CategoryViewModel(category);
        Categories.Add(categoryViewModel);
    }

    private async Task DeleteCategoryAsync()
    {
        var selectedCategory = SelectedCategory;

        if (!await TrackPlay.Player.DeleteCategoryAsync(selectedCategory?.Category))
            return;

        SelectedCategory = null;
        Categories.Remove(selectedCategory);
    }

    private async Task AddPlaylistAsync()
    {
        var playlist = await TrackPlay.Player.AddPlaylistAsync();
        var playlistViewModel = new PlaylistViewModel(playlist);
        Playlists.Add(playlistViewModel);
    }

    private async Task DeletePlaylistAsync()
    {
        var selectedPlaylist = SelectedPlaylist;

        if (!await TrackPlay.Player.DeletePlaylistAsync(selectedPlaylist?.Playlist))
            return;

        SelectedPlaylist = null;
        Playlists.Remove(selectedPlaylist);
        await SelectSongsAsync();
    }

    public Track[] CreateRandomizeTracks()
    {
        int count = Tracks.Count;
        var randomizeTracks = new Track[count];

        for (int i = 0; i < count; i++)
            randomizeTracks[i] = Tracks[i].Track;

        RandomNumberGenerator.Shuffle(randomizeTracks.AsSpan());
        return randomizeTracks;
    }

    public abstract void OnExit();
}

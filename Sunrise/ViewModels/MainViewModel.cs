﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Converters;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;
using Sunrise.ViewModels.Categories;
using Sunrise.ViewModels.Columns;
using Sunrise.ViewModels.Playlists;

namespace Sunrise.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly RubricViewModel _artists;
    private readonly RubricViewModel _albums;
    private readonly RubricViewModel _songs;
    private readonly RubricViewModel _genres;

    private RubricViewModel _selectedRubrick;
    private bool _isTrackSourcesVisible;
    private TrackSourceViewModel? _selectedTrackSource;
    private bool _isReadOnlyTracks = true;

    public MainViewModel() { } // For designer

    public MainViewModel(Player player)
    {
        TrackPlay = new TrackPlayViewModel(this, player);

        _artists = new ArtistsRubricViewModel(player);
        _albums = new AlbumsRubricViewModel(player);
        _songs = new SongsRubricViewModel(player);
        _genres = new GenresRubricViewModel(player);

        AddFolderCommand = new AsyncRelayCommand(AddFolderAsync);
        AddCategoryCommand = new RelayCommand(AddCategory);
        AddPlaylistCommand = new RelayCommand(AddPlaylist);
        DeletePlaylistCommand = new AsyncRelayCommand(DeletePlaylist);
        DoubleClickCommand = new RelayCommand<TrackViewModel>(OnDoubleClick);

        Rubricks = new([_artists, _albums, _songs, _genres]);
        _selectedRubrick = _songs;
        InitTracksColumns();
    }

    public Window Owner { get; internal set; }

    public TrackPlayViewModel TrackPlay { get; }

    public IRelayCommand AddFolderCommand { get; }

    public ObservableCollection<RubricViewModel> Rubricks { get; }

    public RubricViewModel SelectedRubrick
    {
        get => _selectedRubrick;
        set => SetProperty(ref _selectedRubrick, value);
    }

    public IRelayCommand AddCategoryCommand { get; }

    public ObservableCollection<CategoryViewModel> Categories { get; } = [];

    public IRelayCommand AddPlaylistCommand { get; }

    public ObservableCollection<PlaylistViewModel> Playlists { get; } = [];

    public PlaylistViewModel? SelectedPlaylist { get; set; }

    public IRelayCommand DeletePlaylistCommand { get; }

    public IRelayCommand DoubleClickCommand { get; }

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

    public ObservableCollection<ColumnViewModel> TracksColumns { get; } = [];

    public async Task ReloadTracksAsync(CancellationToken token = default)
    {
        var rubricViewModel = _songs;
        var playlists = await TrackPlay.Player.GetAllPlaylists(token);
        await ChangeTracksAsync(rubricViewModel, token);
        ChangePlaylists(playlists.Values);
    }

    public Task SelectSongsAsync(CancellationToken token = default)
        => ChangeTracksAsync(_songs, token);

    public async Task ChangeTracksAsync(object tracksOwner, CancellationToken token = default)
    {
        if (Equals(TracksOwner, tracksOwner))
            return;

        TracksOwner = tracksOwner;
        List<Track> tracks;

        var pickedColumn = TracksColumns.First(c => c.Name == nameof(TrackViewModel.Picked));
        pickedColumn.IsVisible = tracksOwner is SongsRubricViewModel;

        if (tracksOwner is Playlist playlist)
        {
            tracks = playlist.Tracks;
            IsTrackSourcesVisible = false;
            TrackSources.Clear();
        }
        else if (tracksOwner is RubricViewModel rubricViewModel)
        {
            var screenshot = await TrackPlay.Player.GetAllTracks(token);
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
            var screenshot = await TrackPlay.Player.GetAllTracks(token);
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
        {
            var trackViewModel = new TrackViewModel(track, TrackPlay.Player);
            Tracks.Add(trackViewModel);
        }
    }

    public void ChangePlaylists(IEnumerable<Playlist> playlists)
    {
        Playlists.Clear();

        foreach (var playlist in playlists.OrderBy(p => p.Name, NaturalStringComparer.Instance))
        {
            var playlistViewModel = new PlaylistViewModel(playlist);
            Playlists.Add(playlistViewModel);
        }
    }

    private async Task AddFolderAsync(CancellationToken token)
    {
        await MediaFoldersViewModel.ShowAsync(Owner, TrackPlay.Player, token);

        if (TrackPlay.Player.IsAllTracksLoaded())
            return;

        await ReloadTracksAsync(token);
    }

    private void AddCategory()
    {

    }

    private void AddPlaylist()
    {

    }

    private async Task DeletePlaylist()
    {
        var selectedPlaylist = SelectedPlaylist;

        if (!await TrackPlay.Player.DeletePlaylist(selectedPlaylist?.Playlist))
            return;

        SelectedPlaylist = null;
        Playlists.Remove(selectedPlaylist);
        await SelectSongsAsync();
    }

    private void OnDoubleClick(TrackViewModel? trackViewModel)
    {
        if (trackViewModel is not null)
            TrackPlay.PlayItBegin(trackViewModel);
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

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, byte>(nameof(TrackViewModel.Rating), t => t.Rating)
        {
            Caption = Texts.Rating,
            Width = 80,
        });

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
    }

}

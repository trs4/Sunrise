using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Utils;
using Sunrise.ViewModels.Categories;
using Sunrise.ViewModels.Playlists;

namespace Sunrise.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly RubricViewModel _artists;
    private readonly RubricViewModel _albums;
    private readonly RubricViewModel _songs;
    private readonly RubricViewModel _genres;

    private RubricViewModel _selectedRubrick;
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

    public ObservableCollection<TrackViewModel> Tracks { get; } = [];

    public bool IsReadOnlyTracks
    {
        get => _isReadOnlyTracks;
        set => SetProperty(ref _isReadOnlyTracks, value);
    }

    public async Task ReloadTracksAsync(CancellationToken token = default)
    {
        var rubricViewModel = _songs;
        var tracks = await rubricViewModel.GetTracks(token);
        var playlists = await TrackPlay.Player.GetAllPlaylists(token);
        ChangeTracks(rubricViewModel, tracks);
        ChangePlaylists(playlists.Values);
    }

    public async Task SelectSongsAsync(CancellationToken token = default)
    {
        var rubricViewModel = _songs;
        var tracks = await rubricViewModel.GetTracks(token);
        ChangeTracks(rubricViewModel, tracks);
    }

    public void ChangeTracks(object tracksOwner, IEnumerable<Track> tracks)
    {
        if (Equals(TracksOwner, tracksOwner))
            return;

        TracksOwner = tracksOwner;
        Tracks.Clear();

        foreach (var track in tracks)
        {
            var trackViewModel = new TrackViewModel(track);
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
            TrackPlay.Play(trackViewModel);
    }

}

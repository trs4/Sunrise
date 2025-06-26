using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Model.Resources;

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

    public MainDeviceViewModel() { } // For designer

    public MainDeviceViewModel(Player player)
        : base(player)
    {
        BackCommand = new AsyncRelayCommand(OnBackAsync);
        RandomPlayRunCommand = new AsyncRelayCommand(OnRandomPlayRunAsync);
        RecentlyAddedCommand = new AsyncRelayCommand(OnRecentlyAddedAsync);
        RecentlyAddedPlaylistsCommand = new AsyncRelayCommand(OnRecentlyAddedPlaylistsAsync);
    }

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

    public IRelayCommand BackCommand { get; }

    public IRelayCommand RandomPlayRunCommand { get; }

    public IRelayCommand RecentlyAddedCommand { get; }

    public IRelayCommand RecentlyAddedPlaylistsCommand { get; }

    public RecentlyAddedRubricViewModel RecentlyAddedRubric { get; private set; }

    public ObservableCollection<TrackViewModel> RecentlyAddedTracks { get; } = [];

    public ObservableCollection<PlaylistViewModel> RecentlyAddedPlaylists { get; } = [];

    public List<object> TrackSourceHistory { get; } = [];

    protected override async Task SelectTracksAsync(object tracksOwner, CancellationToken token = default)
    {
        string trackSourceCaption = null;
        AddTrackSourceHistory(tracksOwner);
        await base.SelectTracksAsync(tracksOwner, token);

        if (tracksOwner is SongsRubricViewModel)
        {
            if (RecentlyAddedRubric is null)
                await FillRecentlyAddedTracks(token);
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
        else if (tracksOwner is RubricViewModel rubricViewModel && rubricViewModel.IsDependent)
        {
            IsTrackListVisible = true;
            BackCaption = rubricViewModel.Name;
        }

        if (tracksOwner is not PlaylistViewModel and not PlaylistRubricViewModel)
            TrackSourceCaption = trackSourceCaption;
    }

    protected override bool CanAddRubricTracks(RubricViewModel rubricViewModel)
        => rubricViewModel is SongsRubricViewModel || rubricViewModel.IsDependent;

    protected override void ChangePlaylist(Playlist playlist)
    {
        BackPlaylistCaption = Texts.Playlists;
        PlaylistCaption = playlist.Name;
    }

    private void AddTrackSourceHistory(object tracksOwner)
    {
        if (TrackSourceHistory.Count > 0 && ReferenceEquals(TrackSourceHistory[^1], tracksOwner))
            return;

        TrackSourceHistory.Add(tracksOwner);
    }

    private async Task FillRecentlyAddedTracks(CancellationToken token)
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

    private Task OnBackAsync()
    {
        if (TrackSourceHistory.Count == 0)
            return Task.CompletedTask;

        object currentTracksOwner = TrackSourceHistory[^1];
        TrackSourceHistory.RemoveAt(TrackSourceHistory.Count - 1);
        object tracksOwner = TrackSourceHistory.Count > 0 ? TrackSourceHistory[^1] : null;

        if (currentTracksOwner is PlaylistViewModel or PlaylistRubricViewModel)
        {
            IsPlaylistsVisible = true;
            return Task.CompletedTask;
        }
        else
        {
            IsTrackListVisible = TrackSourceHistory.Count > 1;
            return ChangeTracksAsync(tracksOwner);
        }
    }

    private async Task OnRandomPlayRunAsync()
    {
        var rubricViewModel = new RandomizeRubricViewModel(TrackPlay.Player);
        await ChangeTracksAsync(rubricViewModel);

        var trackViewModel = Tracks.FirstOrDefault();

        if (trackViewModel is null)
            return;

        TrackPlay.ChangeOwnerRubric(rubricViewModel);
        await TrackPlay.PlayAsync(trackViewModel);
        ShowTrackPage();
    }

    private Task OnRecentlyAddedAsync()
    {
        var rubricViewModel = RecentlyAddedRubric;
        TrackPlay.ChangeOwnerRubric(rubricViewModel);
        return ChangeTracksAsync(rubricViewModel);
    }

    private Task OnRecentlyAddedPlaylistsAsync()
    {
        //var rubricViewModel = RecentlyAddedRubric;
        //TrackPlay.ChangeOwnerRubric(rubricViewModel);
        //return ChangeTracksAsync(rubricViewModel);
        return Task.CompletedTask;
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedRubrick))
        {
            var rubricViewModel = SelectedRubrick;

            if (rubricViewModel is not null && !ReferenceEquals(TracksOwner, rubricViewModel))
            {
                TrackSourceHistory.Clear();
                await ChangeTracksAsync(rubricViewModel);
            }
        }
    }

    public async override Task OnNextListAsync()
    {
        var ownerRubric = TrackPlay.OwnerRubric;
        object tracksOwner = (object)TrackPlay.OwnerTrackSource ?? TrackPlay.OwnerRubric;

        if (ownerRubric is null || tracksOwner is null)
            return;

        if (tracksOwner is PlaylistViewModel or PlaylistRubricViewModel)
        {
            SelectedTab = DeviceTabs.Playlists;
            IsPlaylistsVisible = false;
        }
        else
        {
            SelectedTab = DeviceTabs.Tracks;

            if (!ownerRubric.IsDependent)
                SelectedRubrick = ownerRubric;
        }

        OnExit();
        TrackPlay.ChangeOwnerRubric(ownerRubric, TrackPlay.OwnerTrackSource);
        await SelectTracksAsync(tracksOwner);
        SelectedTrack = TrackPlay.CurrentTrack;
    }

    public override void OnExit()
    {
        IsTrackVisible = false;
        IsShortTrackVisible = true;
    }

}

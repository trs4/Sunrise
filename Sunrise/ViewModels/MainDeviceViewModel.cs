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
    private const int _recentlyAddedCount = 10;
    private bool _isShortTrackVisible;
    private bool _isTrackVisible;
    private bool _isTrackListVisible;
    private string _backCaption = Texts.Back;
    private string? _trackSourceCaption;

    public MainDeviceViewModel() { } // For designer

    public MainDeviceViewModel(Player player)
        : base(player)
    {
        BackCommand = new AsyncRelayCommand(OnBackAsync);
        RandomPlayRunCommand = new AsyncRelayCommand(OnRandomPlayRunAsync);
        RecentlyAddedCommand = new AsyncRelayCommand(OnRecentlyAddedAsync);
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

    public string? TrackSourceCaption
    {
        get => _trackSourceCaption;
        set => SetProperty(ref _trackSourceCaption, value);
    }

    public IRelayCommand BackCommand { get; }

    public IRelayCommand RandomPlayRunCommand { get; }

    public IRelayCommand RecentlyAddedCommand { get; }

    public RecentlyAddedRubricViewModel RecentlyAddedRubric { get; private set; }

    public ObservableCollection<TrackViewModel> RecentlyAddedTracks { get; } = [];

    public List<object> TrackSourceHistory { get; } = [];

    protected override async Task ChangeTracksCoreAsync(object tracksOwner, CancellationToken token)
    {
        string trackSourceCaption = null;
        AddTrackSourceHistory(tracksOwner);
        await base.ChangeTracksCoreAsync(tracksOwner, token);

        if (tracksOwner is SongsRubricViewModel)
        {
            if (RecentlyAddedRubric is null)
                await FillRecentlyAddedTracks(token);
        }
        else if (tracksOwner is TrackSourceViewModel trackSourceViewModel)
        {
            IsTrackSourcesVisible = false;
            IsTrackListVisible = true;
            BackCaption = trackSourceViewModel.Rubric.Name;
            trackSourceCaption = trackSourceViewModel.Name;
        }
        else if (tracksOwner is RubricViewModel rubricViewModel && rubricViewModel.IsDependent)
        {
            IsTrackListVisible = true;
            BackCaption = rubricViewModel.Name;
        }

        TrackSourceCaption = trackSourceCaption;
    }

    private void AddTrackSourceHistory(object tracksOwner)
    {
        if (TrackSourceHistory.Count > 0 && ReferenceEquals(TrackSourceHistory[^1], tracksOwner))
            return;

        TrackSourceHistory.Add(tracksOwner);
    }

    private async Task FillRecentlyAddedTracks(CancellationToken token)
    {
        var screenshot = await TrackPlay.Player.GetAllTracksAsync(token);
        RecentlyAddedRubric = new RecentlyAddedRubricViewModel(TrackPlay.Player);
        RecentlyAddedTracks.Clear();

        foreach (var track in RecentlyAddedRubric.GetTracks(screenshot).Take(_recentlyAddedCount))
            RecentlyAddedTracks.Add(GetTrackViewModel(track));
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

        TrackSourceHistory.RemoveAt(TrackSourceHistory.Count - 1);
        object tracksOwner = TrackSourceHistory.Count > 0 ? TrackSourceHistory[^1] : null;
        IsTrackListVisible = TrackSourceHistory.Count > 1;
        return ChangeTracksAsync(tracksOwner);
    }

    private async Task OnRandomPlayRunAsync()
    {
        var rubricViewModel = new RandomizeRubricViewModel(this);
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

    public override void OnExit()
    {
        IsTrackVisible = false;
        IsShortTrackVisible = true;
    }

}

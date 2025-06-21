using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public IRelayCommand BackCommand { get; }

    public IRelayCommand RandomPlayRunCommand { get; }

    public IRelayCommand RecentlyAddedCommand { get; }

    public ObservableCollection<TrackViewModel> RecentlyAddedTracks { get; } = [];

    public List<object> TrackSourceHistory { get; } = [];

    protected override async Task ChangeTracksCoreAsync(object tracksOwner, CancellationToken token)
    {
        TrackSourceHistory.Add(tracksOwner);
        await base.ChangeTracksCoreAsync(tracksOwner, token);

        if (tracksOwner is SongsRubricViewModel)
        {
            RecentlyAddedTracks.Clear();

            foreach (var trackViewModel in Tracks.OrderByDescending(t => t.Added).Take(_recentlyAddedCount))
                RecentlyAddedTracks.Add(trackViewModel);
        }
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
        await ChangeToDependentRubricAsync(rubricViewModel);

        var trackViewModel = Tracks.FirstOrDefault();

        if (trackViewModel is null)
            return;

        await TrackPlay.PlayAsync(trackViewModel);
        ShowTrackPage();
    }

    private Task OnRecentlyAddedAsync()
    {
        var rubricViewModel = new RecentlyAddedRubricViewModel(TrackPlay.Player);
        return ChangeToDependentRubricAsync(rubricViewModel);
    }

    private Task ChangeToDependentRubricAsync(RubricViewModel rubricViewModel)
    {
        IsTrackListVisible = true;
        BackCaption = rubricViewModel.Name;
        return ChangeTracksAsync(rubricViewModel);
    }

    public override void OnExit()
    {
        IsTrackVisible = false;
        IsShortTrackVisible = true;
    }

}

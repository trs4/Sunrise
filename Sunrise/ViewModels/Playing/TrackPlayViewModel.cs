using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Services;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public sealed class TrackPlayViewModel : ObservableObject
{
    private static readonly TimeSpan _prevStayTime = TimeSpan.FromSeconds(4);

    private static readonly object _repeatPlayIconSource = IconSource.From(nameof(Icons.RepeatPlay));
    private static readonly object _repeatOnePlayIconSource = IconSource.From(nameof(Icons.RepeatOnePlay));
    private static readonly object _playIconSource = IconSource.From(nameof(Icons.Play));
    private static readonly object _pauseIconSource = IconSource.From(nameof(Icons.Pause));

    private object _playIcon = _playIconSource;

    private TrackViewModel? _currentTrack;
    private double _volume = 15d;
    private readonly DispatcherTimer _playerTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private TimeSpan _position;
    private bool _randomPlay;
    private bool? _repeatPlay;
    private object? _trackIcon;
    private object _repeatPlayIcon = _repeatPlayIconSource;
    private RubricViewModel? _ownerRubric;
    private TrackSourceViewModel? _ownerTrackSource;

    public TrackPlayViewModel() { } // For designer

    public TrackPlayViewModel(MainViewModel owner, Player player)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Strategy = TrackPlayStrategy.Create(owner, ownerRubric: owner.Songs);
        Player.Media.Volume = _volume;
        player.Media.OnStopped = OnStopped;
        _playerTimer.Tick += OnTick;

        RandomPlayCommand = new RelayCommand(OnRandomPlay);
        RepeatPlayCommand = new RelayCommand(OnRepeatPlay);
        PrevCommand = new AsyncRelayCommand(GoToPrevTrackAsync);
        PlayCommand = new AsyncRelayCommand(PlayPauseTrackAsync);
        NextCommand = new AsyncRelayCommand(GoToNextTrackAsync);

        ImportFromITunesCommand = new AsyncRelayCommand(OnImportFromITunesAsync);
        NextListCommand = new AsyncRelayCommand(OnNextListAsync);
        ExitCommand = new RelayCommand(OnExit);
    }

    public MainViewModel Owner { get; }

    public Player Player { get; }

    public TrackPlayStrategy Strategy { get; private set; }

    public IRelayCommand PrevCommand { get; }

    public IRelayCommand PlayCommand { get; }

    public object PlayIcon
    {
        get => _playIcon;
        set => SetProperty(ref _playIcon, value);
    }

    public IRelayCommand NextCommand { get; }

    public TrackViewModel? CurrentTrack
    {
        get => _currentTrack;
        set => SetProperty(ref _currentTrack, value);
    }

    public double Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    public TimeSpan Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    public IRelayCommand RandomPlayCommand { get; }

    public bool RandomPlay
    {
        get => _randomPlay;
        set => SetProperty(ref _randomPlay, value);
    }

    public IRelayCommand RepeatPlayCommand { get; }

    public bool? RepeatPlay
    {
        get => _repeatPlay;
        set => SetProperty(ref _repeatPlay, value);
    }

    public object? TrackIcon
    {
        get => _trackIcon;
        set => SetProperty(ref _trackIcon, value);
    }

    public object RepeatPlayIcon
    {
        get => _repeatPlayIcon;
        set => SetProperty(ref _repeatPlayIcon, value);
    }

    public IRelayCommand ImportFromITunesCommand { get; }

    public IRelayCommand NextListCommand { get; }

    public IRelayCommand ExitCommand { get; }

    public RubricViewModel? OwnerRubric
    {
        get => _ownerRubric;
        set => SetProperty(ref _ownerRubric, value);
    }

    public TrackSourceViewModel? OwnerTrackSource
    {
        get => _ownerTrackSource;
        set => SetProperty(ref _ownerTrackSource, value);
    }

    public void ChangeOwnerRubric(RubricViewModel? ownerRubric, TrackSourceViewModel? ownerTrackSource = null)
    {
        OwnerRubric = ownerRubric;
        OwnerTrackSource = ownerTrackSource;
        OnPropertyChanged(nameof(TrackPlayStrategy));
    }

    public void ChangeOwnerRubric(TrackSourceViewModel? ownerTrackSource)
    {
        OwnerTrackSource = ownerTrackSource;
        OnPropertyChanged(nameof(TrackPlayStrategy));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Volume))
            Player.Media.Volume = _volume;
        else if (e.PropertyName is nameof(RandomPlay) or nameof(TrackPlayStrategy))
        {
            if (Strategy is not null && Strategy.Equals(_randomPlay, _ownerRubric, _ownerTrackSource))
                return;

            Strategy = TrackPlayStrategy.Create(Owner, _randomPlay, _ownerRubric, _ownerTrackSource);
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var currentTrack = CurrentTrack;

        if (currentTrack is null)
            return;

        Position = TimeSpan.FromMilliseconds(currentTrack.Duration.TotalMilliseconds * Player.Media.Position);
    }

    private void SetPicture(Track track)
        => TrackIconHelper.SetPicture(Player, track, icon => TrackIcon = icon);

    public Task PlayAsync(TrackViewModel trackViewModel) => PlayCoreAsync(trackViewModel);

    public Task PlayItBeginAsync(TrackViewModel trackViewModel) => PlayCoreAsync(trackViewModel, toStart: true);

    private async Task PlayCoreAsync(TrackViewModel trackViewModel, bool toStart = false)
    {
        try
        {
            bool change = toStart || _currentTrack != trackViewModel;

            if (change)
            {
                if (_currentTrack is not null)
                    _currentTrack.IsPlaying = null;

                Position = default;
                TrackIcon = null;
                CurrentTrack = trackViewModel;
            }

            PlayIcon = _pauseIconSource;
            trackViewModel.IsPlaying = true;
            Player.Media.Play(trackViewModel.Track);
            _playerTimer.Start();

            if (change)
                SetPicture(trackViewModel.Track);
        }
        catch (Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandard("Play", e.Message, ButtonEnum.Ok).ShowAsync();
        }
    }

    private void Change(TrackViewModel trackViewModel)
    {
        bool isPlaying = false;

        if (_currentTrack is not null)
        {
            isPlaying = _currentTrack.IsPlaying ?? false;
            _currentTrack.IsPlaying = null;
        }

        Position = default;
        TrackIcon = null;
        CurrentTrack = trackViewModel;
        PlayIcon = isPlaying ? _pauseIconSource : _playIconSource;
        trackViewModel.IsPlaying = isPlaying;

        if (isPlaying)
        {
            Player.Media.Play(trackViewModel.Track);
            _playerTimer.Start();
        }
        else
        {
            Player.Media.Pause();
            _playerTimer.Stop();
        }

        SetPicture(trackViewModel.Track);
    }

    public void ChangePosition(double position)
    {
        var currentTrack = CurrentTrack;
        Position = currentTrack is null ? default : TimeSpan.FromMilliseconds(currentTrack.Duration.TotalMilliseconds * position);
        Player.Media.Position = position;
    }

    private void Pause(TrackViewModel trackViewModel)
    {
        PlayIcon = _playIconSource;
        trackViewModel.IsPlaying = false;
        Player.Media.Pause();
        _playerTimer.Stop();
    }

    private void Clear()
    {
        if (_currentTrack is not null)
            _currentTrack.IsPlaying = null;

        CurrentTrack = null;
        Position = default;
        TrackIcon = null;
        PlayIcon = _playIconSource;
        CurrentTrack = null;
        Player.Media.Stop();
        _playerTimer.Stop();
        TrackIcon = null;
    }

    private void OnRandomPlay() => RandomPlay = !_randomPlay;

    private void OnRepeatPlay()
    {
        if (!_repeatPlay.HasValue)
        {
            RepeatPlayIcon = _repeatPlayIconSource;
            RepeatPlay = false;
        }
        else if (!_repeatPlay.Value)
        {
            RepeatPlayIcon = _repeatOnePlayIconSource;
            RepeatPlay = true;
        }
        else
        {
            RepeatPlayIcon = _repeatPlayIconSource;
            RepeatPlay = null;
        }
    }

    private async Task PlayPauseTrackAsync()
    {
        var currentTrack = CurrentTrack ??= await Strategy.GetFirstAsync();

        if (currentTrack is null)
            return;

        if (currentTrack.IsPlaying ?? false)
            Pause(currentTrack);
        else
            await PlayCoreAsync(currentTrack);
    }

    private async Task GoToPrevTrackAsync()
    {
        var track = CurrentTrack;

        if (track is null)
            return;

        if (_repeatPlay == true || _position > _prevStayTime)
        {
            ChangePosition(0);
            return;
        }

        while (true)
        {
            track = await Strategy.GetPrevAsync(track);

            if (track is null)
            {
                Clear();
                break;
            }
            else if (Owner.SelectedRubrick is not SongsRubricViewModel || track.Picked)
            {
                Change(track);
                break;
            }
        }
    }

    private async Task GoToNextTrackAsync()
    {
        var track = CurrentTrack;

        if (track is null)
        {
            await PlayPauseTrackAsync();
            return;
        }

        if (_repeatPlay == true)
        {
            ChangePosition(0);
            return;
        }

        while (true)
        {
            var nextTrack = await Strategy.GetNextAsync(track);

            if (nextTrack is null)
            {
                if (_repeatPlay == false)
                {
                    track = await Strategy.GetFirstAsync();

                    if (track is not null)
                        Change(track);
                }
                else
                    Clear();

                break;
            }
            else if (Owner.SelectedRubrick is SongsRubricViewModel && !nextTrack.Picked)
                track = nextTrack;
            else
            {
                Change(nextTrack);
                break;
            }
        }
    }

    private async ValueTask OnStopped()
    {
        var currentTrack = CurrentTrack;

        if (currentTrack is null)
            return;

        if (Player.Media.IsEnd)
        {
            await Player.OnEndPlayedAsync(currentTrack.Track);
            currentTrack.Reproduced = currentTrack.Track.Reproduced;
            currentTrack.LastPlay = currentTrack.Track.LastPlay;
        }

        if (_repeatPlay == true)
        {
            ChangePosition(0);
            await PlayCoreAsync(currentTrack);
            return;
        }

        var nextTrack = await Strategy.GetNextAsync(currentTrack);

        if (nextTrack is null)
        {
            if (_repeatPlay == false)
            {
                currentTrack = await Strategy.GetFirstAsync();

                if (currentTrack is null)
                    return;

                await PlayCoreAsync(currentTrack);
            }
            else
                Clear();
        }
        else
            await PlayCoreAsync(nextTrack);
    }

    private async Task OnImportFromITunesAsync(CancellationToken token)
    {
        string? filePath = null;

        if (!UIDispatcher.Run(() => AppServices.Get<ISystemDialogsService>().ShowSelectFile(out filePath)))
            return;

        await ImportFromITunes.LoadAsync(Player, filePath, token: token);
        Player.ClearAllTracks();
        Player.ClearAllPlaylists();
        await Owner.ReloadTracksAsync(token);
    }

    private Task OnNextListAsync() => Owner.OnNextListAsync();

    private void OnExit() => Owner.OnExit();
}

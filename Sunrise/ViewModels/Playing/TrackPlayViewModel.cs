using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public TrackPlayViewModel() { } // For designer

    public TrackPlayViewModel(MainViewModel owner, Player player)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Strategy = TrackPlayStrategy.Create(owner);
        Player.Media.Volume = _volume;
        player.Media.OnStopped = OnStopped;
        _playerTimer.Tick += OnTick;

        RandomPlayCommand = new RelayCommand(OnRandomPlay);
        RepeatPlayCommand = new RelayCommand(OnRepeatPlay);
        PrevCommand = new RelayCommand(GoToPrevTrack);
        PlayCommand = new RelayCommand(PlayPauseTrack);
        NextCommand = new RelayCommand(GoToNextTrack);

        ImportFromITunesCommand = new AsyncRelayCommand(OnImportFromITunesAsync);
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

    public IRelayCommand ExitCommand { get; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Volume))
            Player.Media.Volume = _volume;
        else if (e.PropertyName == nameof(RandomPlay))
            Strategy = TrackPlayStrategy.Create(Owner, _randomPlay);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var currentTrack = CurrentTrack;

        if (currentTrack is null)
            return;

        Position = TimeSpan.FromMilliseconds(currentTrack.Duration.TotalMilliseconds * Player.Media.Position);
    }

    private void SetPicture(Track track)
    {
        if (!track.HasPicture)
            return;

        var icon = track.PictureIcon;

        if (icon is not null)
        {
            TrackIcon = icon;
            return;
        }

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var picture = await Player.LoadPictureAsync(track);

            if (picture is null)
                track.HasPicture = false;
            else
            {
                try
                {
                    using var bitmapStream = new MemoryStream(picture.Data);
                    bitmapStream.Seek(0, SeekOrigin.Begin);
                    var bitmap = new Bitmap(bitmapStream);
                    TrackIcon = track.PictureIcon = bitmap; // %%TODO Convert to 48 x 48
                }
                catch
                {
                    track.HasPicture = false;
                }
            }
        }, DispatcherPriority.Normal);
    }

    public void Play(TrackViewModel trackViewModel) => PlayCore(trackViewModel, toStart: true);

    private void PlayCore(TrackViewModel trackViewModel, bool toStart = false)
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

    private void PlayPauseTrack()
    {
        var currentTrack = CurrentTrack ??= Strategy.GetFirst();

        if (currentTrack is null)
            return;

        if (currentTrack.IsPlaying ?? false)
            Pause(currentTrack);
        else
            PlayCore(currentTrack);
    }

    private void GoToPrevTrack()
    {
        var currentTrack = CurrentTrack;

        if (currentTrack is null)
            return;

        if (_repeatPlay == true || _position > _prevStayTime)
        {
            ChangePosition(0);
            return;
        }

        var prevTrack = Strategy.GetPrev(currentTrack);

        if (prevTrack is null)
            Clear();
        else
            Change(prevTrack);
    }

    private void GoToNextTrack()
    {
        var currentTrack = CurrentTrack;

        if (currentTrack is null)
        {
            PlayPauseTrack();
            return;
        }

        if (_repeatPlay == true)
        {
            ChangePosition(0);
            return;
        }

        var nextTrack = Strategy.GetNext(currentTrack);

        if (nextTrack is null)
        {
            if (_repeatPlay == false)
            {
                currentTrack = Strategy.GetFirst();

                if (currentTrack is null)
                    return;

                Change(currentTrack);
            }
            else
                Clear();
        }
        else
            Change(nextTrack);
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
            PlayCore(currentTrack);
            return;
        }

        var nextTrack = Strategy.GetNext(currentTrack);

        if (nextTrack is null)
        {
            if (_repeatPlay == false)
            {
                currentTrack = Strategy.GetFirst();

                if (currentTrack is null)
                    return;

                PlayCore(currentTrack);
            }
            else
                Clear();
        }
        else
            PlayCore(nextTrack);
    }

    private async Task OnImportFromITunesAsync(CancellationToken token)
    {
        string? filePath = null;

        if (!UIDispatcher.Run(() => AppServices.Get<ISystemDialogsService>().ShowSelectFile(out filePath)))
            return;

        await ImportFromITunes.LoadAsync(Player, filePath, token: token);
    }

    private void OnExit() => Owner.Owner.Close();
}

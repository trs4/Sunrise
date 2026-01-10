using Sunrise.Model.SoundFlow.Components;
using Sunrise.Model.SoundFlow.Enums;

namespace Sunrise.Model;

public sealed class MediaPlayer : IDisposable
{
    private volatile bool _canOnStoppedRaised = true;
    private SoundPlayer? _wavePlayer;
    private Track? _track;
    private string _filePath;
    private double _volume = 100d;

    internal MediaPlayer(Player player)
        => Player = player;

    public Player Player { get; }

    /// <summary>Громкость от 0 до 100</summary>
    public double Volume
    {
        get => _volume;
        set
        {
            if (value < 0)
                value = 0d;
            else if (value > 100d)
                value = 100d;

            _volume = value;
            var wavePlayer = _wavePlayer;

            if (wavePlayer is not null)
                SetVolume(wavePlayer);
        }
    }

    public double Position
    {
        get
        {
            var wavePlayer = _wavePlayer;
            return wavePlayer is null ? 0d : (double)wavePlayer.DataProvider.Position / wavePlayer.DataProvider.Length;
        }
        set
        {
            var wavePlayer = _wavePlayer;

            if (wavePlayer is not null)
            {
                int sampleOffset = (int)(wavePlayer.DataProvider.Length * value);
                wavePlayer.Seek(sampleOffset);
                RaiseStateChanged();
            }
        }
    }

    public bool IsPlaying => _wavePlayer?.State == PlaybackState.Playing;

    public bool IsEnd
    {
        get
        {
            var wavePlayer = _wavePlayer;
            return wavePlayer is not null && wavePlayer.DataProvider.Position >= wavePlayer.DataProvider.Length;
        }
    }

    public Func<ValueTask> OnStopped { get; set; }

    public event TrackStateChangedEventHandler? StateChanged;

    private void RaiseStateChanged()
    {
        var stateChanged = StateChanged;

        if (stateChanged is null)
            return;

        var args = GetStateArgs();

        if (args is null)
            return;

        stateChanged(this, args);
    }

    public TrackStateChangedEventArgs? GetStateArgs()
    {
        var track = _track;
        var wavePlayer = _wavePlayer;

        if (track is null || wavePlayer is null)
            return null;

        double position = (double)wavePlayer.DataProvider.Position / wavePlayer.DataProvider.Length;
        return new(track, wavePlayer.State, position);
    }

    public void Play(Track track) => PlayPause(track, true);

    public void Pause(Track track) => PlayPause(track, false);

    private void PlayPause(Track track, bool play)
    {
        _track = track ?? throw new ArgumentNullException(nameof(track));
        string filePath = track.Path;
        SoundPlayer? prevWavePlayer = null;
        _canOnStoppedRaised = false;

        try
        {
            var wavePlayer = _wavePlayer;

            if (wavePlayer is null)
            {
                _filePath = filePath;
                _wavePlayer = wavePlayer = CreateWavePlayer(filePath);
            }
            else if (wavePlayer.State == PlaybackState.Playing)
                wavePlayer.Stop();

            if (_filePath != filePath)
            {
                prevWavePlayer = wavePlayer;
                _filePath = filePath;

                if (wavePlayer.State != PlaybackState.Stopped)
                    wavePlayer.Stop();

                _wavePlayer = wavePlayer = CreateWavePlayer(filePath);
            }
            else if (wavePlayer.State == PlaybackState.Playing)
                wavePlayer.Seek(0);

            if (play)
            {
                if (IsEnd)
                    wavePlayer.Seek(0);

                wavePlayer.Play();
            }
            else
                wavePlayer.Pause();

            RaiseStateChanged();
        }
        finally
        {
            prevWavePlayer?.Dispose();
            _canOnStoppedRaised = true;
        }
    }

    private SoundPlayer CreateWavePlayer(string filePath)
    {
        var wavePlayer = WavePlayer.Create(filePath);
        SetVolume(wavePlayer);
        wavePlayer.PlaybackEnded += OnPlaybackEnded;
        return wavePlayer;
    }

    private void SetVolume(SoundPlayer wavePlayer)
        => wavePlayer.Volume = (float)(_volume / 100d);

    private async void OnPlaybackEnded(object? sender, EventArgs e)
    {
        if (_canOnStoppedRaised && _wavePlayer?.State == PlaybackState.Stopped)
        {
            if (OnStopped is not null)
                await OnStopped();

            RaiseStateChanged();
        }
    }

    public void Pause()
    {
        _wavePlayer?.Pause();
        RaiseStateChanged();
    }

    public void Stop()
    {
        _wavePlayer?.Stop();
        RaiseStateChanged();
    }

    public void Dispose()
    {
        var wavePlayer = _wavePlayer;

        if (wavePlayer is not null)
        {
            wavePlayer.PlaybackEnded -= OnPlaybackEnded;
            wavePlayer.Dispose();
            _wavePlayer = null;
        }

        GC.SuppressFinalize(this);
    }

}

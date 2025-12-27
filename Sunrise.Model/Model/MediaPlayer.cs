using Sunrise.Model.SoundFlow.Components;
using Sunrise.Model.SoundFlow.Enums;

namespace Sunrise.Model;

public sealed class MediaPlayer : IDisposable
{
    private volatile bool _canOnStoppedRaised = true;
    private SoundPlayer? _wavePlayer;
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
            }
        }
    }

    public bool IsEnd
    {
        get
        {
            var wavePlayer = _wavePlayer;
            return wavePlayer is not null && wavePlayer.DataProvider.Position >= wavePlayer.DataProvider.Length;
        }
    }

    public Func<ValueTask> OnStopped { get; set; }

    public void Play(Track track)
    {
        ArgumentNullException.ThrowIfNull(track);
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

            wavePlayer.Play();
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
        if (_canOnStoppedRaised && _wavePlayer?.State == PlaybackState.Stopped && OnStopped is not null)
            await OnStopped();
    }

    public void Pause() => _wavePlayer?.Pause();

    public void Stop() => _wavePlayer?.Stop();

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

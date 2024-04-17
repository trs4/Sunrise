using Sunrise.Model.Audio;
using Sunrise.Model.Audio.Wave;

namespace Sunrise.Model;

public sealed class MediaPlayer : IDisposable
{
    private volatile bool _canOnStoppedRaised = true;
    private WaveOutEvent? _wavePlayer;
    private string _filePath;
    private WaveStream? _fileStream;
    private VolumeWaveProvider16 _volumeProvider;
    private double _volume = 100d;

    internal MediaPlayer(Player player)
        => Player = player;

    public Player Player { get; }

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
            var volumeProvider = _volumeProvider;

            if (volumeProvider is not null)
                volumeProvider.Volume = (float)(value / 100d);
        }
    }

    public double Position
    {
        get
        {
            var fileStream = _fileStream;
            return fileStream is null ? 0d : (double)fileStream.Position / fileStream.Length;
        }
        set
        {
            var fileStream = _fileStream;

            if (fileStream is not null)
                fileStream.Position = (long)(fileStream.Length * value);
        }
    }

    public bool IsEnd
    {
        get
        {
            var fileStream = _fileStream;
            return fileStream is not null && fileStream.Position >= fileStream.Length;
        }
    }

    public Func<ValueTask> OnStopped { get; set; }

    public void Play(Track track)
    {
        ArgumentNullException.ThrowIfNull(track);
        string filePath = track.Path;
        Stream prevFileStream = null;
        _canOnStoppedRaised = false;

        try
        {
            var wavePlayer = _wavePlayer;

            if (wavePlayer is null)
            {
                _wavePlayer = wavePlayer = new WaveOutEvent { DesiredLatency = 200 };
                wavePlayer.PlaybackStopped += OnPlaybackStopped;
            }
            else if (wavePlayer.PlaybackState != PlaybackState.Paused)
                wavePlayer.Stop();

            if (_filePath != filePath)
            {
                prevFileStream = _fileStream;
                _fileStream = new Mp3FileReader(filePath);
                _volumeProvider = new VolumeWaveProvider16(_fileStream) { Volume = (float)(_volume / 100d) };
                _filePath = filePath;

                if (wavePlayer.PlaybackState != PlaybackState.Stopped)
                    wavePlayer.Stop();

                wavePlayer.Init(_volumeProvider);
            }
            else if (wavePlayer.PlaybackState != PlaybackState.Paused)
                _fileStream?.Seek(0, SeekOrigin.Begin);

            wavePlayer.Play();
        }
        finally
        {
            prevFileStream?.Dispose();
            _canOnStoppedRaised = true;
        }
    }

    private async void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (_canOnStoppedRaised && _wavePlayer?.PlaybackState == PlaybackState.Stopped && OnStopped is not null)
            await OnStopped();
    }

    public void Pause() => _wavePlayer?.Pause();

    public void Stop() => _wavePlayer?.Stop();

    public void Dispose()
    {
        var wavePlayer = _wavePlayer;

        if (wavePlayer is not null)
        {
            wavePlayer.PlaybackStopped -= OnPlaybackStopped;
            wavePlayer.Dispose();
            _wavePlayer = null;
        }

        _fileStream?.Dispose();
        _fileStream = null;

        GC.SuppressFinalize(this);
    }

}

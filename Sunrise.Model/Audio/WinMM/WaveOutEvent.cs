using System.Diagnostics;
using Sunrise.Model.Audio.Wave;

namespace Sunrise.Model.Audio;

/// <summary>Alternative WaveOut class, making use of the Event callback</summary>
public sealed class WaveOutEvent : IWavePlayer, IWavePosition
{
    private readonly object _waveOutLock = new object();
    private readonly SynchronizationContext? _syncContext;
    private IntPtr _hWaveOut; // WaveOut handle
    private WaveOutBuffer[]? _buffers;
    private IWaveProvider _waveStream;
    private volatile PlaybackState _playbackState;
    private AutoResetEvent? _callbackEvent;

    /// <summary>Indicates playback has stopped automatically</summary>
    public event EventHandler<StoppedEventArgs> PlaybackStopped;

    /// <summary>Gets or sets the desired latency in milliseconds. Should be set before a call to Init</summary>
    public int DesiredLatency { get; set; }

    /// <summary>Gets or sets the number of buffers used. Should be set before a call to Init</summary>
    public int NumberOfBuffers { get; set; }

    /// <summary>
    /// Gets or sets the device number
    /// Should be set before a call to Init
    /// This must be between -1 and <see>DeviceCount</see> - 1.
    /// -1 means stick to default device even default device is changed
    /// </summary>
    public int DeviceNumber { get; set; } = -1;

    /// <summary>Opens a WaveOut device</summary>
    public WaveOutEvent()
    {
        _syncContext = SynchronizationContext.Current;

        if (_syncContext is not null)
        {
            string name = _syncContext.GetType().Name;

            if (name == "LegacyAspNetSynchronizationContext" || name == "AspNetSynchronizationContext")
                _syncContext = null;
        }

        // set default values up
        DesiredLatency = 300;
        NumberOfBuffers = 2;
    }

    /// <summary>Initialises the WaveOut device</summary>
    /// <param name="waveProvider">WaveProvider to play</param>
    public void Init(IWaveProvider waveProvider)
    {
        if (_playbackState != PlaybackState.Stopped)
            throw new InvalidOperationException("Can't re-initialize during playback");

        if (_hWaveOut != IntPtr.Zero)
        {
            // normally we don't allow calling Init twice, but as experiment, see if we can clean up and go again
            // try to allow reuse of this waveOut device
            // n.b. risky if Playback thread has not exited
            DisposeBuffers();
            CloseWaveOut();
        }

        _callbackEvent = new AutoResetEvent(false);
        _waveStream = waveProvider;
        int bufferSize = waveProvider.WaveFormat.ConvertLatencyToByteSize((DesiredLatency + NumberOfBuffers - 1) / NumberOfBuffers);
        MmResult result;

        lock (_waveOutLock)
        {
            result = WaveInterop.waveOutOpenWindow(out _hWaveOut, DeviceNumber, _waveStream.WaveFormat,
                _callbackEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackEvent);
        }

        MmException.Try(result, "waveOutOpen");

        _buffers = new WaveOutBuffer[NumberOfBuffers];
        _playbackState = PlaybackState.Stopped;

        for (var n = 0; n < NumberOfBuffers; n++)
            _buffers[n] = new WaveOutBuffer(_hWaveOut, bufferSize, _waveStream, _waveOutLock);
    }

    /// <summary>Start playing the audio from the WaveStream</summary>
    public void Play()
    {
        if (_buffers is null || _waveStream is null)
            throw new InvalidOperationException("Must call Init first");

        if (_playbackState == PlaybackState.Stopped)
        {
            _playbackState = PlaybackState.Playing;
            _callbackEvent?.Set(); // give the thread a kick
            ThreadPool.QueueUserWorkItem(state => PlaybackThread(), null);
        }
        else if (_playbackState == PlaybackState.Paused)
        {
            Resume();
            _callbackEvent?.Set(); // give the thread a kick
        }
    }

    private void PlaybackThread()
    {
        Exception exception = null;

        try
        {
            DoPlayback();
        }
        catch (Exception e)
        {
            exception = e;
        }
        finally
        {
            _playbackState = PlaybackState.Stopped;
            // we're exiting our background thread
            RaisePlaybackStoppedEvent(exception);
        }
    }

    private void DoPlayback()
    {
        while (_playbackState != PlaybackState.Stopped)
        {
            if (_callbackEvent is not null && !_callbackEvent.WaitOne(DesiredLatency))
            {
                if (_playbackState == PlaybackState.Playing)
                    Debug.WriteLine("WARNING: WaveOutEvent callback event timeout");
            }

            // requeue any buffers returned to us
            if (_playbackState == PlaybackState.Playing)
            {
                int queued = 0;

                if (_buffers is not null)
                {
                    foreach (var buffer in _buffers)
                    {
                        if (buffer.InQueue || buffer.OnDone())
                            queued++;
                    }
                }

                if (queued == 0)
                {
                    // we got to the end
                    _playbackState = PlaybackState.Stopped;
                    _callbackEvent?.Set();
                }
            }
        }
    }

    /// <summary>Pause the audio</summary>
    public void Pause()
    {
        if (_playbackState == PlaybackState.Playing)
        {
            MmResult result;
            _playbackState = PlaybackState.Paused; // set this here to avoid a deadlock problem with some drivers

            lock (_waveOutLock)
                result = WaveInterop.waveOutPause(_hWaveOut);

            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutPause");
        }
    }

    /// <summary>Resume playing after a pause from the same position</summary>
    private void Resume()
    {
        if (_playbackState == PlaybackState.Paused)
        {
            MmResult result;

            lock (_waveOutLock)
                result = WaveInterop.waveOutRestart(_hWaveOut);

            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutRestart");

            _playbackState = PlaybackState.Playing;
        }
    }

    /// <summary>Stop and reset the WaveOut device</summary>
    public void Stop()
    {
        if (_playbackState != PlaybackState.Stopped)
        {
            // in the call to waveOutReset with function callbacks
            // some drivers will block here until OnDone is called
            // for every buffer
            _playbackState = PlaybackState.Stopped; // set this here to avoid a problem with some drivers whereby 
            MmResult result;

            lock (_waveOutLock)
                result = WaveInterop.waveOutReset(_hWaveOut);

            if (result != MmResult.NoError)
                throw new MmException(result, "waveOutReset");

            _callbackEvent?.Set(); // give the thread a kick, make sure we exit
        }
    }

    /// <summary>
    /// Gets the current position in bytes from the wave output device.
    /// (n.b. this is not the same thing as the position within your reader
    /// stream - it calls directly into waveOutGetPosition)
    /// </summary>
    /// <returns>Position in bytes</returns>
    public long GetPosition() => WaveOutUtils.GetPositionBytes(_hWaveOut, _waveOutLock);

    /// <summary>Gets a <see cref="Wave.WaveFormat"/> instance indicating the format the hardware is using</summary>
    public WaveFormat OutputWaveFormat => _waveStream.WaveFormat;

    /// <summary>Playback State</summary>
    public PlaybackState PlaybackState => _playbackState;

    /// <summary>Volume for this device 1.0 is full scale</summary>
    public float Volume
    {
        get => WaveOutUtils.GetWaveOutVolume(_hWaveOut, _waveOutLock);
        set => WaveOutUtils.SetWaveOutVolume(value, _hWaveOut, _waveOutLock);
    }

    #region Dispose Pattern

    /// <summary>Closes this WaveOut device</summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    /// <summary>Closes the WaveOut device and disposes of buffers</summary>
    /// <param name="disposing">True if called from <see>Dispose</see></param>
    private void Dispose(bool disposing)
    {
        Stop();

        if (disposing)
            DisposeBuffers();

        CloseWaveOut();
    }

    private void CloseWaveOut()
    {
        var callbackEvent = _callbackEvent;

        if (callbackEvent is not null)
        {
            callbackEvent.Close();
            _callbackEvent = null;
        }

        lock (_waveOutLock)
        {
            if (_hWaveOut != IntPtr.Zero)
            {
                WaveInterop.waveOutClose(_hWaveOut);
                _hWaveOut = IntPtr.Zero;
            }
        }
    }

    private void DisposeBuffers()
    {
        if (_buffers is not null)
        {
            foreach (var buffer in _buffers)
                buffer.Dispose();

            _buffers = null;
        }
    }

    /// <summary>Finalizer. Only called when user forgets to call <see>Dispose</see></summary>
    ~WaveOutEvent()
    {
        Dispose(false);
        Debug.Assert(false, "WaveOutEvent device was not closed");
    }

    #endregion

    private void RaisePlaybackStoppedEvent(Exception e)
    {
        var handler = PlaybackStopped;

        if (handler is null)
            return;

        if (_syncContext is null)
            handler(this, new StoppedEventArgs(e));
        else
            _syncContext.Post(state => handler(this, new StoppedEventArgs(e)), null);
    }

}

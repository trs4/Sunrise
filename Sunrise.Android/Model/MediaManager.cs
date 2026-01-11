using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Android.Media.Session;
using Android.Telephony;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Sunrise.Model;
using Sunrise.Model.Common;
using Sunrise.ViewModels;

namespace Sunrise.Android.Model;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
internal sealed class MediaManager
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private const int _rewindSeconds = 10;
    private readonly AvaloniaMainActivity _activity;
    private readonly AudioManager _audioManager;
    private readonly TelephonyManager _telephonyManager;
    private readonly MediaSession _mediaSession;
    private readonly MediaDeviceCallback _mediaDeviceCallback;
    private readonly MediaTelephonyCallback _telephonyCallback;
    private string? _currentProductName;
    private DateTime _lastPlayPauseTime;
    private static readonly TimeSpan _delay = TimeSpan.FromSeconds(1);

#pragma warning disable CA2000 // Dispose objects before losing scope
    public MediaManager(AvaloniaMainActivity activity)
    {
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));

        MainViewModel = (activity.Content as StyledElement)?.DataContext as MainDeviceViewModel
            ?? throw new InvalidOperationException(nameof(MainDeviceViewModel));

        _audioManager = (AudioManager)activity.GetSystemService(Context.AudioService)
            ?? throw new InvalidOperationException(nameof(AudioManager));

        _telephonyManager = (TelephonyManager)activity.GetSystemService(Context.TelephonyService)
            ?? throw new InvalidOperationException(nameof(AudioManager));

        _mediaSession = new MediaSession(activity, "SunrisePlayerMediaSession");

#pragma warning disable CA1416 // Validate platform compatibility
        _telephonyCallback = new MediaTelephonyCallback(MainViewModel);
        _telephonyManager.RegisterTelephonyCallback(activity.MainExecutor, _telephonyCallback);

        _mediaDeviceCallback = new MediaDeviceCallback(this);
        _audioManager.RegisterAudioDeviceCallback(_mediaDeviceCallback, null);
#pragma warning restore CA1416 // Validate platform compatibility

        _mediaSession.SetCallback(new MediaCallback(this));

        _mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
        _mediaSession.Active = true;

        if (IsBluetoothA2dpOn())
            MediaHelper.SendPlaybackState(_mediaSession);

        var mediaPlayer = MainViewModel.TrackPlay.Player.Media;
        mediaPlayer.StateChanged -= MediaPlayer_StateChanged;
        mediaPlayer.StateChanged += MediaPlayer_StateChanged;
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    public MainDeviceViewModel MainViewModel { get; }

    public Track? CurrentTrack { get; private set; }

    public bool Execute(Keycode key, [CallerMemberName] string? propertyName = null)
        => Tasks.Execute(ExecuteAsync(key, propertyName));

    public async Task<bool> ExecuteAsync(Keycode key, [CallerMemberName] string? propertyName = null)
    {
        try
        {
            if (MainViewModel.SettingsDisplayed)
                MainViewModel.WriteInfo($"KeyDown {propertyName}: {key}");

            switch (key)
            {
                case Keycode.MediaPlay:
                    if (!CanPlayPauseExecute())
                        return true;

                    await MainViewModel.TrackPlay.PlayTrackAsync();
                    break;
                case Keycode.MediaPlayPause:
                case Keycode.MediaPause:
                    if (!CanPlayPauseExecute())
                        return true;

                    await MainViewModel.TrackPlay.PlayPauseTrackAsync();
                    break;
                case Keycode.MediaStop:
                    await MainViewModel.TrackPlay.PauseTrackAsync();
                    break;
                case Keycode.MediaPrevious:
                    await MainViewModel.TrackPlay.GoToPrevTrackAsync();
                    break;
                case Keycode.MediaNext:
                    await MainViewModel.TrackPlay.GoToNextTrackAsync();
                    break;
                case Keycode.MediaRewind:
                    Rewind(-_rewindSeconds);
                    break;
                case Keycode.MediaFastForward:
                    Rewind(_rewindSeconds);
                    break;
                case Keycode.Back:
                    if (!await MainViewModel.BackAsync())
                        Hide();

                    break;
                default:
                    return false;
            }
        }
        catch (Exception e)
        {
            MainViewModel.WriteInfo(e);
        }

        return true;
    }

    private bool CanPlayPauseExecute()
    {
        var now = DateTime.Now;
        bool result = now.Subtract(_lastPlayPauseTime) >= _delay;
        _lastPlayPauseTime = now;
        return result;
    }

    private void Rewind(int rewindSeconds)
    {
        var track = CurrentTrack;

        if (track is null)
            return;

        var trackPlay = MainViewModel.TrackPlay;
        double position = trackPlay.Position.Add(TimeSpan.FromSeconds(rewindSeconds)).TotalMilliseconds / track.Duration.TotalMilliseconds;
        trackPlay.ChangePositionDelay(position);
    }

#pragma warning disable CA1416 // Validate platform compatibility
    public bool IsBluetoothA2dpOn()
        => _audioManager.GetDevices(GetDevicesTargets.Outputs)?.Any(d => d.Type == AudioDeviceType.BluetoothA2dp) ?? false;

    public bool IsBluetoothA2dpOn(out string? productName)
    {
        var device = _audioManager.GetDevices(GetDevicesTargets.Outputs)?.FirstOrDefault(d => d.Type == AudioDeviceType.BluetoothA2dp);
        productName = device?.ProductName;
        return device is not null;
    }
#pragma warning restore CA1416 // Validate platform compatibility

    private void MediaPlayer_StateChanged(object? sender, TrackStateChangedEventArgs args)
    {
        if (!IsBluetoothA2dpOn(out string? productName))
            return;

        SendMetadata(args, productName);
    }

    public void SendCurrentMetadata()
    {
        if (!IsBluetoothA2dpOn(out string? productName))
            return;

        var args = MainViewModel.TrackPlay.Player.Media.GetStateArgs();

        if (args is null)
            return;

        SendMetadata(args, productName, forced: true);
    }

    private void SendMetadata(TrackStateChangedEventArgs args, string? productName, bool forced = false)
    {
        var (playbackStateCode, durationMilliseconds, positionMilliseconds) = MediaHelper.Prepare(args);

        if (forced || _currentProductName != productName || !ReferenceEquals(CurrentTrack, args.Track))
        {
            _currentProductName = productName;
            CurrentTrack = args.Track;
            MediaHelper.SendMediaMetadata(_mediaSession, args.Track, durationMilliseconds);
        }

        MediaHelper.SendPlaybackState(_mediaSession, playbackStateCode, positionMilliseconds);
    }

#pragma warning disable CA2000 // Dispose objects before losing scope
    private void Hide()
    {
        var main = new Intent(Intent.ActionMain);
        main.AddCategory(Intent.CategoryHome);
        _activity.StartActivity(main);
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    public void Release()
    {
        MainViewModel.TrackPlay.Player.Media.StateChanged -= MediaPlayer_StateChanged;
        CurrentTrack = null;
        _currentProductName = null;
#pragma warning disable CA1416 // Validate platform compatibility
        _audioManager.UnregisterAudioDeviceCallback(_mediaDeviceCallback);
        _telephonyManager.UnregisterTelephonyCallback(_telephonyCallback);
#pragma warning restore CA1416 // Validate platform compatibility
        _mediaSession.Active = false;
        _mediaSession.Release();
    }

}

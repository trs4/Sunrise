using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Android.Media.Session;
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
    private readonly AvaloniaMainActivity _activity;
    private readonly MainDeviceViewModel _mainViewModel;
    private readonly AudioManager _audioManager;
    private readonly MediaSession _mediaSession;
    private string? _currentProductName;
    private Track? _currentTrack;

#pragma warning disable CA2000 // Dispose objects before losing scope
    public MediaManager(AvaloniaMainActivity activity)
    {
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));

        _mainViewModel = (activity.Content as StyledElement)?.DataContext as MainDeviceViewModel
            ?? throw new InvalidOperationException(nameof(MainDeviceViewModel));

        _audioManager = (AudioManager)activity.GetSystemService(Context.AudioService)
            ?? throw new InvalidOperationException(nameof(AudioManager));

        _mediaSession = new MediaSession(activity, "SunrisePlayerMediaSession");

#pragma warning disable CA1416 // Validate platform compatibility
        _audioManager.RegisterAudioDeviceCallback(new MediaDeviceCallback(this), null);
#pragma warning restore CA1416 // Validate platform compatibility

        _mediaSession.SetCallback(new MediaCallback(this));

        _mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
        _mediaSession.Active = true;

        var mediaPlayer = _mainViewModel.TrackPlay.Player.Media;
        mediaPlayer.StateChanged -= MediaPlayer_StateChanged;
        mediaPlayer.StateChanged += MediaPlayer_StateChanged;
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    public void Execute(Keycode key) => Tasks.Execute(ExecuteAsync(key));

    public async Task ExecuteAsync(Keycode key)
    {
        try
        {
            switch (key)
            {
                case Keycode.MediaPlay:
                    await _mainViewModel.TrackPlay.PlayTrackAsync();
                    break;
                case Keycode.MediaPause:
                case Keycode.MediaPlayPause:
                    await _mainViewModel.TrackPlay.PlayPauseTrackAsync();
                    break;
                case Keycode.MediaStop:
                    await _mainViewModel.TrackPlay.PauseTrackAsync();
                    break;
                case Keycode.MediaNext:
                    await _mainViewModel.TrackPlay.GoToNextTrackAsync();
                    break;
                case Keycode.MediaPrevious:
                    await _mainViewModel.TrackPlay.GoToPrevTrackAsync();
                    break;
                case Keycode.Back:
                    await _mainViewModel.BackAsync();

                    if (!_mainViewModel.CanBack())
                        Hide();

                    break;
            }
        }
        catch (Exception e)
        {
            _mainViewModel.WriteInfo(e);
        }
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

        var args = _mainViewModel.TrackPlay.Player.Media.GetStateArgs();

        if (args is null)
            return;

        SendMetadata(args, productName, forced: true);
    }

    private void SendMetadata(TrackStateChangedEventArgs args, string? productName, bool forced = false)
    {
        var (playbackStateCode, durationMilliseconds, positionMilliseconds) = MediaHelper.Prepare(args);

        if (forced || _currentProductName != productName || !ReferenceEquals(_currentTrack, args.Track))
        {
            _currentProductName = productName;
            _currentTrack = args.Track;
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
        _mainViewModel.TrackPlay.Player.Media.StateChanged -= MediaPlayer_StateChanged;
        _currentTrack = null;
        _currentProductName = null;
        _mediaSession.Release();
    }

}

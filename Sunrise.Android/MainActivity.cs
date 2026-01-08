using System;
using Android.App;
using Android.Content.PM;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Sunrise.Android.Model;
using Sunrise.Model;
using Sunrise.ViewModels;

namespace Sunrise.Android;

[Activity(
    Label = "Музыка",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private MainDeviceViewModel? _mainViewModel;
    private AudioManager? _audioManager;
    private MediaSession? _mediaSession;
    private string? _currentProductName;
    private Track? _currentTrack;
#pragma warning restore CA2213 // Disposable fields should be disposed

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
        .WithInterFont();

#pragma warning disable CA2000 // Dispose objects before losing scope
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var mainViewModel = _mainViewModel = (Content as StyledElement)?.DataContext as MainDeviceViewModel
            ?? throw new InvalidOperationException(nameof(MainDeviceViewModel));

        var audioManager = _audioManager = (AudioManager)GetSystemService(AudioService)
            ?? throw new InvalidOperationException(nameof(AudioManager));

        //#pragma warning disable CA1416 // Validate platform compatibility
        //        audioManager.AddOnCommunicationDeviceChangedListener(new Executor(mainViewModel), new OnCommunicationDeviceChangedListener(mainViewModel));
        //#pragma warning restore CA1416 // Validate platform compatibility

        var mediaSession = _mediaSession = new MediaSession(this, "SunrisePlayerMediaSession");
        mediaSession.SetCallback(new MediaCallback(mainViewModel));

        mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
        mediaSession.Active = true;

        var mediaPlayer = mainViewModel.TrackPlay.Player.Media;
        mediaPlayer.StateChanged -= MediaPlayer_StateChanged;
        mediaPlayer.StateChanged += MediaPlayer_StateChanged;
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent? e)
    {
        MediaManager.Execute(_mainViewModel, keyCode);
        return base.OnKeyDown(keyCode, e);
    }

    private void MediaPlayer_StateChanged(object? sender, TrackStateChangedEventArgs e)
    {
        var audioManager = _audioManager;
        var mediaSession = _mediaSession;

        if (audioManager is null || mediaSession is null || !MediaManager.IsBluetoothA2dpOn(audioManager, out string? productName))
            return;

        var (playbackStateCode, durationMilliseconds, positionMilliseconds) = MediaManager.Prepare(e);

        if (_currentProductName != productName || !ReferenceEquals(_currentTrack, e.Track))
        {
            _currentProductName = productName;
            _currentTrack = e.Track;
            MediaManager.SendMediaMetadata(mediaSession, e.Track, durationMilliseconds);
        }

        MediaManager.SendPlaybackState(mediaSession, playbackStateCode, positionMilliseconds);
    }

    protected override void OnDestroy()
    {
        var mainViewModel = _mainViewModel;

        if (mainViewModel is not null)
        {
            mainViewModel.TrackPlay.Player.Media.StateChanged -= MediaPlayer_StateChanged;
            _mainViewModel = null;
        }

        _audioManager = null;
        _currentTrack = null;
        _mediaSession?.Release();
        _mediaSession = null;

        base.OnDestroy();
    }

}

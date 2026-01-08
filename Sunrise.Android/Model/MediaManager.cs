using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Views;
using Sunrise.Model;
using Sunrise.Model.Common;
using Sunrise.ViewModels;
using State = Sunrise.Model.SoundFlow.Enums.PlaybackState;

namespace Sunrise.Android.Model;

internal static class MediaManager
{
    public static (PlaybackStateCode playbackStateCode, long durationMilliseconds, long positionMilliseconds) Prepare(TrackStateChangedEventArgs e)
    {
        var playbackStateCode = e.State is State.Playing ? PlaybackStateCode.Playing : PlaybackStateCode.Stopped;
        double duration = e.Track.Duration.TotalMilliseconds;
        long durationMilliseconds = (long)duration;
        long positionMilliseconds = (long)(e.Position * duration);
        return (playbackStateCode, durationMilliseconds, positionMilliseconds);
    }

#pragma warning disable CA2000 // Dispose objects before losing scope
    public static void SendMediaMetadata(MediaSession mediaSession, Track track, long durationMilliseconds)
    {
        var mediaMetadata = new MediaMetadata.Builder()
            !.PutString(MediaMetadata.MetadataKeyTitle, track.Title)
            !.PutString(MediaMetadata.MetadataKeyArtist, track.Artist)
            !.PutString(MediaMetadata.MetadataKeyAlbum, track.Album)
            !.PutLong(MediaMetadata.MetadataKeyDuration, durationMilliseconds)
            !.Build();

        mediaSession.SetMetadata(mediaMetadata);
    }

    public static void SendPlaybackState(MediaSession mediaSession, PlaybackStateCode playbackStateCode, long positionMilliseconds)
    {
        var playbackState = new PlaybackState.Builder()
            !.SetActions(PlaybackState.ActionPlay)
            !.SetState(playbackStateCode, positionMilliseconds, 1.0f, SystemClock.ElapsedRealtime())
            !.Build();

        mediaSession.SetPlaybackState(playbackState);
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    public static void Execute(MainDeviceViewModel? mainViewModel, Keycode key)
        => Tasks.Execute(ExecuteAsync(mainViewModel, key));

    public static async Task ExecuteAsync(MainDeviceViewModel? mainViewModel, Keycode key)
    {
        if (mainViewModel is null)
            return;

        try
        {
            switch (key)
            {
                case Keycode.MediaPlay:
                    await mainViewModel.TrackPlay.PlayTrackAsync();
                    break;
                case Keycode.MediaPause:
                case Keycode.MediaPlayPause:
                    await mainViewModel.TrackPlay.PlayPauseTrackAsync();
                    break;
                case Keycode.MediaStop:
                    await mainViewModel.TrackPlay.PauseTrackAsync();
                    break;
                case Keycode.MediaNext:
                    await mainViewModel.TrackPlay.GoToNextTrackAsync();
                    break;
                case Keycode.MediaPrevious:
                    await mainViewModel.TrackPlay.GoToPrevTrackAsync();
                    break;
                case Keycode.Back:
                    await mainViewModel.BackAsync();
                    break;
            }
        }
        catch (Exception e)
        {
            mainViewModel.WriteInfo(e);
        }
    }

    public static T? GetParcelableExtra<T>(Intent intent, string? name)
        where T : Java.Lang.Object
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13 (API 33)
#pragma warning disable CA1416 // Validate platform compatibility
            return (T)intent.GetParcelableExtra(name, Java.Lang.Class.FromType(typeof(T)));
#pragma warning restore CA1416 // Validate platform compatibility
        else
#pragma warning disable CA1422 // Validate platform compatibility
            return (T)intent.GetParcelableExtra(name);
#pragma warning restore CA1422 // Validate platform compatibility
    }

#pragma warning disable CA1416 // Validate platform compatibility
    public static bool IsBluetoothA2dpOn(AudioManager audioManager, out string? productName)
    {
        var device = audioManager.GetDevices(GetDevicesTargets.Outputs)?.FirstOrDefault(d => d.Type == AudioDeviceType.BluetoothA2dp);
        productName = device?.ProductName;
        return device is not null;
    }
#pragma warning restore CA1416 // Validate platform compatibility

}

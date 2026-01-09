using System;
using Android.Content;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Sunrise.Model;
using Sunrise.Model.Common;
using State = Sunrise.Model.SoundFlow.Enums.PlaybackState;

namespace Sunrise.Android.Model;

internal static class MediaHelper
{
    public static (PlaybackStateCode playbackStateCode, long durationMilliseconds, long positionMilliseconds) Prepare(TrackStateChangedEventArgs e)
    {
        var playbackStateCode = e.State switch
        {
            State.Playing => PlaybackStateCode.Playing,
            State.Paused => PlaybackStateCode.Paused,
            State.Stopped => PlaybackStateCode.Stopped,
            _ => throw new NotSupportedException(e.State.ToString()),
        };

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
        const long actions = PlaybackState.ActionPlay | PlaybackState.ActionPause | PlaybackState.ActionPlayPause | PlaybackState.ActionStop
            | PlaybackState.ActionSkipToNext | PlaybackState.ActionSkipToPrevious | PlaybackState.ActionSeekTo;

        var playbackState = new PlaybackState.Builder()
            !.SetActions(actions)
            !.SetState(playbackStateCode, positionMilliseconds, 1.0f, SystemClock.ElapsedRealtime())
            !.Build();

        mediaSession.SetPlaybackState(playbackState);
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    public static string? Serialize(Intent intent)
    {
        var builder = CacheStringBuilder.Get()
            .Append("Action: ").Append(intent.Action);

        foreach (string key in intent.Extras?.KeySet() ?? [])
            builder.AppendLine().Append(key).Append(" -> ").Append(intent.Extras?.Get(key));

        return CacheStringBuilder.ToString(builder);
    }

}

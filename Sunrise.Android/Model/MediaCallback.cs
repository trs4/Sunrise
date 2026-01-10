using System;
using Android.Content;
using Android.Media.Session;
using Android.Views;

namespace Sunrise.Android.Model;

internal sealed class MediaCallback : MediaSession.Callback
{
    private const int _rewindSeconds = 10;
    private readonly MediaManager _manager;

    public MediaCallback(MediaManager manager)
        => _manager = manager ?? throw new ArgumentNullException(nameof(manager));

    public override bool OnMediaButtonEvent(Intent mediaButtonIntent)
    {
        if (_manager.MainViewModel.SettingsDisplayed)
            _manager.MainViewModel.WriteInfo($"OnMediaButtonEvent: {MediaHelper.Serialize(mediaButtonIntent)}");

        return base.OnMediaButtonEvent(mediaButtonIntent);
    }

    public override void OnPlay()
        => _manager.Execute(Keycode.MediaPlay);

    public override void OnPause()
        => _manager.Execute(Keycode.MediaPause);

    public override void OnStop()
        => _manager.Execute(Keycode.MediaStop);

    public override void OnSkipToPrevious()
        => _manager.Execute(Keycode.MediaPrevious);

    public override void OnSkipToNext()
        => _manager.Execute(Keycode.MediaNext);

    public override void OnSeekTo(long pos)
    {
        var track = _manager.CurrentTrack;

        if (track is null)
            return;

        double position = pos / track.Duration.TotalMilliseconds;
        _manager.MainViewModel.TrackPlay.ChangePositionDelay(position);
    }

    public override void OnRewind() => Rewind(-_rewindSeconds);
    
    public override void OnFastForward() => Rewind(_rewindSeconds);
    
    private void Rewind(int rewindSeconds)
    {
        var track = _manager.CurrentTrack;

        if (track is null)
            return;

        var trackPlay = _manager.MainViewModel.TrackPlay;
        double position = trackPlay.Position.Add(TimeSpan.FromSeconds(rewindSeconds)).TotalMilliseconds / track.Duration.TotalMilliseconds;
        trackPlay.ChangePositionDelay(position);
    }

}

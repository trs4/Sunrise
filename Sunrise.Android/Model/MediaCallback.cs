using System;
using Android.Content;
using Android.Media.Session;
using Android.Views;

namespace Sunrise.Android.Model;

internal sealed class MediaCallback : MediaSession.Callback
{
    private readonly MediaManager _manager;

    public MediaCallback(MediaManager manager)
        => _manager = manager ?? throw new ArgumentNullException(nameof(manager));

    public override bool OnMediaButtonEvent(Intent mediaButtonIntent)
    {
        if (_manager.MainViewModel.SettingsDisplayed)
            _manager.MainViewModel.WriteInfo($"~OnMediaButton: {MediaHelper.Serialize(mediaButtonIntent)}");

        var keyEvent = MediaHelper.GetParcelableExtra<KeyEvent>(mediaButtonIntent, Intent.ExtraKeyEvent);

        if (keyEvent is not null && keyEvent.Action == KeyEventActions.Down && _manager.Execute(keyEvent.KeyCode))
            return true;
    
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

    public override void OnRewind()
        => _manager.Execute(Keycode.MediaRewind);

    public override void OnFastForward()
        => _manager.Execute(Keycode.MediaFastForward);
}

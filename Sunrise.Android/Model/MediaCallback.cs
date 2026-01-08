using System;
using Android.Content;
using Android.Media.Session;
using Android.Views;
using Sunrise.ViewModels;

namespace Sunrise.Android.Model;

internal sealed class MediaCallback : MediaSession.Callback
{
    private readonly MainDeviceViewModel _mainViewModel;

    public MediaCallback(MainDeviceViewModel mainViewModel)
        => _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

    public override bool OnMediaButtonEvent(Intent mediaButtonIntent)
    {
        var keyEvent = MediaManager.GetParcelableExtra<KeyEvent>(mediaButtonIntent, Intent.ExtraKeyEvent);

        if (keyEvent is not null && keyEvent.Action == KeyEventActions.Down)
            MediaManager.Execute(_mainViewModel, keyEvent.KeyCode);

        return base.OnMediaButtonEvent(mediaButtonIntent);
    }

    public override void OnPause()
        => MediaManager.Execute(_mainViewModel, Keycode.MediaPause);

    public override void OnPlay()
        => MediaManager.Execute(_mainViewModel, Keycode.MediaPlay);

    public override void OnSkipToNext()
        => MediaManager.Execute(_mainViewModel, Keycode.MediaNext);

    public override void OnSkipToPrevious()
        => MediaManager.Execute(_mainViewModel, Keycode.MediaPrevious);

    public override void OnStop()
        => MediaManager.Execute(_mainViewModel, Keycode.MediaStop);
}

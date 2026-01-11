using System;
using Android.Telephony;
using Sunrise.ViewModels;

namespace Sunrise.Android.Model;

internal sealed class MediaTelephonyCallback : TelephonyCallback, TelephonyCallback.ICallStateListener
{
    private readonly MainDeviceViewModel _mainViewModel;
    private CallState _lastCallState;
    private bool _lastPlayingState;

    public MediaTelephonyCallback(MainDeviceViewModel mainViewModel)
        => _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

    public async void OnCallStateChanged(int state)
    {
        var callState = (CallState)state;

        if (_lastCallState == callState)
            return;

        if (_mainViewModel.SettingsDisplayed)
            _mainViewModel.WriteInfo($"~OnCallState: {_lastCallState} -> {callState} lastPlaying:{_lastPlayingState}");

        var trackPlay = _mainViewModel.TrackPlay;

        if (_lastCallState == CallState.Idle) // Начало звонка
        {
            _lastPlayingState = trackPlay.Player.Media.IsPlaying;

            if (_lastPlayingState)
                await trackPlay.PauseTrackAsync();
        }
        else if (callState == CallState.Idle) // Конец звонка
        {
            if (_lastPlayingState)
            {
                _lastPlayingState = false;
                await trackPlay.PlayTrackAsync();
            }
        }

        _lastCallState = callState;
    }

}

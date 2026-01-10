using System;
using Android.Media;
using Android.Views;

namespace Sunrise.Android.Model;

internal sealed class MediaDeviceCallback : AudioDeviceCallback
{
    private readonly MediaManager _manager;

    public MediaDeviceCallback(MediaManager manager)
        => _manager = manager ?? throw new ArgumentNullException(nameof(manager));

    public override void OnAudioDevicesAdded(AudioDeviceInfo[]? addedDevices)
        => _manager.SendCurrentMetadata(); // При включении гарнитуры делаем рассылку метаданных

    public override void OnAudioDevicesRemoved(AudioDeviceInfo[]? removedDevices)
    {
        if (!_manager.IsBluetoothA2dpOn()) // При отключении гарнитуры ставим на паузу трек
            _manager.Execute(Keycode.MediaPause);
    }

}

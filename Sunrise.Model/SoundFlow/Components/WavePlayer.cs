using Sunrise.Model.SoundFlow.Abstracts;
using Sunrise.Model.SoundFlow.Abstracts.Devices;
using Sunrise.Model.SoundFlow.Backends.MiniAudio;
using Sunrise.Model.SoundFlow.Backends.MiniAudio.Devices;
using Sunrise.Model.SoundFlow.Backends.MiniAudio.Enums;
using Sunrise.Model.SoundFlow.Providers;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Components;

public static class WavePlayer
{
    private static readonly AudioEngine Engine = new MiniAudioEngine();
    private static readonly AudioFormat Format = AudioFormat.DvdHq;

    // Represents detailed configuration for a MiniAudio device, allowing fine-grained control over general and backend-specific settings, Not essential though
    private static readonly DeviceConfig DeviceConfig = new MiniAudioDeviceConfig
    {
        PeriodSizeInFrames = 960, // 10ms at 48kHz = 480 frames @ 2 channels = 960 frames
        Playback = new DeviceSubConfig
        {
            ShareMode = ShareMode.Shared // Use shared mode for better compatibility with other applications
        },
        Capture = new DeviceSubConfig
        {
            ShareMode = ShareMode.Shared // Use shared mode for better compatibility with other applications
        },
        Wasapi = new WasapiSettings
        {
            Usage = WasapiUsage.ProAudio // Use ProAudio mode for lower latency on Windows
        }
    };

    private static AudioPlaybackDevice? _playbackDevice;
    private static readonly object _initializeSync = new();

    public static SoundPlayer Create(string filePath)
    {
        var playbackDevice = _playbackDevice ??= Initialize();
        var dataProvider = StreamDataProvider.Create(Engine, Format, filePath);
        var soundPlayer = new SoundPlayer(Engine, Format, dataProvider);
        var mixer = playbackDevice.MasterMixer;

        foreach (var component in mixer.Components)
            mixer.RemoveComponent(component);

        mixer.AddComponent(soundPlayer);
        return soundPlayer;
    }

    private static AudioPlaybackDevice Initialize()
    {
        lock (_initializeSync)
        {
            if (_playbackDevice is not null)
                return _playbackDevice;

            Engine.UpdateAudioDevicesInfo();
            var playbackDevice = Engine.InitializePlaybackDevice(GetDevice(), Format, DeviceConfig);
            playbackDevice.Start();
            return playbackDevice;
        }
    }

    private static DeviceInfo GetDevice()
    {
        var devices = Engine.PlaybackDevices;

        foreach (var device in devices)
        {
            if (device.IsDefault)
                return device;
        }

        return devices.FirstOrDefault();
    }

}

using Sunrise.Model.SoundFlow.Abstracts.Devices;

namespace Sunrise.Model.SoundFlow.Structs.Events;

/// <summary>
/// Provides data for device-related events.
/// </summary>
public class DeviceEventArgs(AudioDevice device) : EventArgs
{
    /// <summary>
    /// Gets the audio device that the event is about.
    /// </summary>
    public AudioDevice Device { get; } = device;
}
using Sunrise.Model.SoundFlow.Midi.Structs;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Midi.Interfaces;

/// <summary>
/// Defines an interface for components that can be controlled by MIDI messages.
/// </summary>
public interface IMidiControllable
{
    /// <summary>
    /// Processes an incoming MIDI message to control the state of the component.
    /// </summary>
    /// <param name="message">The MIDI message to process.</param>
    void ProcessMidiMessage(MidiMessage message);
}
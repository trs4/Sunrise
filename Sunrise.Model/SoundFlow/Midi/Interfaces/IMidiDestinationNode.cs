using Sunrise.Model.SoundFlow.Midi.Structs;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Midi.Interfaces;

/// <summary>
/// Represents a destination for MIDI messages within the MIDI routing graph.
/// </summary>
public interface IMidiDestinationNode : IMidiControllable
{
    /// <summary>
    /// Gets a user-friendly name for the destination node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Processes an incoming MIDI message.
    /// </summary>
    /// <param name="message">The MIDI message to process.</param>
    Result ProcessMessage(MidiMessage message);
}
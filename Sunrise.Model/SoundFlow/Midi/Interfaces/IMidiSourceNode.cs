using Sunrise.Model.SoundFlow.Midi.Structs;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Midi.Interfaces;

/// <summary>
/// Represents a source of MIDI messages within the MIDI routing graph.
/// </summary>
public interface IMidiSourceNode
{
    /// <summary>
    /// Gets a user-friendly name for the source node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Occurs when this node generates a MIDI channel message.
    /// </summary>
    event Action<MidiMessage> OnMessageOutput;

    /// <summary>
    /// Occurs when this node generates a System Exclusive (SysEx) message.
    /// </summary>
    event Action<byte[]> OnSysExOutput;
}
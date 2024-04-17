using System.Runtime.InteropServices;

namespace Sunrise.Model.Audio.Wave;

/// <summary>GSM 610</summary>
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public class Gsm610WaveFormat : WaveFormat
{
    /// <summary>
    /// Creates a GSM 610 WaveFormat
    /// For now hardcoded to 13kbps
    /// </summary>
    public Gsm610WaveFormat()
    {
        _waveFormatTag = WaveFormatEncoding.Gsm610;
        _channels = 1;
        _averageBytesPerSecond = 1625;
        _bitsPerSample = 0; // must be zero
        _blockAlign = 65;
        _sampleRate = 8000;

        _extraSize = 2;
        SamplesPerBlock = 320;
    }

    /// <summary>Samples per block</summary>
    public short SamplesPerBlock { get; }

    /// <summary>Writes this structure to a BinaryWriter</summary>
    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(SamplesPerBlock);
    }

}

using System.Runtime.InteropServices;

namespace Sunrise.Model.Audio.Wave;

/// <summary>
/// WaveFormatExtensible
/// http://www.microsoft.com/whdc/device/audio/multichaud.mspx
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
public class WaveFormatExtensible : WaveFormat
{
    private readonly short _wValidBitsPerSample; // bits of precision, or is wSamplesPerBlock if wBitsPerSample==0
    private readonly int _dwChannelMask; // which channels are present in stream

    /// <summary>Parameterless constructor for marshalling</summary>
    private WaveFormatExtensible() { }

    /// <summary>Creates a new WaveFormatExtensible for PCM or IEEE</summary>
    public WaveFormatExtensible(int rate, int bits, int channels)
        : base(rate, bits, channels)
    {
        _waveFormatTag = WaveFormatEncoding.Extensible;
        _extraSize = 22;
        _wValidBitsPerSample = (short)bits;

        for (int n = 0; n < channels; n++)
            _dwChannelMask |= (1 << n);

        if (bits == 32)
            SubFormat = AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT; // KSDATAFORMAT_SUBTYPE_IEEE_FLOAT
        else
            SubFormat = AudioMediaSubtypes.MEDIASUBTYPE_PCM; // KSDATAFORMAT_SUBTYPE_PCM
    }

    /// <summary>
    /// WaveFormatExtensible for PCM or floating point can be awkward to work with
    /// This creates a regular WaveFormat structure representing the same audio format
    /// Returns the WaveFormat unchanged for non PCM or IEEE float
    /// </summary>
    public WaveFormat ToStandardWaveFormat()
    {
        if (SubFormat == AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT && _bitsPerSample == 32)
            return CreateIeeeFloatWaveFormat(_sampleRate, _channels);

        if (SubFormat == AudioMediaSubtypes.MEDIASUBTYPE_PCM)
            return new WaveFormat(_sampleRate, _bitsPerSample, _channels);

        return this; // throw new InvalidOperationException("Not a recognised PCM or IEEE float format");
    }

    /// <summary>SubFormat (may be one of AudioMediaSubtypes)</summary>
    public Guid SubFormat { get; }

    /// <summary>Serialize</summary>
    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(_wValidBitsPerSample);
        writer.Write(_dwChannelMask);
        byte[] guid = SubFormat.ToByteArray();
        writer.Write(guid, 0, guid.Length);
    }

    /// <summary>String representation</summary>
    public override string ToString()
        => $"WAVE_FORMAT_EXTENSIBLE {AudioMediaSubtypes.GetAudioSubtypeName(SubFormat)} {SampleRate}Hz {Channels} channels {BitsPerSample} bit";
}

using System.Runtime.InteropServices;

namespace Sunrise.Model.Audio.Wave;

/// <summary>Microsoft ADPCM http://icculus.org/SDL_sound/downloads/external_documentation/wavecomp.htm</summary>
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public class AdpcmWaveFormat : WaveFormat
{
    private readonly short _samplesPerBlock;
    private readonly short _numCoeff;

    // 7 pairs of coefficients
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
    private readonly short[] _coefficients;

    /// <summary>Empty constructor needed for marshalling from a pointer</summary>
    private AdpcmWaveFormat() : this(8000, 1) { }

    /// <summary>Samples per block</summary>
    public int SamplesPerBlock => _samplesPerBlock;

    /// <summary>Number of coefficients</summary>
    public int NumCoefficients => _numCoeff;

    /// <summary>Coefficients</summary>
    public short[] Coefficients => _coefficients;

    /// <summary>Microsoft ADPCM</summary>
    /// <param name="sampleRate">Sample Rate</param>
    /// <param name="channels">Channels</param>
    public AdpcmWaveFormat(int sampleRate, int channels)
        : base(sampleRate, 0, channels)
    {
        _waveFormatTag = WaveFormatEncoding.Adpcm;
        _extraSize = 32; // TODO: validate sampleRate, bitsPerSample

        _blockAlign = _sampleRate switch
        {
            8000 or 11025 => 256,
            22050 => 512,
            _ => 1024,
        };

        _bitsPerSample = 4;
        _samplesPerBlock = (short)((((_blockAlign - (7 * channels)) * 8) / (_bitsPerSample * channels)) + 2);
        _averageBytesPerSecond = ((SampleRate * _blockAlign) / _samplesPerBlock);
        // samplesPerBlock = blockAlign - (7 * channels)) * (2 / channels) + 2;
        _numCoeff = 7;
        _coefficients = [256, 0, 512, -256, 0, 0, 192, 64, 240, 0, 460, -208, 392, -232];
    }

    /// <summary>Serializes this wave format</summary>
    /// <param name="writer">Binary writer</param>
    public override void Serialize(System.IO.BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(_samplesPerBlock);
        writer.Write(_numCoeff);

        foreach (short coefficient in _coefficients)
            writer.Write(coefficient);
    }

    /// <summary>String Description of this WaveFormat</summary>
    public override string ToString()
        => string.Format("Microsoft ADPCM {0} Hz {1} channels {2} bits per sample {3} samples per block", SampleRate, _channels, _bitsPerSample, _samplesPerBlock);
}

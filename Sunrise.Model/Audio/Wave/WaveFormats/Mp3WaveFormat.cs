using System.Runtime.InteropServices;

namespace Sunrise.Model.Audio.Wave;

/// <summary>MP3 WaveFormat, MPEGLAYER3WAVEFORMAT from mmreg.h</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
public class Mp3WaveFormat : WaveFormat
{
    /// <summary>Creates a new MP3 WaveFormat</summary>
    public Mp3WaveFormat(int sampleRate, int channels, int blockSize, int bitRate)
    {
        _waveFormatTag = WaveFormatEncoding.MpegLayer3;
        _channels = (short)channels;
        _averageBytesPerSecond = bitRate / 8;
        _bitsPerSample = 0; // must be zero
        _blockAlign = 1; // must be 1
        _sampleRate = sampleRate;

        _extraSize = Mp3WaveFormatExtraBytes;
        Id = Mp3WaveFormatId.Mpeg;
        Flags = Mp3WaveFormatFlags.PaddingIso;
        BlockSize = (ushort)blockSize;
        FramesPerBlock = 1;
        CodecDelay = 0;
    }

    /// <summary>Wave format ID (wID)</summary>
    public Mp3WaveFormatId Id;

    /// <summary>Padding flags (fdwFlags)</summary>
    public Mp3WaveFormatFlags Flags;

    /// <summary>Block Size (nBlockSize)</summary>
    public ushort BlockSize;

    /// <summary>Frames per block (nFramesPerBlock)</summary>
    public ushort FramesPerBlock;

    /// <summary>Codec Delay (nCodecDelay)</summary>
    public ushort CodecDelay;

    private const short Mp3WaveFormatExtraBytes = 12; // MPEGLAYER3_WFX_EXTRA_BYTES
}

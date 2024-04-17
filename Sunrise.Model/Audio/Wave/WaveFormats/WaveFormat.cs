using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sunrise.Model.Audio.Wave;

/// <summary>Represents a Wave file format</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
public class WaveFormat
{
    /// <summary>format type</summary>
    protected WaveFormatEncoding _waveFormatTag;

    /// <summary>number of channels</summary>
    protected short _channels;

    /// <summary>sample rate</summary>
    protected int _sampleRate;

    /// <summary>for buffer estimation</summary>
    protected int _averageBytesPerSecond;

    /// <summary>block size of data</summary>
    protected short _blockAlign;

    /// <summary>number of bits per sample of mono data</summary>
    protected short _bitsPerSample;

    /// <summary>number of following bytes</summary>
    protected short _extraSize;

    /// <summary>Creates a new PCM 44.1Khz stereo 16 bit format</summary>
    public WaveFormat() : this(44100, 16, 2) { }

    /// <summary>Creates a new 16 bit wave format with the specified sample rate and channel count</summary>
    /// <param name="sampleRate">Sample Rate</param>
    /// <param name="channels">Number of channels</param>
    public WaveFormat(int sampleRate, int channels) : this(sampleRate, 16, channels) { }

    /// <summary>Gets the size of a wave buffer equivalent to the latency in milliseconds</summary>
    /// <param name="milliseconds">The milliseconds</param>
    public int ConvertLatencyToByteSize(int milliseconds)
    {
        int bytes = (int)((AverageBytesPerSecond / 1000.0) * milliseconds);

        if ((bytes % BlockAlign) != 0)
            bytes = bytes + BlockAlign - (bytes % BlockAlign); // Return the upper BlockAligned

        return bytes;
    }

    /// <summary>Creates a WaveFormat with custom members</summary>
    /// <param name="tag">The encoding</param>
    /// <param name="sampleRate">Sample Rate</param>
    /// <param name="channels">Number of channels</param>
    /// <param name="averageBytesPerSecond">Average Bytes Per Second</param>
    /// <param name="blockAlign">Block Align</param>
    /// <param name="bitsPerSample">Bits Per Sample</param>
    public static WaveFormat CreateCustomFormat(WaveFormatEncoding tag, int sampleRate, int channels, int averageBytesPerSecond, int blockAlign, int bitsPerSample)
        => new()
        {
            _waveFormatTag = tag,
            _channels = (short)channels,
            _sampleRate = sampleRate,
            _averageBytesPerSecond = averageBytesPerSecond,
            _blockAlign = (short)blockAlign,
            _bitsPerSample = (short)bitsPerSample,
            _extraSize = 0
        };

    /// <summary>Creates an A-law wave format</summary>
    /// <param name="sampleRate">Sample Rate</param>
    /// <param name="channels">Number of Channels</param>
    /// <returns>Wave Format</returns>
    public static WaveFormat CreateALawFormat(int sampleRate, int channels)
        => CreateCustomFormat(WaveFormatEncoding.ALaw, sampleRate, channels, sampleRate * channels, channels, 8);

    /// <summary>Creates a Mu-law wave format</summary>
    /// <param name="sampleRate">Sample Rate</param>
    /// <param name="channels">Number of Channels</param>
    /// <returns>Wave Format</returns>
    public static WaveFormat CreateMuLawFormat(int sampleRate, int channels)
        => CreateCustomFormat(WaveFormatEncoding.MuLaw, sampleRate, channels, sampleRate * channels, channels, 8);

    /// <summary>Creates a new PCM format with the specified sample rate, bit depth and channels</summary>
    public WaveFormat(int rate, int bits, int channels)
    {
        if (channels < 1)
            throw new ArgumentOutOfRangeException(nameof(channels), "Channels must be 1 or greater");
        
        // minimum 16 bytes, sometimes 18 for PCM
        _waveFormatTag = WaveFormatEncoding.Pcm;
        _channels = (short)channels;
        _sampleRate = rate;
        _bitsPerSample = (short)bits;
        _extraSize = 0;

        _blockAlign = (short)(channels * (bits / 8));
        _averageBytesPerSecond = _sampleRate * _blockAlign;
    }

    /// <summary>Creates a new 32 bit IEEE floating point wave format</summary>
    /// <param name="sampleRate">sample rate</param>
    /// <param name="channels">number of channels</param>
    public static WaveFormat CreateIeeeFloatWaveFormat(int sampleRate, int channels)
    {
        var wf = new WaveFormat
        {
            _waveFormatTag = WaveFormatEncoding.IeeeFloat,
            _channels = (short)channels,
            _bitsPerSample = 32,
            _sampleRate = sampleRate,
            _blockAlign = (short)(4 * channels)
        };

        wf._averageBytesPerSecond = sampleRate * wf._blockAlign;
        wf._extraSize = 0;
        return wf;
    }

    /// <summary>Helper function to retrieve a WaveFormat structure from a pointer</summary>
    /// <param name="pointer">WaveFormat structure</param>
    public static WaveFormat MarshalFromPtr(IntPtr pointer)
    {
        WaveFormat waveFormat = Marshal.PtrToStructure<WaveFormat>(pointer) ?? throw new InvalidOperationException(nameof(WaveFormat));

        switch (waveFormat.Encoding)
        {
            case WaveFormatEncoding.Pcm:
                // can't rely on extra size even being there for PCM so blank it to avoid reading
                // corrupt data
                waveFormat._extraSize = 0;
                break;
            case WaveFormatEncoding.Extensible:
                waveFormat = Marshal.PtrToStructure<WaveFormatExtensible>(pointer) ?? throw new InvalidOperationException(nameof(WaveFormatExtensible));
                break;
            case WaveFormatEncoding.Adpcm:
                waveFormat = Marshal.PtrToStructure<AdpcmWaveFormat>(pointer) ?? throw new InvalidOperationException(nameof(AdpcmWaveFormat));
                break;
            case WaveFormatEncoding.Gsm610:
                waveFormat = Marshal.PtrToStructure<Gsm610WaveFormat>(pointer) ?? throw new InvalidOperationException(nameof(Gsm610WaveFormat));
                break;
            default:
                if (waveFormat.ExtraSize > 0)
                    waveFormat = Marshal.PtrToStructure<WaveFormatExtraData>(pointer) ?? throw new InvalidOperationException(nameof(WaveFormatExtraData));
                
                break;
        }

        return waveFormat;
    }

    /// <summary>Helper function to marshal WaveFormat to an IntPtr</summary>
    /// <param name="format">WaveFormat</param>
    /// <returns>IntPtr to WaveFormat structure (needs to be freed by callee)</returns>
    public static IntPtr MarshalToPtr(WaveFormat format)
    {
        int formatSize = Marshal.SizeOf(format);
        IntPtr formatPointer = Marshal.AllocHGlobal(formatSize);
        Marshal.StructureToPtr(format, formatPointer, false);
        return formatPointer;
    }

    /// <summary>
    /// Reads in a WaveFormat (with extra data) from a fmt chunk (chunk identifier and
    /// length should already have been read)
    /// </summary>
    /// <param name="br">Binary reader</param>
    /// <param name="formatChunkLength">Format chunk length</param>
    /// <returns>A WaveFormatExtraData</returns>
    public static WaveFormat FromFormatChunk(BinaryReader br, int formatChunkLength)
    {
        var waveFormat = new WaveFormatExtraData();
        waveFormat.ReadWaveFormat(br, formatChunkLength);
        waveFormat.ReadExtraData(br);
        return waveFormat;
    }

    private void ReadWaveFormat(BinaryReader br, int formatChunkLength)
    {
        if (formatChunkLength < 16)
            throw new InvalidDataException("Invalid WaveFormat Structure");

        _waveFormatTag = (WaveFormatEncoding)br.ReadUInt16();
        _channels = br.ReadInt16();
        _sampleRate = br.ReadInt32();
        _averageBytesPerSecond = br.ReadInt32();
        _blockAlign = br.ReadInt16();
        _bitsPerSample = br.ReadInt16();

        if (formatChunkLength > 16)
        {
            _extraSize = br.ReadInt16();

            if (_extraSize != formatChunkLength - 18)
            {
                Debug.WriteLine("Format chunk mismatch");
                _extraSize = (short)(formatChunkLength - 18);
            }
        }
    }

    /// <summary>Reads a new WaveFormat object from a stream</summary>
    /// <param name="br">A binary reader that wraps the stream</param>
    public WaveFormat(BinaryReader br)
    {
        int formatChunkLength = br.ReadInt32();
        ReadWaveFormat(br, formatChunkLength);
    }

    /// <summary>Reports this WaveFormat as a string</summary>
    /// <returns>String describing the wave format</returns>
    public override string ToString() => _waveFormatTag switch
    {
        WaveFormatEncoding.Pcm or WaveFormatEncoding.Extensible
            => $"{_bitsPerSample} bit PCM: {_sampleRate}Hz {_channels} channels", // extensible just has some extra bits after the PCM header
        WaveFormatEncoding.IeeeFloat => $"{_bitsPerSample} bit IEEFloat: {_sampleRate}Hz {_channels} channels",
        _ => _waveFormatTag.ToString(),
    };

    /// <summary>Compares with another WaveFormat object</summary>
    /// <param name="obj">Object to compare to</param>
    /// <returns>True if the objects are the same</returns>
    public override bool Equals(object? obj)
        => obj is WaveFormat other && _waveFormatTag == other._waveFormatTag && _channels == other._channels
        && _sampleRate == other._sampleRate && _averageBytesPerSecond == other._averageBytesPerSecond
        && _blockAlign == other._blockAlign && _bitsPerSample == other._bitsPerSample;

    /// <summary>Provides a Hashcode for this WaveFormat</summary>
    /// <returns>A hashcode</returns>
    public override int GetHashCode() => (int)_waveFormatTag ^ _channels ^ _sampleRate ^ _averageBytesPerSecond ^ _blockAlign ^ _bitsPerSample;

    /// <summary>Returns the encoding type used</summary>
    public WaveFormatEncoding Encoding => _waveFormatTag;

    /// <summary>Writes this WaveFormat object to a stream</summary>
    /// <param name="writer">the output stream</param>
    public virtual void Serialize(BinaryWriter writer)
    {
        writer.Write(18 + _extraSize); // wave format length
        writer.Write((short)Encoding);
        writer.Write((short)Channels);
        writer.Write(SampleRate);
        writer.Write(AverageBytesPerSecond);
        writer.Write((short)BlockAlign);
        writer.Write((short)BitsPerSample);
        writer.Write(_extraSize);
    }

    /// <summary>Returns the number of channels (1=mono, 2=stereo etc)</summary>
    public int Channels => _channels;

    /// <summary>Returns the sample rate (samples per second)</summary>
    public int SampleRate => _sampleRate;

    /// <summary>Returns the average number of bytes used per second</summary>
    public int AverageBytesPerSecond => _averageBytesPerSecond;

    /// <summary>Returns the block alignment</summary>
    public virtual int BlockAlign => _blockAlign;

    /// <summary>
    /// Returns the number of bits per sample (usually 16 or 32, sometimes 24 or 8)
    /// Can be 0 for some codecs
    /// </summary>
    public int BitsPerSample => _bitsPerSample;

    /// <summary>
    /// Returns the number of extra bytes used by this waveformat. Often 0,
    /// except for compressed formats which store extra data after the WAVEFORMATEX header
    /// </summary>
    public int ExtraSize => _extraSize;
}

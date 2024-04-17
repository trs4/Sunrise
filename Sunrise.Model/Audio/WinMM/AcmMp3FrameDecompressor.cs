using Sunrise.Model.Audio.Compression;
using Sunrise.Model.Audio.Wave;

namespace Sunrise.Model.Audio;

/// <summary>MP3 Frame Decompressor using ACM</summary>
public class AcmMp3FrameDecompressor : IMp3FrameDecompressor
{
    private readonly AcmStream _conversionStream;
    private bool _disposed;

    /// <summary>Creates a new ACM frame decompressor</summary>
    /// <param name="sourceFormat">The MP3 source format</param>
    public AcmMp3FrameDecompressor(WaveFormat sourceFormat)
    {
        OutputFormat = AcmStream.SuggestPcmFormat(sourceFormat);

        try
        {
            _conversionStream = new AcmStream(sourceFormat, OutputFormat);
        }
        catch
        {
            _disposed = true;
            throw;
        }
    }

    /// <summary>Output format (PCM)</summary>
    public WaveFormat OutputFormat { get; }

    /// <summary>Decompresses a frame</summary>
    /// <param name="frame">The MP3 frame</param>
    /// <param name="dest">destination buffer</param>
    /// <param name="destOffset">Offset within destination buffer</param>
    /// <returns>Bytes written into destination buffer</returns>
    public int DecompressFrame(Mp3Frame frame, byte[] dest, int destOffset)
    {
        ArgumentNullException.ThrowIfNull(frame, "You must provide a non-null Mp3Frame to decompress");
        Array.Copy(frame.RawData, _conversionStream.SourceBuffer, frame.FrameLength);
        int converted = _conversionStream.Convert(frame.FrameLength, out int sourceBytesConverted);

        if (sourceBytesConverted != frame.FrameLength)
        {
            throw new InvalidOperationException(string.Format("Couldn't convert the whole MP3 frame (converted {0}/{1})",
                sourceBytesConverted, frame.FrameLength));
        }

        Array.Copy(_conversionStream.DestBuffer, 0, dest, destOffset, converted);
        return converted;
    }

    /// <summary>Resets the MP3 Frame Decompressor after a reposition operation</summary>
    public void Reset() => _conversionStream.Reposition();

    /// <summary>Disposes of this MP3 frame decompressor</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _conversionStream?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>Finalizer ensuring that resources get released properly</summary>
    ~AcmMp3FrameDecompressor()
    {
        System.Diagnostics.Debug.Assert(false, "AcmMp3FrameDecompressor Dispose was not called");
        Dispose();
    }

}

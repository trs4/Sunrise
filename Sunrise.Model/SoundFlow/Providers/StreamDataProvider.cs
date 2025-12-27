using Sunrise.Model.SoundFlow.Abstracts;
using Sunrise.Model.SoundFlow.Enums;
using Sunrise.Model.SoundFlow.Interfaces;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Providers;

/// <summary>Provides audio data from a stream</summary>
public sealed class StreamDataProvider : ISoundDataProvider
{
    private readonly Stream _stream;
    private readonly ISoundDecoder _decoder;

    private StreamDataProvider(Stream stream, ISoundDecoder decoder)
    {
        _stream = stream;
        _decoder = decoder;
        SampleRate = decoder.SampleRate;
        decoder.EndOfStreamReached += EndOfStreamReached;
    }

    /// <summary>Initializes a new instance of the <see cref="StreamDataProvider" /> class with a specified format</summary>
    /// <param name="engine">The audio engine instance</param>
    /// <param name="format">The audio format</param>
    /// <param name="filePath">The file to read audio data from</param>
    public static StreamDataProvider Create(AudioEngine engine, AudioFormat format, string filePath)
    {
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        string formatId = Path.GetExtension(filePath).Substring(1).ToLower();
        var decoder = engine.CreateDecoder(stream, formatId, format);
        return new(stream, decoder);
    }

    public int Position { get; private set; }

    public int Length => _decoder.Length;

    public bool CanSeek => _stream.CanSeek;

    public SampleFormat SampleFormat => _decoder.SampleFormat;

    public int SampleRate { get; }

    public bool IsDisposed { get; private set; }

    public event EventHandler<EventArgs>? EndOfStreamReached;

    public event EventHandler<PositionChangedEventArgs>? PositionChanged;

    public int ReadBytes(Span<float> buffer)
    {
        if (IsDisposed)
            return 0;

        var count = _decoder.Decode(buffer);
        Position += count;
        PositionChanged?.Invoke(this, new PositionChangedEventArgs(Position));
        return count;
    }

    public void Seek(int sampleOffset)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (!CanSeek)
            throw new InvalidOperationException("Seeking is not supported for this stream");

        if (sampleOffset < 0 || (Length > 0 && sampleOffset > Length))
            throw new ArgumentOutOfRangeException(nameof(sampleOffset), "Seek position is outside the valid range");

        _decoder.Seek(sampleOffset);
        Position = sampleOffset;
        PositionChanged?.Invoke(this, new PositionChangedEventArgs(Position));
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        _decoder.EndOfStreamReached -= EndOfStreamReached;
        _decoder.Dispose();
        _stream.Dispose();
        IsDisposed = true;
    }

}
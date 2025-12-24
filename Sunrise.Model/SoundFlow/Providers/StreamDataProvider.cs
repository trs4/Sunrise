using Sunrise.Model.SoundFlow.Abstracts;
using Sunrise.Model.SoundFlow.Components;
using Sunrise.Model.SoundFlow.Enums;
using Sunrise.Model.SoundFlow.Interfaces;
using Sunrise.Model.SoundFlow.Metadata;
using Sunrise.Model.SoundFlow.Metadata.Models;
using Sunrise.Model.SoundFlow.Structs;
using Sunrise.Model.SoundFlow.Utils;

namespace Sunrise.Model.SoundFlow.Providers;

/// <summary>
///     Provides audio data from a stream.
/// </summary>
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamDataProvider" /> class by automatically detecting the format.
    ///     It first attempts to read metadata; if that fails, it falls back to probing the stream with all available codecs.
    /// </summary>
    /// <param name="engine">The audio engine instance.</param>
    /// <param name="stream">The stream to read audio data from. Must be readable and seekable.</param>
    /// <param name="options">Optional configuration for metadata reading.</param>
    public static async Task<StreamDataProvider> CreateAsync(AudioEngine engine, Stream stream, ReadOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        SoundFormatInfo formatInfo;
        ISoundDecoder decoder;
        options ??= new ReadOptions();

        var formatInfoResult = await SoundMetadataReader.ReadAsync(stream, options, leaveOpen: true);

        if (formatInfoResult is { IsSuccess: true, Value: not null })
        {
            // Path 1: Metadata read successfully. Use it to create the decoder.
            formatInfo = formatInfoResult.Value;
            var discoveredFormat = new AudioFormat
            {
                Format = formatInfo.BitsPerSample > 0 ? formatInfo.BitsPerSample.GetSampleFormatFromBitsPerSample() : SampleFormat.F32,
                Channels = formatInfo.ChannelCount,
                Layout = AudioFormat.GetLayoutFromChannels(formatInfo.ChannelCount),
                SampleRate = formatInfo.SampleRate
            };

            stream.Position = 0;
            decoder = engine.CreateDecoder(stream, formatInfo.FormatIdentifier, discoveredFormat);
        }
        else
        {
            // Path 2: Metadata read failed. Fall back to probing with codecs.
            stream.Position = 0;
            decoder = engine.CreateDecoder(stream, out var detectedFormat);

            // Create a basic FormatInfo from what the decoder found.
            formatInfo = new SoundFormatInfo
            {
                FormatName = "Unknown (Probed)",
                FormatIdentifier = "unknown",
                ChannelCount = detectedFormat.Channels,
                SampleRate = detectedFormat.SampleRate,
                Duration = decoder.Length > 0 && detectedFormat.SampleRate > 0
                    ? TimeSpan.FromSeconds((double)decoder.Length / (detectedFormat.SampleRate * detectedFormat.Channels))
                    : TimeSpan.Zero
            };
        }

        return new(stream, decoder);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamDataProvider" /> class with a specified format.
    /// </summary>
    /// <param name="engine">The audio engine instance.</param>
    /// <param name="format">The audio format.</param>
    /// <param name="stream">The stream to read audio data from.</param>
    public static async Task<StreamDataProvider> CreateAsync(AudioEngine engine, AudioFormat format, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        SoundFormatInfo formatInfo;
        ISoundDecoder decoder;

        var options = new ReadOptions
        {
            ReadTags = false, 
            ReadAlbumArt = false, 
            DurationAccuracy = DurationAccuracy.FastEstimate
        };

        var formatInfoResult = await SoundMetadataReader.ReadAsync(stream, options, leaveOpen: true);
        
        if (formatInfoResult is { IsSuccess: true, Value: not null })
        {
            // Path 1: Metadata read successfully. Use the discovered formatId with the user's target format.
            formatInfo = formatInfoResult.Value;
            stream.Position = 0;
            decoder = engine.CreateDecoder(stream, formatInfo.FormatIdentifier, format);
        }
        else
        {
            // Path 2: Metadata read failed. Fall back to probing, providing the user's format as a hint.
            stream.Position = 0;
            decoder = engine.CreateDecoder(stream, out var detectedFormat, format);

            // Create a basic FormatInfo from what the decoder found.
            formatInfo = new SoundFormatInfo
            {
                FormatName = "Unknown (Probed)",
                FormatIdentifier = "unknown",
                ChannelCount = detectedFormat.Channels,
                SampleRate = detectedFormat.SampleRate,
                Duration = decoder.Length > 0 && detectedFormat.SampleRate > 0
                    ? TimeSpan.FromSeconds((double)decoder.Length / (detectedFormat.SampleRate * detectedFormat.Channels))
                    : TimeSpan.Zero
            };
        }

        return new(stream, decoder);
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

    /// <inheritdoc />
    public int Position { get; private set; }

    /// <inheritdoc />
    public int Length => _decoder.Length > 0 || FormatInfo == null
        ? _decoder.Length
        : (int)(FormatInfo.Duration.TotalSeconds * FormatInfo.SampleRate * FormatInfo.ChannelCount);

    /// <inheritdoc />
    public bool CanSeek => _stream.CanSeek;

    /// <inheritdoc />
    public SampleFormat SampleFormat => _decoder.SampleFormat;

    /// <inheritdoc />
    public int SampleRate { get; }

    /// <inheritdoc />
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public SoundFormatInfo? FormatInfo { get; }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? EndOfStreamReached;

    /// <inheritdoc />
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;

    /// <inheritdoc />
    public int ReadBytes(Span<float> buffer)
    {
        if (IsDisposed) return 0;
        var count = _decoder.Decode(buffer);
        Position += count;
        PositionChanged?.Invoke(this, new PositionChangedEventArgs(Position));
        return count;
    }

    /// <inheritdoc />
    public void Seek(int sampleOffset)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (!CanSeek)
            throw new InvalidOperationException("Seeking is not supported for this stream.");

        if (sampleOffset < 0 || (Length > 0 && sampleOffset > Length))
            throw new ArgumentOutOfRangeException(nameof(sampleOffset), "Seek position is outside the valid range.");

        _decoder.Seek(sampleOffset);
        Position = sampleOffset;

        PositionChanged?.Invoke(this, new PositionChangedEventArgs(Position));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDisposed) return;
        _decoder.EndOfStreamReached -= EndOfStreamReached;
        _decoder.Dispose();
        _stream.Dispose();
        IsDisposed = true;
    }
}
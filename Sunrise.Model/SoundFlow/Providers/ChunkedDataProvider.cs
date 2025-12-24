using System.Buffers;
using Sunrise.Model.SoundFlow.Abstracts;
using Sunrise.Model.SoundFlow.Enums;
using Sunrise.Model.SoundFlow.Interfaces;
using Sunrise.Model.SoundFlow.Metadata;
using Sunrise.Model.SoundFlow.Metadata.Models;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Providers;

/// <summary>
///     Provides audio data from a file or stream by reading in chunks.
/// </summary>
/// <remarks>
///     Efficiently handles large audio files by reading and decoding audio data in manageable chunks.
/// </remarks>
public sealed class ChunkedDataProvider : ISoundDataProvider
{
    private const int DefaultChunkSize = 220500; // ~5 seconds at 44.1 kHz stereo

    private readonly Stream _stream;
    private readonly ISoundDecoder _decoder;
    private readonly int _chunkSize;

    private readonly Queue<float> _buffer = new();
    private bool _isEndOfStream;
    private int _samplePosition;

    private readonly object _lock = new();

    private ChunkedDataProvider(Stream stream, ISoundDecoder decoder, int chunkSize)
    {
        _stream = stream;
        _decoder = decoder;
        _chunkSize = chunkSize;
        SampleFormat = decoder.SampleFormat;
        SampleRate = decoder.SampleRate;
        CanSeek = stream.CanSeek;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChunkedDataProvider" /> class by automatically detecting the format.
    ///     If metadata reading fails, it will attempt to probe the stream with registered codecs.
    /// </summary>
    /// <param name="engine">The audio engine instance.</param>
    /// <param name="stream">The stream to read audio data from. Must be readable and seekable.</param>
    /// <param name="options">Optional configuration for metadata reading.</param>
    /// <param name="chunkSize">The number of samples per channel to read in each chunk.</param>
    public static async Task<ChunkedDataProvider> CreateAsync(AudioEngine engine, Stream stream, ReadOptions? options = null, int chunkSize = DefaultChunkSize)
    {
        ArgumentNullException.ThrowIfNull(stream);
        SoundFormatInfo formatInfo;
        ISoundDecoder decoder;
        options ??= new ReadOptions();

        var formatInfoResult = await SoundMetadataReader.ReadAsync(stream, options, leaveOpen: true);
        AudioFormat discoveredFormat;

        if (formatInfoResult is { IsSuccess: true, Value: not null })
        {
            formatInfo = formatInfoResult.Value;
            discoveredFormat = new AudioFormat
            {
                Format = SampleFormat.F32,
                Channels = formatInfo.ChannelCount,
                Layout = AudioFormat.GetLayoutFromChannels(formatInfo.ChannelCount),
                SampleRate = formatInfo.SampleRate
            };
            stream.Position = 0;
            decoder = engine.CreateDecoder(stream, formatInfo.FormatIdentifier, discoveredFormat);
        }
        else
        {
            stream.Position = 0;
            decoder = engine.CreateDecoder(stream, out var detectedFormat);
            discoveredFormat = detectedFormat;
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

        chunkSize = chunkSize > 0 ? chunkSize * discoveredFormat.Channels : throw new ArgumentOutOfRangeException(nameof(chunkSize));
        //SampleFormat = _decoder.SampleFormat;
        //SampleRate = _decoder.SampleRate;
        //CanSeek = _stream.CanSeek;

        var reader = new ChunkedDataProvider(stream, decoder, chunkSize);
        reader.FillBuffer();
        return reader;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChunkedDataProvider" /> class with a specified format.
    ///     If metadata reading fails, it will attempt to probe the stream with registered codecs.
    /// </summary>
    /// <param name="engine">The audio engine instance.</param>
    /// <param name="format">The audio format containing channels and sample rate and sample format</param>
    /// <param name="stream">The stream to read audio data from.</param>
    /// <param name="chunkSize">The number of samples to read in each chunk.</param>
    public static async Task<ChunkedDataProvider> CreateAsync(AudioEngine engine, AudioFormat format, Stream stream, int chunkSize = DefaultChunkSize)
    {
        ArgumentNullException.ThrowIfNull(stream);
        SoundFormatInfo formatInfo;
        ISoundDecoder decoder;
        chunkSize = chunkSize > 0 ? chunkSize * format.Channels : throw new ArgumentOutOfRangeException(nameof(chunkSize));

        var options = new ReadOptions
        {
            ReadTags = false,
            ReadAlbumArt = false,
            DurationAccuracy = DurationAccuracy.FastEstimate
        };

        var formatInfoResult = await SoundMetadataReader.ReadAsync(stream, options, leaveOpen: true);

        if (formatInfoResult is { IsSuccess: true, Value: not null })
        {
            formatInfo = formatInfoResult.Value;
            stream.Position = 0;
            decoder = engine.CreateDecoder(stream, formatInfo.FormatIdentifier, format);
        }
        else
        {
            stream.Position = 0;
            decoder = engine.CreateDecoder(stream, out var detectedFormat, format);
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

        //SampleFormat = _decoder.SampleFormat;
        //SampleRate = _decoder.SampleRate;
        //CanSeek = _stream.CanSeek;

        var reader = new ChunkedDataProvider(stream, decoder, chunkSize);
        reader.FillBuffer();
        return reader;
    }

    /// <inheritdoc />
    public int Position
    {
        get
        {
            lock (_lock)
            {
                return _samplePosition;
            }
        }
    }

    /// <inheritdoc />
    public int Length => FormatInfo != null ? (int)(FormatInfo.Duration.TotalSeconds * SampleRate * FormatInfo.ChannelCount) : _decoder.Length;

    /// <inheritdoc />
    public bool CanSeek { get; }

    /// <inheritdoc />
    public SampleFormat SampleFormat { get; }

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
        var samplesRead = 0;

        lock (_lock)
        {
            while (samplesRead < buffer.Length)
            {
                if (_buffer.Count == 0)
                {
                    if (_isEndOfStream)
                    {
                        // End of stream reached
                        EndOfStreamReached?.Invoke(this, EventArgs.Empty);
                        break;
                    }

                    // Fill buffer with more data
                    FillBuffer();
                    if (_buffer.Count == 0)
                    {
                        // No more data to read
                        _isEndOfStream = true;
                        EndOfStreamReached?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                }

                var toRead = Math.Min(buffer.Length - samplesRead, _buffer.Count);
                for (var i = 0; i < toRead; i++)
                {
                    buffer[samplesRead++] = _buffer.Dequeue();
                }
            }

            _samplePosition += samplesRead;
            PositionChanged?.Invoke(this, new PositionChangedEventArgs(_samplePosition));
        }

        return samplesRead;
    }

    /// <inheritdoc />
    public void Seek(int sampleOffset)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (!CanSeek) throw new NotSupportedException("Seeking is not supported on the underlying stream or decoder.");

        lock (_lock)
        {
            var maxLen = Length;
            if (maxLen > 0)
            {
                sampleOffset = Math.Clamp(sampleOffset, 0, maxLen);
            }

            _decoder.Seek(sampleOffset);

            // Clear the existing buffer
            _buffer.Clear();
            _isEndOfStream = false;

            // Update the sample position
            _samplePosition = sampleOffset;

            PositionChanged?.Invoke(this, new PositionChangedEventArgs(_samplePosition));

            // Refill the buffer from the new position
            FillBuffer();
        }
    }

    private void FillBuffer()
    {
        if (IsDisposed || _isEndOfStream) return;

        var buffer = ArrayPool<float>.Shared.Rent(_chunkSize);

        try
        {
            var samplesRead = _decoder.Decode(buffer.AsSpan(0, _chunkSize));

            if (samplesRead > 0)
            {
                for (var i = 0; i < samplesRead; i++)
                {
                    _buffer.Enqueue(buffer[i]);
                }
            }
            else
            {
                _isEndOfStream = true;
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(buffer);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDisposed) return;

        lock (_lock)
        {
            _decoder.Dispose();
            _stream.Dispose();
            _buffer.Clear();
            IsDisposed = true;
        }
    }
}
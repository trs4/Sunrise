using Sunrise.Model.SoundFlow.Abstracts;
using Sunrise.Model.SoundFlow.Enums;
using Sunrise.Model.SoundFlow.Interfaces;
using Sunrise.Model.SoundFlow.Metadata;
using Sunrise.Model.SoundFlow.Metadata.Models;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Providers;

/// <summary>
///     Provides audio data from a file or stream.
/// </summary>
/// <remarks>Loads full audio directly to memory.</remarks>
public sealed class AssetDataProvider : ISoundDataProvider
{
    private readonly float[] _data;
    private int _samplePosition;

    private AssetDataProvider(SampleFormat sampleFormat, int sampleRate, float[] data)
    {
        SampleFormat = sampleFormat;
        SampleRate = sampleRate;
        Length = data.Length;
        _data = data;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetDataProvider" /> class by reading from a stream and detecting its format.
    ///     If metadata reading fails, it will attempt to probe the stream with registered codecs.
    /// </summary>
    /// <param name="engine">The audio engine instance.</param>
    /// <param name="stream">The stream to read audio data from.</param>
    /// <param name="options">Optional configuration for metadata reading.</param>
    public static async Task<AssetDataProvider> CreateAsync(AudioEngine engine, Stream stream, ReadOptions? options = null)
    {
        SoundFormatInfo formatInfo;
        //float[] data;
        options ??= new ReadOptions();
        
        var formatInfoResult = await SoundMetadataReader.ReadAsync(stream, options, leaveOpen: true);
        ISoundDecoder decoder;

        if (formatInfoResult is { IsSuccess: true, Value: not null })
        {
            formatInfo = formatInfoResult.Value;
            var discoveredFormat = new AudioFormat
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

        //SampleFormat = ;
        var data = Decode(decoder, formatInfo);
        decoder.Dispose();
        //SampleRate = ;
        //Length = _data.Length;


        return new(decoder.SampleFormat, formatInfo.SampleRate, data);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetDataProvider" /> class with a specified format.
    ///     If metadata reading fails, it will attempt to probe the stream with registered codecs.
    /// </summary>
    /// <param name="engine">The audio engine instance.</param>
    /// <param name="format">The audio format containing channels and sample rate and sample format</param>
    /// <param name="stream">The stream to read audio data from.</param>
    public static async Task<AssetDataProvider> CreateAsync(AudioEngine engine, AudioFormat format, Stream stream)
    {
        SoundFormatInfo formatInfo;
        //float[] data;

        var options = new ReadOptions
        {
            ReadTags = false, 
            ReadAlbumArt = false, 
            DurationAccuracy = DurationAccuracy.FastEstimate
        };

        var formatInfoResult = await SoundMetadataReader.ReadAsync(stream, options, leaveOpen: true);
        ISoundDecoder decoder;
        
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

        //SampleFormat = decoder.SampleFormat;
        var data = Decode(decoder, formatInfo);
        decoder.Dispose();
        //SampleRate = format.SampleRate;
        //Length = _data.Length;


        return new(decoder.SampleFormat, formatInfo.SampleRate, data);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetDataProvider" /> class from a byte array.
    /// </summary>
    /// <param name="engine">The audio engine instance.</param>
    /// <param name="data">The byte array containing the audio file data.</param>
    /// <param name="options">Optional configuration for metadata reading.</param>
    public static Task<AssetDataProvider> CreateAsync(AudioEngine engine, byte[] data, ReadOptions? options = null)
        => CreateAsync(engine, new MemoryStream(data), options);

    /// <inheritdoc />
    public int Position => _samplePosition;

    /// <inheritdoc />
    public int Length { get; } // Length in samples

    /// <inheritdoc />
    public bool CanSeek => true;

    /// <inheritdoc />
    public SampleFormat SampleFormat { get; private set; }
    
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
        var samplesToRead = Math.Min(buffer.Length, _data.Length - _samplePosition);
        if (samplesToRead <= 0)
        {
            EndOfStreamReached?.Invoke(this, EventArgs.Empty);
            return 0;
        }
        
        _data.AsSpan(_samplePosition, samplesToRead).CopyTo(buffer);
        _samplePosition += samplesToRead;
        PositionChanged?.Invoke(this, new PositionChangedEventArgs(_samplePosition));

        return samplesToRead;
    }

    /// <inheritdoc />
    public void Seek(int sampleOffset)
    {
        _samplePosition = Math.Clamp(sampleOffset, 0, _data.Length);
        PositionChanged?.Invoke(this, new PositionChangedEventArgs(_samplePosition));
    }

    private static float[] Decode(ISoundDecoder decoder, SoundFormatInfo? formatInfo)
    {
        var length = decoder.Length > 0 || formatInfo == null
            ? decoder.Length 
            : (int)(formatInfo.Duration.TotalSeconds * formatInfo.SampleRate * formatInfo.ChannelCount);

        return length > 0 ? DecodeKnownLength(decoder, length) : DecodeUnknownLength(decoder);
    }

    private static float[] DecodeKnownLength(ISoundDecoder decoder, int length)
    {
        var samples = new float[length];
        var read = decoder.Decode(samples);
        if (read < length)
        {
            // If fewer samples were read than expected, resize the array to the actual count.
            Array.Resize(ref samples, read);
        }
        return samples;
    }

    private static float[] DecodeUnknownLength(ISoundDecoder decoder)
    {
        const int blockSize = 22050; // Approx 0.5s at 44.1kHz stereo
        var blocks = new List<float[]>();
        var totalSamples = 0;
        
        while(true)
        {
            var block = new float[blockSize * decoder.Channels];
            var samplesRead = decoder.Decode(block);
            if (samplesRead == 0) break;

            if (samplesRead < block.Length)
            {
                Array.Resize(ref block, samplesRead);
            }
            blocks.Add(block);
            totalSamples += samplesRead;
        }

        var samples = new float[totalSamples];
        var offset = 0;
        foreach (var block in blocks)
        {
            block.CopyTo(samples, offset);
            offset += block.Length;
        }
        return samples;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
    }
}
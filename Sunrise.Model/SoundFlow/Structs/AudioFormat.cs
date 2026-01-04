using Sunrise.Model.SoundFlow.Enums;
using Sunrise.Model.SoundFlow.Utils;

namespace Sunrise.Model.SoundFlow.Structs;

/// <summary>Defines the physical or logical arrangement of channels in an audio stream</summary>
public enum ChannelLayout
{
    /// <summary>The channel layout is unknown or not specified</summary>
    Unknown,

    /// <summary>A single audio channel</summary>
    Mono,

    /// <summary>Two audio channels, typically Left and Right</summary>
    Stereo,

    /// <summary>Four audio channels, typically Front Left, Front Right, Rear Left, Rear Right</summary>
    Quad,

    /// <summary>Six audio channels for a 5.1 surround sound setup (L, R, C, LFE, SL, SR)</summary>
    Surround51,

    /// <summary>Eight audio channels for a 7.1 surround sound setup (L, R, C, LFE, SL, SR, SideL, SideR)</summary>
    Surround71
}

/// <summary>
/// Represents the format of an audio stream, including sample format, channel count, and sample rate.
/// This is a record struct, providing value-based equality and a non-nullable value type
/// </summary>
public record struct AudioFormat
{
    /// <summary>Gets or sets the sample format (e.g., S16, F32)</summary>
    public SampleFormat Format;

    /// <summary>Gets or sets the number of audio channels (e.g., 1 for mono, 2 for stereo)</summary>
    public int Channels;

    /// <summary>Gets or sets the physical or logical arrangement of the channels</summary>
    public ChannelLayout Layout;

    /// <summary>Gets or sets the sample rate in Hertz (e.g., 44100, 48000)</summary>
    public int SampleRate;

    /// <summary>Gets the inverse of the sample rate</summary>
    public readonly float InverseSampleRate => 1f / SampleRate;

    /// <summary>Preset for standard Compact Disc (CD) audio</summary>
    /// <remarks>Format: S16, Channels: 2 (Stereo), Layout: Stereo, Sample Rate: 44100 Hz</remarks>
    public static readonly AudioFormat Cd = new()
    {
        Format = SampleFormat.S16,
        Channels = 2,
        Layout = ChannelLayout.Stereo,
        SampleRate = 44100
    };

    /// <summary>Preset for standard DVD-Video audio</summary>
    /// <remarks>Format: S16, Channels: 2 (Stereo), Layout: Stereo, Sample Rate: 48000 Hz</remarks>
    public static readonly AudioFormat Dvd = new()
    {
        Format = SampleFormat.S16,
        Channels = 2,
        Layout = ChannelLayout.Stereo,
        SampleRate = 48000
    };

    /// <summary>Preset for standard DVD-Video audio using 32-bit floating-point samples</summary>
    /// <remarks>Format: F32, Channels: 2 (Stereo), Layout: Stereo, Sample Rate: 48000 Hz</remarks>
    public static readonly AudioFormat DvdHq = new()
    {
        Format = SampleFormat.F32,
        Channels = 2,
        Layout = ChannelLayout.Stereo,
        SampleRate = 48000
    };

    /// <summary>Preset for 5.1 Surround Sound audio</summary>
    /// <remarks>Format: F32, Channels: 6, Layout: Surround51, Sample Rate: 48000 Hz</remarks>
    public static readonly AudioFormat Surround51 = new()
    {
        Format = SampleFormat.F32,
        Channels = 6,
        Layout = ChannelLayout.Surround51,
        SampleRate = 48000
    };

    /// <summary>Preset for 7.1 Surround Sound audio</summary>
    /// <remarks>Format: F32, Channels: 8, Layout: Surround71, Sample Rate: 48000 Hz</remarks>
    public static readonly AudioFormat Surround71 = new()
    {
        Format = SampleFormat.F32,
        Channels = 8,
        Layout = ChannelLayout.Surround71,
        SampleRate = 48000
    };

    /// <summary>Preset for common studio recording (24-bit, 96 kHz)</summary>
    /// <remarks>Format: S24, Channels: 2 (Stereo), Layout: Stereo, Sample Rate: 96000 Hz</remarks>
    public static readonly AudioFormat Studio = new()
    {
        Format = SampleFormat.S24,
        Channels = 2,
        Layout = ChannelLayout.Stereo,
        SampleRate = 96000
    };

    /// <summary>Preset for common studio recording using 32-bit floating-point samples</summary>
    /// <remarks>Format: F32, Channels: 2 (Stereo), Layout: Stereo, Sample Rate: 96000 Hz</remarks>
    public static readonly AudioFormat StudioHq = new()
    {
        Format = SampleFormat.F32,
        Channels = 2,
        Layout = ChannelLayout.Stereo,
        SampleRate = 96000
    };

    /// <summary>Preset for standard broadcast audio (mono)</summary>
    /// <remarks>Format: S16, Channels: 1 (Mono), Layout: Mono, Sample Rate: 48000 Hz. Often used for voice-over</remarks>
    public static readonly AudioFormat Broadcast = new()
    {
        Format = SampleFormat.S16,
        Channels = 1,
        Layout = ChannelLayout.Mono,
        SampleRate = 48000
    };

    /// <summary>Preset for telephony and VoIP audio</summary>
    /// <remarks>Format: U8, Channels: 1 (Mono), Layout: Mono, Sample Rate: 8000 Hz</remarks>
    public static readonly AudioFormat Telephony = new()
    {
        Format = SampleFormat.U8,
        Channels = 1,
        Layout = ChannelLayout.Mono,
        SampleRate = 8000
    };

    /// <summary>Infers an <see cref="AudioFormat"/> from the given stream by reading the stream's metadata</summary>
    /// <param name="filePath">The stream to read the metadata from</param>
    /// <returns>The inferred <see cref="AudioFormat"/></returns>
    public static AudioFormat? GetFormatFromStream(string filePath)
    {
        using var tfile = TagLib.File.Create(filePath);
        var properties = tfile?.Properties;

        if (properties is null)
            return null;

        return new AudioFormat
        {
            Format = properties.BitsPerSample > 0 ? properties.BitsPerSample.GetSampleFormatFromBitsPerSample() : SampleFormat.S16, // Some formats may not have a valid bits per sample value, so we default to S16.
            Channels = properties.AudioChannels,
            Layout = GetLayoutFromChannels(properties.AudioChannels),
            SampleRate = properties.AudioSampleRate,
        };
    }

    /// <summary>Infers the ChannelLayout from a given channel count</summary>
    /// <param name="channels">The number of channels</param>
    /// <returns>The inferred ChannelLayout</returns>
    public static ChannelLayout GetLayoutFromChannels(int channels) => channels switch
    {
        1 => ChannelLayout.Mono,
        2 => ChannelLayout.Stereo,
        4 => ChannelLayout.Quad,
        6 => ChannelLayout.Surround51,
        8 => ChannelLayout.Surround71,
        _ => ChannelLayout.Unknown
    };

}
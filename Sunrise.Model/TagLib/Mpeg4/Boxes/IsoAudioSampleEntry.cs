using System.Globalization;

namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>
///    This class extends <see cref="IsoSampleEntry" /> and implements
///    <see cref="IAudioCodec" /> to provide an implementation of a
///    ISO/IEC 14496-12 AudioSampleEntry and support for reading MPEG-4
///    video properties
/// </summary>
public class IsoAudioSampleEntry : IsoSampleEntry, IAudioCodec
{
    /// <summary>Contains the channel count</summary>
    private readonly ushort _channelCount;

    /// <summary>Contains the sample size</summary>
    private readonly ushort _sampleSize;

    /// <summary>Contains the sample rate</summary>
    private readonly uint _sampleRate;

    /// <summary>Contains the children of the box</summary>
    private readonly List<Box> _children;

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="IsoVisualSampleEntry" /> with a provided header and
    ///    handler by reading the contents from a specified file
    /// </summary>
    /// <param name="header">
    ///    A <see cref="BoxHeader" /> object containing the header
    ///    to use for the new instance
    /// </param>
    /// <param name="file">
    ///    A <see cref="TagLib.File" /> object to read the contents
    ///    of the box from
    /// </param>
    /// <param name="handler">
    ///    A <see cref="IsoHandlerBox" /> object containing the
    ///    handler that applies to the new instance
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///    <paramref name="file" /> is <see langword="null" />
    /// </exception>
    public IsoAudioSampleEntry(BoxHeader header, TagLib.File file, IsoHandlerBox handler)
        : base(header, file, handler)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Seek(base.DataPosition + 8);
        _channelCount = file.ReadBlock(2).ToUShort();
        _sampleSize = file.ReadBlock(2).ToUShort();
        file.Seek(base.DataPosition + 16);
        _sampleRate = file.ReadBlock(4).ToUInt();
        _children = LoadChildren(file);
    }

    /// <summary>
    ///    Gets the position of the data contained in the current
    ///    instance, after any box specific headers
    /// </summary>
    /// <value>
    ///    A <see cref="long" /> value containing the position of
    ///    the data contained in the current instance
    /// </value>
    protected override long DataPosition => base.DataPosition + 20;

    /// <summary>Gets the children of the current instance</summary>
    public override List<Box> Children => _children;

    /// <summary>Gets the duration of the media represented by the current instance</summary>
    /// <value>Always <see cref="TimeSpan.Zero" /></value>
    public TimeSpan Duration => TimeSpan.Zero;

    /// <summary>Gets the types of media represented by the current instance</summary>
    /// <value>Always <see cref="MediaTypes.Video" /></value>
    public MediaTypes MediaTypes => MediaTypes.Audio;

    /// <summary>Gets a text description of the media represented by the current instance</summary>
    /// <value>A <see cref="string" /> object containing a description of the media represented by the current instance</value>
    public string Description => string.Format(CultureInfo.InvariantCulture, "MPEG-4 Audio ({0})", BoxType);

    /// <summary>Gets the bitrate of the audio represented by the current instance</summary>
    /// <value>A <see cref="int" /> value containing a bitrate of the audio represented by the current instance</value>
    public int AudioBitrate
    {
        get
        {
            if (GetChildRecursively("esds") is not AppleElementaryStreamDescriptor esds)
                return 0;

            return (int)esds.AverageBitrate;
        }
    }

    /// <summary>Gets the sample rate of the audio represented by the current instance</summary>
    /// <value>A <see cref="int" /> value containing the sample rate of the audio represented by the current instance</value>
    public int AudioSampleRate => (int)(_sampleRate >> 16);

    /// <summary>Gets the number of channels in the audio represented by the current instance</summary>
    /// <value>A <see cref="int" /> value containing the number of channels in the audio represented by the current instance</value>
    public int AudioChannels => _channelCount;

    /// <summary>Gets the sample size of the audio represented by the current instance</summary>
    /// <value>A <see cref="int" /> value containing the sample size of the audio represented by the current instance</value>
    public int AudioSampleSize => _sampleSize;
}

using System.Globalization;

namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>
///    This class extends <see cref="IsoSampleEntry" /> and implements
///    <see cref="IVideoCodec" /> to provide an implementation of a
///    ISO/IEC 14496-12 VisualSampleEntry and support for reading MPEG-4
///    video properties
/// </summary>
public class IsoVisualSampleEntry : IsoSampleEntry, IVideoCodec
{
    /// <summary>Contains the width of the visual</summary>
    private readonly ushort _width;

    /// <summary>Contains the height of the visual</summary>
    private readonly ushort _height;

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
    public IsoVisualSampleEntry(BoxHeader header, TagLib.File file, IsoHandlerBox handler)
        : base(header, file, handler)
    {
        file.Seek(base.DataPosition + 16);
        _width = file.ReadBlock(2).ToUShort();
        _height = file.ReadBlock(2).ToUShort();
    }

    /// <summary>
    ///    Gets the position of the data contained in the current
    ///    instance, after any box specific headers
    /// </summary>
    /// <value>
    ///    A <see cref="long" /> value containing the position of
    ///    the data contained in the current instance
    /// </value>
    protected override long DataPosition => base.DataPosition + 62;

    /// <summary>Gets the duration of the media represented by the current instance</summary>
    /// <value>Always <see cref="TimeSpan.Zero" /></value>
    public TimeSpan Duration => TimeSpan.Zero;

    /// <summary>Gets the types of media represented by the current instance</summary>
    public MediaTypes MediaTypes => MediaTypes.Video;

    /// <summary>Gets a text description of the media represented by the current instance</summary>
    /// <value>A <see cref="string" /> object containing a description of the media represented by the current instance</value>
    public string Description => string.Format(CultureInfo.InvariantCulture, "MPEG-4 Video ({0})", BoxType);

    /// <summary>Gets the width of the video represented by the current instance</summary>
    /// <value>A <see cref="int" /> containing the width of the video represented by the current instance</value>
    public int VideoWidth => _width;

    /// <summary>Gets the height of the video represented by the current instance</summary>
    /// <value>A <see cref="int" /> containing the height of the video represented by the current instance</value>
    public int VideoHeight => _height;
}
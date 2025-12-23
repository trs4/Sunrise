namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>
///    This class extends <see cref="FullBox" /> to provide an
///    implementation of a ISO/IEC 14496-12 FullBox
/// </summary>
public class IsoHandlerBox : FullBox
{
    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="IsoHandlerBox" /> with a provided header and
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
    public IsoHandlerBox(BoxHeader header, TagLib.File file, IsoHandlerBox handler)
        : base(header, file, handler)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Seek(DataPosition + 4);
        ByteVector box_data = file.ReadBlock(DataSize - 4);
        HandlerType = box_data.Mid(0, 4);
        int end = box_data.Find(0, 16);

        if (end < 16)
            end = box_data.Count;

        Name = box_data.ToString(StringType.UTF8, 16, end - 16);
    }

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="IsoHandlerBox" /> with a specified type and name
    /// </summary>
    /// <param name="handlerType">
    ///    A <see cref="ByteVector" /> object specifying a 4 byte
    ///    handler type
    /// </param>
    /// <param name="name">
    ///    A <see cref="string" /> object specifying the handler
    ///    name
    /// </param>
    public IsoHandlerBox(ByteVector handlerType, string name)
        : base("hdlr", 0, 0)
    {
        ArgumentNullException.ThrowIfNull(handlerType);

        if (handlerType.Count < 4)
            throw new ArgumentException("The handler type must be four bytes long", nameof(handlerType));

        HandlerType = handlerType.Mid(0, 4);
        Name = name;
    }

    /// <summary>Gets the data contained in the current instance</summary>
    /// <value>A <see cref="ByteVector" /> object containing the rendered version of the data contained in the current instance</value>
    public override ByteVector Data => new ByteVector(4)
    {
        HandlerType,
        new ByteVector(12),
        ByteVector.FromString(Name,
        StringType.UTF8),
        new ByteVector(2)
    };

    /// <summary>Gets the handler type of the current instance</summary>
    /// <value>A <see cref="ByteVector" /> object containing the handler type of the current instance</value>
    public ByteVector HandlerType { get; }

    /// <summary>Gets the name of the current instance</summary>
    /// <value>A <see cref="string" /> object containing the name of the current instance</value>
    public string Name { get; }
}

namespace Sunrise.Model.TagLib.Aac;

/// <summary>This class extends <see cref="NonContainer.File" /> to provide tagging and properties support for ADTS AAC audio files.
/// </summary>
[SupportedMimeType("taglib/aac", "aac")]
[SupportedMimeType("audio/aac")]
public class File : NonContainer.File
{
    /// <summary>Contains the first audio header</summary>
    private AudioHeader first_header;

    /// <summary>Constructs and initializes a new instance of <see cref="File" /> for a specified file abstraction and specified read style</summary>
    /// <param name="abstraction">A <see cref="TagLib.File.IFileAbstraction" /> object to use when reading from and writing to the file</param>
    /// <param name="propertiesStyle">
    /// A <see cref="ReadStyle" /> value specifying at what level of accuracy to read the media properties, or <see cref="ReadStyle.None" /> to ignore the properties
    /// </param>
    public File(IFileAbstraction abstraction, ReadStyle propertiesStyle) : base(abstraction, propertiesStyle) { }

    /// <summary>Constructs and initializes a new instance of <see cref="File" /> for a specified file abstraction with an average read style</summary>
    /// <param name="abstraction">A <see cref="TagLib.File.IFileAbstraction" /> object to use when reading from and writing to the file</param>
    public File(IFileAbstraction abstraction) : base(abstraction) { }

    /// <summary>Gets a tag of a specified type from the current instance, optionally creating a new tag if possible</summary>
    /// <param name="type">A <see cref="TagTypes" /> value indicating the type of tag to read</param>
    /// <param name="create">A <see cref="bool" /> value specifying whether or not to try and create the tag if one is not found</param>
    public override Tag? GetTag(TagTypes type, bool create)
    {
        var t = ((NonContainer.Tag)Tag).GetTag(type);

        if (t is not null || !create)
            return t;

        return type switch
        {
            TagTypes.Id3v1 => EndTag.AddTag(type, Tag),
            TagTypes.Id3v2 => StartTag.AddTag(type, Tag),
            TagTypes.Ape => EndTag.AddTag(type, Tag),
            _ => null,
        };
    }

    /// <summary>Reads format specific information at the start of the file</summary>
    /// <param name="start">A <see cref="long" /> value containing the seek position at which the tags end and the media data begins</param>
    /// <param name="propertiesStyle">
    /// A <see cref="ReadStyle" /> value specifying at what level of accuracy to read the media properties,
    /// or <see cref="ReadStyle.None" /> to ignore the properties
    /// </param>
    protected override void ReadStart(long start, ReadStyle propertiesStyle)
    {
        // Only check the first 16 bytes so we're not stuck reading a bad file forever
        if (propertiesStyle != ReadStyle.None && !AudioHeader.Find(out first_header, this, start, 0x4000))
            throw new CorruptFileException("ADTS audio header not found");
    }

    /// <summary>Reads format specific information at the end of the file</summary>
    /// <param name="end">A <see cref="long" /> value containing the seek position at which the media data ends and the tags begin</param>
    /// <param name="propertiesStyle">
    /// A <see cref="ReadStyle" /> value specifying at what level of accuracy to read the media properties, or <see cref="ReadStyle.None" /> to ignore the properties
    /// </param>
    protected override void ReadEnd(long end, ReadStyle propertiesStyle)
    {
        // Make sure we have ID3v1 and ID3v2 tags
        GetTag(TagTypes.Id3v1, true);
        GetTag(TagTypes.Id3v2, true);
    }

    /// <summary>Reads the audio properties from the file represented by the current instance</summary>
    /// <param name="start">A <see cref="long" /> value containing the seek position at which the tags end and the media data begins</param>
    /// <param name="end">A <see cref="long" /> value containing the seek position at which the media data ends and the tags begin</param>
    /// <param name="propertiesStyle">
    /// A <see cref="ReadStyle" /> value specifying at what level of accuracy to read the media properties, or <see cref="ReadStyle.None" /> to ignore the properties
    /// </param>
    protected override Properties ReadProperties(long start, long end, ReadStyle propertiesStyle)
    {
        first_header.SetStreamLength(end - start);
        return new Properties(TimeSpan.Zero, first_header);
    }

}

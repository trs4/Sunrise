namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>This class extends <see cref="Box" /> to provide an implementation of an Apple ItemListBox</summary>
public class AppleItemListBox : Box
{
    /// <summary>Contains the children of the box</summary>
    private readonly List<Box> _children;

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="AppleItemListBox" /> with a provided header and
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
    public AppleItemListBox(BoxHeader header, TagLib.File file, IsoHandlerBox handler)
        : base(header, handler)
    {
        ArgumentNullException.ThrowIfNull(file);
        _children = LoadChildren(file);
    }

    /// <summary>Constructs and initializes a new instance of <see cref="AppleItemListBox" /> with no children</summary>
    public AppleItemListBox()
        : base("ilst")
        => _children = [];

    /// <summary>Gets the children of the current instance</summary>
    public override List<Box> Children => _children;
}

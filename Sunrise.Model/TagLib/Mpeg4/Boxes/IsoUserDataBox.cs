namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>
///    This class extends <see cref="Box" /> to provide an
///    implementation of a ISO/IEC 14496-12 UserDataBox
/// </summary>
public class IsoUserDataBox : Box
{
    /// <summary>Contains the children of the box</summary>
    private readonly IEnumerable<Box> _children;

    /// <summary>Contains the box headers from the top of the file to the current udta box</summary>
    private BoxHeader[] parent_tree;

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="IsoUserDataBox" /> with a provided header and
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
    public IsoUserDataBox(BoxHeader header, TagLib.File file, IsoHandlerBox handler)
        : base(header, handler)
    {
        ArgumentNullException.ThrowIfNull(file);
        _children = LoadChildren(file);
    }

    /// <summary>Constructs and initializes a new instance of <see cref="IsoUserDataBox" /> with no children</summary>
    public IsoUserDataBox()
        : base("udta")
        => _children = [];

    /// <summary>Gets the children of the current instance</summary>
    /// <value>A <see cref="T:System.Collections.Generic.IEnumerable`1" /> object enumerating the children of the current instance</value>
    public override IEnumerable<Box> Children => _children;

    /// <summary>
    ///    Gets the box headers for the current "<c>udta</c>" box and
    ///    all parent boxes up to the top of the file
    /// </summary>
    /// <value>
    ///    A <see cref="BoxHeader[]" /> containing the headers for
    ///    the current "<c>udta</c>" box and its parent boxes up to
    ///    the top of the file, in the order they appear, or <see
    ///    langword="null" /> if none is present
    /// </value>
    public BoxHeader[] ParentTree
    {
        get => parent_tree;
        set => parent_tree = value;
    }

}

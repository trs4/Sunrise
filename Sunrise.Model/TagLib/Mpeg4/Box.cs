namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>This abstract class provides a generic implementation of a ISO/IEC 14496-12 box</summary>
public class Box
{
    /// <summary>Contains the box header</summary>
    private BoxHeader _header;

    /// <summary>Contains the position of the box data</summary>
    private readonly long _dataPosition;

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="Box" /> with a specified header and handler
    /// </summary>
    /// <param name="header">
    ///    A <see cref="BoxHeader" /> object describing the new
    ///    instance
    /// </param>
    /// <param name="handler">
    ///    A <see cref="IsoHandlerBox" /> object containing the
    ///    handler that applies to the new instance, or <see
    ///    langword="null" /> if no handler applies
    /// </param>
    protected Box(BoxHeader header, IsoHandlerBox? handler)
    {
        _header = header;
        _dataPosition = header.Position + header.HeaderSize;
        Handler = handler;
    }

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="Box" /> with a specified header
    /// </summary>
    /// <param name="header">
    ///    A <see cref="BoxHeader" /> object describing the new
    ///    instance
    /// </param>
    protected Box(BoxHeader header) : this(header, null) { }

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="Box" /> with a specified box type
    /// </summary>
    /// <param name="type">
    ///    A <see cref="ByteVector" /> object containing the box
    ///    type to use for the new instance
    /// </param>
    protected Box(ByteVector type) : this(new BoxHeader(type)) { }

    /// <summary>
    ///    Gets the MPEG-4 box type of the current instance
    /// </summary>
    /// <value>
    ///    A <see cref="ByteVector" /> object containing the four
    ///    byte box type of the current instance
    /// </value>
    public virtual ByteVector BoxType => _header.BoxType;

    /// <summary>
    ///    Gets the total size of the current instance as it last
    ///    appeared on disk
    /// </summary>
    /// <value>
    ///    A <see cref="int" /> value containing the total size of
    ///    the current instance as it last appeared on disk
    /// </value>
    public virtual int Size => (int)_header.TotalBoxSize;

    /// <summary>
    ///    Gets and sets the data contained in the current instance
    /// </summary>
    /// <value>
    ///    A <see cref="ByteVector" /> object containing the data
    ///    contained in the current instance
    /// </value>
    public virtual ByteVector? Data
    {
        get => null;
        set { }
    }

    /// <summary>Gets the children of the current instance</summary>
    /// <value>
    ///    A <see cref="T:System.Collections.Generic.IEnumerable`1" /> object enumerating the
    ///    children of the current instance
    /// </value>
    public virtual List<Box>? Children => null;

    /// <summary>Gets the handler box that applies to the current instance</summary>
    /// <value>
    ///    A <see cref="IsoHandlerBox" /> object containing the
    ///    handler that applies to the current instance, or <see
    ///    langword="null" /> if no handler applies
    /// </value>
    public IsoHandlerBox? Handler { get; }

    /// <summary>
    ///    Renders the current instance, including its children, to
    ///    a new <see cref="ByteVector" /> object
    /// </summary>
    /// <returns>
    ///    A <see cref="ByteVector" /> object containing the
    ///    rendered version of the current instance
    /// </returns>
    public ByteVector Render() => Render([]);

    /// <summary>
    ///    Gets a child box from the current instance by finding
    ///    a matching box type
    /// </summary>
    /// <param name="type">
    ///    A <see cref="ByteVector" /> object containing the box
    ///    type to match
    /// </param>
    /// <returns>
    ///    A <see cref="Box" /> object containing the matched box,
    ///    or <see langword="null" /> if no matching box was found
    /// </returns>
    public Box? GetChild(ByteVector type)
    {
        if (Children is null)
            return null;

        foreach (Box box in Children)
        {
            if (box.BoxType == type)
                return box;
        }

        return null;
    }

    /// <summary>
    ///    Gets a child box from the current instance by finding
    ///    a matching box type, searching recursively
    /// </summary>
    /// <param name="type">
    ///    A <see cref="ByteVector" /> object containing the box
    ///    type to match
    /// </param>
    /// <returns>
    ///    A <see cref="Box" /> object containing the matched box,
    ///    or <see langword="null" /> if no matching box was found
    /// </returns>
    public Box? GetChildRecursively(ByteVector type)
    {
        if (Children is null)
            return null;

        foreach (Box box in Children)
        {
            if (box.BoxType == type)
                return box;
        }

        foreach (Box box in Children)
        {
            var childBox = box.GetChildRecursively(type);

            if (childBox is not null)
                return childBox;
        }

        return null;
    }

    /// <summary>Removes all children with a specified box type from the current instance</summary>
    /// <param name="type">A <see cref="ByteVector" /> object containing the box type to remove</param>
    public void RemoveChild(ByteVector type)
    {
        var children = Children;

        if (children is null)
            return;

        foreach (var box in new List<Box>(children))
        {
            if (box.BoxType == type)
                children.Remove(box);
        }
    }

    /// <summary>Removes a specified box from the current instance</summary>
    /// <param name="box">A <see cref="Box" /> object to remove from the current instance</param>
    public void RemoveChild(Box box) => Children?.Remove(box);

    /// <summary>Adds a specified box to the current instance</summary>
    /// <param name="box">A <see cref="Box" /> object to add to the current instance</param>
    public void AddChild(Box box) => Children?.Add(box);

    /// <summary>Removes all children from the current instance</summary>
    public void ClearChildren() => Children?.Clear();

    /// <summary>Gets whether or not the current instance has children</summary>
    /// <value>A <see cref="bool" /> value indicating whether or not the current instance has any children.
    /// 
    public bool HasChildren => Children?.Count > 0;

    /// <summary>
    ///    Gets the size of the data contained in the current
    ///    instance, minux the size of any box specific headers
    /// </summary>
    /// <value>
    ///    A <see cref="long" /> value containing the size of
    ///    the data contained in the current instance
    /// </value>
    protected int DataSize => (int)(_header.DataSize + _dataPosition - DataPosition);

    /// <summary>
    ///    Gets the position of the data contained in the current
    ///    instance, after any box specific headers
    /// </summary>
    /// <value>
    ///    A <see cref="long" /> value containing the position of
    ///    the data contained in the current instance
    /// </value>
    protected virtual long DataPosition => _dataPosition;

    /// <summary>
    ///    Gets the header of the current instance
    /// </summary>
    /// <value>
    ///    A <see cref="BoxHeader" /> object containing the header
    ///    of the current instance
    /// </value>
    protected BoxHeader Header => _header;

    /// <summary>
    ///    Loads the children of the current instance from a
    ///    specified file using the internal data position and size
    /// </summary>
    /// <param name="file">
    ///    The <see cref="TagLib.File" /> from which the current
    ///    instance was read and from which to read the children
    /// </param>
    /// <returns>
    ///    A <see cref="T:System.Collections.Generic.IEnumerable`1" /> object enumerating the
    ///    boxes read from the file
    /// </returns>
    protected List<Box> LoadChildren(TagLib.File file)
    {
        ArgumentNullException.ThrowIfNull(file);
        var children = new List<Box>();
        long position = DataPosition;
        long end = position + DataSize;
        _header.Box = this;

        while (position < end)
        {
            var child = BoxFactory.CreateBox(file, position, _header, Handler, children.Count);
            children.Add(child);
            position += child.Size;
        }

        _header.Box = null;
        return children;
    }

    /// <summary>
    ///    Loads the data of the current instance from a specified
    ///    file using the internal data position and size
    /// </summary>
    /// <param name="file">
    ///    The <see cref="TagLib.File" /> from which the current
    ///    instance was read and from which to read the data
    /// </param>
    /// <returns>
    ///    A <see cref="ByteVector" /> object containing the data
    ///    read from the file
    /// </returns>
    protected ByteVector LoadData(TagLib.File file)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Seek(DataPosition);
        return file.ReadBlock(DataSize);
    }

    /// <summary>
    ///    Renders the current instance, including its children, to
    ///    a new <see cref="ByteVector" /> object, preceeding the
    ///    contents with a specified block of data
    /// </summary>
    /// <param name="topData">
    ///    A <see cref="ByteVector" /> object containing box
    ///    specific header data to preceed the content
    /// </param>
    /// <returns>
    ///    A <see cref="ByteVector" /> object containing the
    ///    rendered version of the current instance
    /// </returns>
    protected virtual ByteVector Render(ByteVector topData)
    {
        bool free_found = false;
        var output = new ByteVector();

        if (Children is not null)
        {
            foreach (Box box in Children)
            {
                if (box.GetType() == typeof(IsoFreeSpaceBox))
                    free_found = true;
                else
                    output.Add(box.Render());
            }
        }
        else if (Data is not null)
            output.Add(Data);

        // If there was a free, don't take it away, and let meta be a special case
        if (free_found || BoxType == Mpeg4.BoxType.Meta)
        {
            long size_difference = DataSize - output.Count;

            // If we have room for free space, add it so we don't have to resize the file
            if (_header.DataSize != 0 && size_difference >= 8)
                output.Add((new IsoFreeSpaceBox(size_difference)).Render());

            // If we're getting bigger, get a lot bigger so we might not have to again
            else
                output.Add(new IsoFreeSpaceBox(2048).Render());
        }

        // Adjust the header's data size to match the content
        _header.DataSize = topData.Count + output.Count;

        // Render the full box.
        output.Insert(0, topData);
        output.Insert(0, _header.Render());

        return output;
    }

}

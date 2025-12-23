namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>This structure provides support for reading and writing headers for ISO/IEC 14496-12 boxes</summary>
public struct BoxHeader
{
    /// <summary>Contains the box size</summary>
    private ulong _boxSize;

    /// <summary>Contains the header size</summary>
    private uint _headerSize;

    /// <summary>Contains the position of the header</summary>
    private readonly long _position;

    /// <summary>Contains the box (temporarily)</summary>
    private Box? _box;

    /// <summary>Indicated that the header was read from a file</summary>
    private readonly bool _fromDisk;

    /// <summary>An empty box header</summary>
    public static readonly BoxHeader Empty = new BoxHeader("xxxx");

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="BoxHeader" /> by reading it from a specified seek
    ///    position in a specified file
    /// </summary>
    /// <param name="file">
    ///    A <see cref="TagLib.File" /> object to read the new
    ///    instance from
    /// </param>
    /// <param name="position">
    ///    A <see cref="long" /> value specifying the seek position
    ///    in <paramref name="file" /> at which to start reading
    /// </param>
    public BoxHeader(TagLib.File file, long position)
    {
        ArgumentNullException.ThrowIfNull(file);
        _box = null;
        _fromDisk = true;
        _position = position;
        file.Seek(position);
        ByteVector data = file.ReadBlock(32);
        int offset = 0;

        if (data.Count < 8 + offset)
            throw new CorruptFileException("Not enough data in box header");

        _headerSize = 8;
        _boxSize = data.Mid(offset, 4).ToUInt();
        BoxType = data.Mid(offset + 4, 4);

        // If the size is 1, that just tells us we have a massive ULONG size waiting for us in the next 8 bytes
        if (_boxSize == 1)
        {
            if (data.Count < 8 + offset)
                throw new CorruptFileException("Not enough data in box header");

            _headerSize += 8;
            _boxSize = data.Mid(offset, 8).ToULong();
            offset += 8;
        }

        // UUID has a special header with 16 extra bytes
        if (BoxType == Mpeg4.BoxType.Uuid)
        {
            if (data.Count < 16 + offset)
                throw new CorruptFileException("Not enough data in box header");

            _headerSize += 16;
            ExtendedType = data.Mid(offset, 16);
        }
        else
            ExtendedType = null;

        if (_boxSize > (ulong)(file.Length - position))
            throw new CorruptFileException("Box header specified a size of {0} bytes but only {1} bytes left in the file");
    }

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="BoxHeader" /> with a specified box type
    /// </summary>
    /// <param name="type">
    ///    A <see cref="ByteVector" /> object containing the four
    ///    byte box type
    /// </param>
    /// <remarks>
    ///    <see cref="BoxHeader(ByteVector,ByteVector)" /> must be
    ///    used to create a header of type "<c>uuid</c>"
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///    <paramref name="type" /> is <see langword="null" /> or is
    ///    equal to "<c>uuid</c>"
    /// </exception>
    /// <exception cref="ArgumentException">
    ///    <paramref name="type" /> isn't exactly 4 bytes long
    /// </exception>
    public BoxHeader(ByteVector type) : this(type, null) { }

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="BoxHeader" /> with a specified box type and
    ///    optionally extended type
    /// </summary>
    /// <param name="type">
    ///    A <see cref="ByteVector" /> object containing the four
    ///    byte box type
    /// </param>
    /// <param name="extendedType">
    ///    A <see cref="ByteVector" /> object containing the four
    ///    byte box type
    /// </param>
    public BoxHeader(ByteVector type, ByteVector? extendedType)
    {
        if (type.Count != 4)
            throw new ArgumentException("Box type must be 4 bytes in length", nameof(type));

        ArgumentNullException.ThrowIfNull(type);
        _position = -1;
        _box = null;
        _fromDisk = false;
        BoxType = type;
        _boxSize = _headerSize = 8;

        if (type != "uuid")
        {
            if (extendedType is not null)
                throw new ArgumentException("Extended type only permitted for 'uuid'", nameof(extendedType));

            ExtendedType = extendedType;
            return;
        }

        ArgumentNullException.ThrowIfNull(extendedType);

        if (extendedType.Count != 16)
            throw new ArgumentException("Extended type must be 16 bytes in length", nameof(extendedType));

        _boxSize = _headerSize = 24;
        ExtendedType = extendedType;
    }

    /// <summary>Gets the type of box represented by the current instance</summary>
    /// <value>A <see cref="ByteVector" /> object containing the 4 byte box type</value>
    public ByteVector BoxType { get; }

    /// <summary>Gets the extended type of the box represented by the current instance</summary>
    /// <value>A <see cref="ByteVector" /> object containing the 16 byte extended type, or <see langword="null" /> if <see cref="BoxType" /> is not "<c>uuid</c>"</value>
    public ByteVector? ExtendedType { get; }

    /// <summary>Gets the size of the header represented by the current instance</summary>
    /// <value>A <see cref="long" /> value containing the size of the header represented by the current instance</value>
    public readonly long HeaderSize => _headerSize;

    /// <summary>
    ///    Gets and sets the size of the data in the box described
    ///    by the current instance
    /// </summary>
    /// <value>
    ///    A <see cref="long" /> value containing the size of the
    ///    data in the box described by the current instance
    /// </value>
    public long DataSize
    {
        get => (long)(_boxSize - _headerSize);
        set => _boxSize = (ulong)value + _headerSize;
    }

    /// <summary>Gets the total size of the box described by the current instance</summary>
    /// <value>A <see cref="long" /> value containing the total size of the box described by the current instance</value>
    public long TotalBoxSize => (long)_boxSize;

    /// <summary>Gets the position box represented by the current instance in the file it comes from</summary>
    /// <value>A <see cref="long" /> value containing the position box represented by the current instance in the file it comes from</value>
    public long Position => _fromDisk ? _position : -1;

    /// <summary>
    ///    Overwrites the header on disk, updating it to include a
    ///    change in the size of the box
    /// </summary>
    /// <param name="file">
    ///    A <see cref="TagLib.File" /> object containing the file
    ///    from which the box originates
    /// </param>
    /// <param name="sizeChange">
    ///    A <see cref="long" /> value indicating the change in the
    ///    size of the box described by the current instance
    /// </param>
    /// <returns>
    ///    The size change encountered by the box that parents the
    ///    box described the the current instance, equal to the
    ///    size change of the box plus any size change that should
    ///    happen in the header
    /// </returns>
    public long Overwrite(TagLib.File file, long sizeChange)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (!_fromDisk)
            throw new InvalidOperationException("Cannot overwrite headers not on disk.");

        long old_header_size = HeaderSize;
        DataSize += sizeChange;
        file.Insert(Render(), _position, old_header_size);
        return sizeChange + HeaderSize - old_header_size;
    }

    /// <summary>Renders the header represented by the current instance</summary>
    /// <returns>A <see cref="ByteVector" /> object containing the rendered version of the current instance</returns>
    public ByteVector Render()
    {
        // Enlarge for size if necessary
        if ((_headerSize == 8 || _headerSize == 24) && _boxSize > uint.MaxValue)
        {
            _headerSize += 8;
            _boxSize += 8;
        }

        // Add the box size and type to the output
        var output = ByteVector.FromUInt((_headerSize == 8 || _headerSize == 24) ? (uint)_boxSize : 1);
        output.Add(BoxType);

        // If the box size is 16 or 32, we must have more a large header to append
        if (_headerSize == 16 || _headerSize == 32)
            output.Add(ByteVector.FromULong(_boxSize));

        // The only reason for such a big size is an extended type. Extend!!!
        if (_headerSize >= 24)
            output.Add(ExtendedType);

        return output;
    }

    /// <summary>Gets and sets the box represented by the current instance as a means of temporary storage for internal uses</summary>
    internal Box? Box
    {
        get => _box;
        set => _box = value;
    }

}

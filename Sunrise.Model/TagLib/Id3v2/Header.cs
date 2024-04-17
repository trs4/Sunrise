namespace Sunrise.Model.TagLib.Id3v2;

public struct Header
{
    public static readonly ReadOnlyByteVector FileIdentifier = "ID3";
    public const uint Size = 10;

    private byte _majorVersion;
    private HeaderFlags _flags;

    public Header(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Count < Size)
            throw new CorruptFileException("Provided data is smaller than object size.");

        if (!data.StartsWith(FileIdentifier))
            throw new CorruptFileException("Provided data does not start with the file identifier");

        _majorVersion = data[3];
        RevisionNumber = data[4];
        _flags = (HeaderFlags)data[5];

        if (_majorVersion == 2 && ((int)_flags & 127) != 0)
            throw new CorruptFileException("Invalid flags set on version 2 tag.");

        if (_majorVersion == 3 && ((int)_flags & 15) != 0)
            throw new CorruptFileException("Invalid flags set on version 3 tag.");

        if (_majorVersion == 4 && ((int)_flags & 7) != 0)
            throw new CorruptFileException("Invalid flags set on version 4 tag.");

        for (int i = 6; i < 10; i++)
        {
            if (data[i] >= 128)
                throw new CorruptFileException("One of the bytes in the header was greater than the allowed 128.");
        }

        TagSize = SynchData.ToUInt(data.Mid(6, 4));
    }

    public byte MajorVersion
    {
        readonly get => _majorVersion == 0 ? Tag.DefaultVersion : _majorVersion;
        set
        {
            if (value < 2 || value > 4)
                throw new ArgumentException("Version unsupported");

            if (value < 3)
                _flags &= ~(HeaderFlags.ExtendedHeader | HeaderFlags.ExperimentalIndicator);

            if (value < 4)
                _flags &= ~HeaderFlags.FooterPresent;

            _majorVersion = value;
        }
    }

    public byte RevisionNumber { get; set; }

    public HeaderFlags Flags
    {
        readonly get => _flags;
        set
        {
            if (0 != (value & (HeaderFlags.ExtendedHeader | HeaderFlags.ExperimentalIndicator)) && MajorVersion < 3)
                throw new ArgumentException("Feature only supported in version 2.3+", nameof(value));

            if (0 != (value & HeaderFlags.FooterPresent) && MajorVersion < 3)
                throw new ArgumentException("Feature only supported in version 2.4+", nameof(value));

            _flags = value;
        }
    }

    public uint TagSize { get; set; }

    public readonly uint CompleteTagSize => (_flags & HeaderFlags.FooterPresent) != 0 ? TagSize + Size + Footer.Size : TagSize + Size;

    public readonly ByteVector Render() => new ByteVector
    {
        FileIdentifier,
        MajorVersion,
        RevisionNumber,
        (byte)_flags,
        SynchData.FromUInt(TagSize),
    };
}

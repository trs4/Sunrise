namespace Sunrise.Model.TagLib.Ape;

public struct Footer : IEquatable<Footer>
{
    public static readonly ReadOnlyByteVector FileIdentifier = "APETAGEX";
    public const uint Size = 32;

    private readonly uint _version;

    public Footer(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Count < Size)
            throw new CorruptFileException("Provided data is smaller than object size.");

        if (!data.StartsWith(FileIdentifier))
            throw new CorruptFileException("Provided data does not start with File Identifier");

        _version = data.Mid(8, 4).ToUInt(false);
        TagSize = data.Mid(12, 4).ToUInt(false);
        ItemCount = data.Mid(16, 4).ToUInt(false);
        Flags = (FooterFlags)data.Mid(20, 4).ToUInt(false);
    }

    public readonly uint Version => _version == 0 ? 2000 : _version;

    public FooterFlags Flags { get; set; }

    public uint ItemCount { get; set; }

    public uint TagSize { get; set; }

    public readonly uint CompleteTagSize => TagSize + ((Flags & FooterFlags.HeaderPresent) != 0 ? Size : 0);

    public readonly ByteVector RenderFooter() => Render(false);

    public readonly ByteVector RenderHeader() => (Flags & FooterFlags.HeaderPresent) != 0 ? Render(true) : [];

    private readonly ByteVector Render(bool isHeader)
    {
        var v = new ByteVector
        {
            FileIdentifier,
            ByteVector.FromUInt(2000, false),
            ByteVector.FromUInt(TagSize, false),
            ByteVector.FromUInt(ItemCount, false)
        };

        uint flags = 0;

        if ((Flags & FooterFlags.HeaderPresent) != 0)
            flags |= (uint)FooterFlags.HeaderPresent;

        if (isHeader)
            flags |= (uint)FooterFlags.IsHeader;
        else
            flags &= (uint)~FooterFlags.IsHeader;

        v.Add(ByteVector.FromUInt(flags, false));
        v.Add(ByteVector.FromULong(0));
        return v;
    }

    public override readonly int GetHashCode()
    {
        unchecked
        {
            return (int)((uint)Flags ^ TagSize ^ ItemCount ^ _version);
        }
    }

    public override readonly bool Equals(object? other) => other is Footer footer && Equals(footer);

    public readonly bool Equals(Footer other) => Flags == other.Flags && TagSize == other.TagSize && ItemCount == other.ItemCount && _version == other._version;

    public static bool operator ==(Footer first, Footer second) => first.Equals(second);

    public static bool operator !=(Footer first, Footer second) => !first.Equals(second);
}

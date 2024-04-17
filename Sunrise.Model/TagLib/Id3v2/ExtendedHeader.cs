namespace Sunrise.Model.TagLib.Id3v2;

public class ExtendedHeader : ICloneable
{
    public ExtendedHeader() { }

    public ExtendedHeader(ByteVector data, byte version)
        => Parse(data, version);

    public uint Size { get; private set; }

    protected void Parse(ByteVector data, byte version)
    {
        ArgumentNullException.ThrowIfNull(data);
        Size = (version == 3 ? 4u : 0u) + SynchData.ToUInt(data.Mid(0, 4));
    }

    public ExtendedHeader Clone() => new ExtendedHeader { Size = Size };

    object ICloneable.Clone() => Clone();
}

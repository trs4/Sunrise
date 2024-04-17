namespace Sunrise.Model.TagLib.Id3v2;

public class UnknownFrame : Frame
{
    public UnknownFrame(ByteVector type, ByteVector? data)
        : base(type, 4)
        => Data = data;

    public UnknownFrame(ByteVector type) : this(type, null) { }

    public UnknownFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal UnknownFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public ByteVector? Data { get; set; }

    protected override void ParseFields(ByteVector data, byte version) => Data = data;

    protected override ByteVector RenderFields(byte version) => Data ?? [];
}

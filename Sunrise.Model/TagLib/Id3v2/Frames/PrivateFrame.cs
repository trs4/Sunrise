namespace Sunrise.Model.TagLib.Id3v2;

public class PrivateFrame : Frame
{
    public PrivateFrame(string owner, ByteVector? data)
        : base(FrameType.PRIV, 4)
    {
        Owner = owner;
        PrivateData = data;
    }

    public PrivateFrame(string owner) : this(owner, null) { }

    public PrivateFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal PrivateFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public string Owner { get; private set; }

    public ByteVector? PrivateData { get; set; }

    public static PrivateFrame? Get(Tag tag, string owner, bool create)
    {
        PrivateFrame priv;

        foreach (Frame frame in tag.GetFrames(FrameType.PRIV))
        {
            priv = frame as PrivateFrame;

            if (priv is not null && priv.Owner == owner)
                return priv;
        }

        if (!create)
            return null;

        priv = new PrivateFrame(owner);
        tag.AddFrame(priv);
        return priv;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        if (data.Count < 1)
            throw new CorruptFileException("A private frame must contain at least 1 byte.");

        var l = ByteVectorCollection.Split(data, ByteVector.TextDelimiter(StringType.Latin1), 1, 2);

        if (l.Count == 2)
        {
            Owner = l[0].ToString(StringType.Latin1);
            PrivateData = l[1];
        }
    }

    protected override ByteVector RenderFields(byte version)
    {
        if (version < 3)
            throw new NotImplementedException();

#pragma warning disable IDE0028 // Simplify collection initialization
        return new ByteVector
        {
            ByteVector.FromString (Owner, StringType.Latin1),
            ByteVector.TextDelimiter (StringType.Latin1),
            PrivateData
        };
#pragma warning restore IDE0028 // Simplify collection initialization
    }

    public override Frame Clone()
    {
        var frame = new PrivateFrame(Owner);

        if (PrivateData is not null)
            frame.PrivateData = new ByteVector(PrivateData);

        return frame;
    }

}

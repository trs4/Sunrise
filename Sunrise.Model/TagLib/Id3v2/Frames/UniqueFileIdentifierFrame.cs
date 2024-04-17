namespace Sunrise.Model.TagLib.Id3v2;

public class UniqueFileIdentifierFrame : Frame
{
    public UniqueFileIdentifierFrame(string owner, ByteVector? identifier)
        : base(FrameType.UFID, 4)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Identifier = identifier;
    }

    public UniqueFileIdentifierFrame(string owner) : this(owner, null) { }

    public UniqueFileIdentifierFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal UniqueFileIdentifierFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public string Owner { get; private set; }

    public ByteVector? Identifier { get; set; }

    public static UniqueFileIdentifierFrame? Get(Tag tag, string owner, bool create)
    {
        UniqueFileIdentifierFrame ufid;

        foreach (Frame frame in tag.GetFrames(FrameType.UFID))
        {
            ufid = frame as UniqueFileIdentifierFrame;

            if (ufid is null)
                continue;

            if (ufid.Owner == owner)
                return ufid;
        }

        if (!create)
            return null;

        ufid = new UniqueFileIdentifierFrame(owner, null);
        tag.AddFrame(ufid);
        return ufid;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        var fields = ByteVectorCollection.Split(data, (byte)0);

        if (fields.Count != 2)
            return;

        Owner = fields[0].ToString(StringType.Latin1);
        Identifier = fields[1];
    }

    protected override ByteVector RenderFields(byte version) => new ByteVector
    {
        ByteVector.FromString (Owner, StringType.Latin1),
        ByteVector.TextDelimiter (StringType.Latin1),
        Identifier,
    };

    public override Frame Clone()
    {
        var frame = new UniqueFileIdentifierFrame(Owner);

        if (Identifier is not null)
            frame.Identifier = new ByteVector(Identifier);

        return frame;
    }

}

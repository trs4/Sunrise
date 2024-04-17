namespace Sunrise.Model.TagLib.Id3v2;

public class MusicCdIdentifierFrame : Frame
{
    public MusicCdIdentifierFrame() : base(FrameType.MCDI, 4) { }

    public MusicCdIdentifierFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal MusicCdIdentifierFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public ByteVector Data { get; set; }

    public static MusicCdIdentifierFrame? Get(Tag tag, bool create)
    {
        MusicCdIdentifierFrame mcdi;

        foreach (Frame frame in tag)
        {
            mcdi = frame as MusicCdIdentifierFrame;

            if (mcdi is not null)
                return mcdi;
        }

        if (!create)
            return null;

        mcdi = new MusicCdIdentifierFrame();
        tag.AddFrame(mcdi);
        return mcdi;
    }

    protected override void ParseFields(ByteVector data, byte version) => Data = data;

    protected override ByteVector RenderFields(byte version) => Data is null ? [] : Data;

    public override Frame Clone()
    {
        var frame = new MusicCdIdentifierFrame();

        if (Data is not null)
            frame.Data = new ByteVector(Data);

        return frame;
    }

}

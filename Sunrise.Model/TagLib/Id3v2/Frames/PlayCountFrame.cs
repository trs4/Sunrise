namespace Sunrise.Model.TagLib.Id3v2;

public class PlayCountFrame : Frame
{
    public PlayCountFrame() : base(FrameType.PCNT, 4) { }

    public PlayCountFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal PlayCountFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public ulong PlayCount { get; set; }

    public static PlayCountFrame? Get(Tag tag, bool create)
    {
        PlayCountFrame pcnt;

        foreach (Frame frame in tag)
        {
            pcnt = frame as PlayCountFrame;

            if (pcnt is not null)
                return pcnt;
        }

        if (!create)
            return null;

        pcnt = new PlayCountFrame();
        tag.AddFrame(pcnt);
        return pcnt;
    }

    protected override void ParseFields(ByteVector data, byte version) => PlayCount = data.ToULong();

    protected override ByteVector RenderFields(byte version)
    {
        ByteVector data = ByteVector.FromULong(PlayCount);

        while (data.Count > 4 && data[0] == 0)
            data.RemoveAt(0);

        return data;
    }

    public override Frame Clone() => new PlayCountFrame { PlayCount = PlayCount };
}

namespace Sunrise.Model.TagLib.Mpeg;

public struct XingHeader
{
    public static readonly ReadOnlyByteVector FileIdentifier = "Xing";
    public static readonly XingHeader Unknown = new XingHeader(0, 0);

    private XingHeader(uint frame, uint size)
    {
        TotalFrames = frame;
        TotalSize = size;
        Present = false;
    }

    public XingHeader(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!data.StartsWith(FileIdentifier))
            throw new CorruptFileException("Not a valid Xing header");

        int position = 8;

        if ((data[7] & 0x01) != 0)
        {
            TotalFrames = data.Mid(position, 4).ToUInt();
            position += 4;
        }
        else
            TotalFrames = 0;

        if ((data[7] & 0x02) != 0)
        {
            TotalSize = data.Mid(position, 4).ToUInt();
            position += 4;
        }
        else
            TotalSize = 0;

        Present = true;
    }

    public uint TotalFrames { get; private set; }

    public uint TotalSize { get; private set; }

    public bool Present { get; private set; }

    public static int XingHeaderOffset(Version version, ChannelMode channelMode)
    {
        bool single_channel = channelMode == ChannelMode.SingleChannel;

        if (version == Version.Version1)
            return single_channel ? 0x15 : 0x24;
        else
            return single_channel ? 0x0D : 0x15;
    }

}

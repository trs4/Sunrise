namespace Sunrise.Model.TagLib.Mpeg;

public struct VBRIHeader
{
    public static readonly ReadOnlyByteVector FileIdentifier = "VBRI";
    public static readonly VBRIHeader Unknown = new VBRIHeader(0, 0);

    private VBRIHeader(uint frame, uint size)
    {
        TotalFrames = frame;
        TotalSize = size;
        Present = false;
    }

    public VBRIHeader(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!data.StartsWith(FileIdentifier))
            throw new CorruptFileException("Not a valid VBRI header");

        int position = 10;
        TotalSize = data.Mid(position, 4).ToUInt();
        position += 4;
        TotalFrames = data.Mid(position, 4).ToUInt();
        //position += 4;
        Present = true;
    }

    public uint TotalFrames { get; private set; }

    public uint TotalSize { get; private set; }

    public bool Present { get; private set; }

    public static int VBRIHeaderOffset() => 0x24;
}

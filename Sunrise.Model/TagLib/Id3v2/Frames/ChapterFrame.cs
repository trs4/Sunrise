namespace Sunrise.Model.TagLib.Id3v2;

public class ChapterFrame : Frame
{
    public ChapterFrame() : base(FrameType.CHAP, 4) { }

    public ChapterFrame(string id)
        : this()
        => Id = id;

    public ChapterFrame(string id, string title)
        : this(id)
        => SubFrames.Add(new TextInformationFrame("TIT2") { Text = new[] { title } });

    public ChapterFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal ChapterFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public string Id { get; set; }

    public uint StartMilliseconds { get; set; }

    public uint EndMilliseconds { get; set; }

    public uint StartByteOffset { get; set; } = 0xFFFFFFFF;

    public uint EndByteOffset { get; set; } = 0xFFFFFFFF;

    public List<Frame> SubFrames { get; set; } = [];

    protected override void ParseFields(ByteVector data, byte version)
    {
        // https://id3.org/id3v2-chapters-1.0
        int idLength = data.IndexOf(0) + 1;

        Id = data.ToString(StringType.Latin1, 0, idLength - 1); // Always Latin1, at least there is no mention of encoding in the spec
        StartMilliseconds = data.Mid(idLength, 4).ToUInt();
        EndMilliseconds = data.Mid(idLength + 4, 4).ToUInt();
        StartByteOffset = data.Mid(idLength + 8, 4).ToUInt(); // I don’t really know why one would use the offsets.
        EndByteOffset = data.Mid(idLength + 12, 4).ToUInt(); // They are to be ignored if all 4 Bytes are FF, i.e. 4,294,967,295.

        SubFrames = [];
        int frame_data_position = idLength + 16;
        int frame_data_endposition = data.Count;

        while (frame_data_position < frame_data_endposition)
        {
            Frame frame;

            try
            {
                frame = FrameFactory.CreateFrame(data, null, ref frame_data_position, version, true);
            }
            catch (NotImplementedException)
            {
                continue;
            }
            catch (CorruptFileException)
            {
                throw;
            }

            if (frame is null)
                break;

            if (frame.Size == 0)
                continue;

            SubFrames.Add(frame);
        }
    }

    protected override ByteVector RenderFields(byte version)
    {
        var data = ByteVector.FromString(Id, StringType.Latin1);
        data.Add(0); //it would be neat if Add were chainable…
        data.Add(ByteVector.FromUInt(StartMilliseconds));
        data.Add(ByteVector.FromUInt(EndMilliseconds));
        data.Add(ByteVector.FromUInt(StartByteOffset));
        data.Add(ByteVector.FromUInt(EndByteOffset));

        foreach (var f in SubFrames)
            data.Add(f.Render(version));

        return data;
    }

    public override Frame Clone()
    {
        var frame = new ChapterFrame(Id)
        {
            StartMilliseconds = StartMilliseconds,
            EndMilliseconds = EndMilliseconds,
            StartByteOffset = StartByteOffset,
            EndByteOffset = EndByteOffset,
        };

        foreach (var f in SubFrames)
            frame.SubFrames.Add(f.Clone());

        return frame;
    }

}
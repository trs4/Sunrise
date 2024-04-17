namespace Sunrise.Model.TagLib.Id3v2;

public class TableOfContentsFrame : Frame
{
    [Flags]
    private enum CTOCFlags : byte
    {
        TopLevel = 1,
        Ordered = 2,
    }

    public TableOfContentsFrame() : base(FrameType.CTOC, 4) { }

    public TableOfContentsFrame(string id)
        : this()
        => Id = id;

    public TableOfContentsFrame(string id, string title)
        : this(id)
        => SubFrames.Add(new TextInformationFrame("TIT2") { Text = new[] { title } });

    public TableOfContentsFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal TableOfContentsFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public string Id { get; set; }

    public bool IsTopLevel { get; set; }

    public bool IsOrdered { get; set; }

    public List<string> ChapterIds { get; set; } = [];

    public List<Frame> SubFrames { get; set; } = [];

    protected override void ParseFields(ByteVector data, byte version)
    {
        // https://id3.org/id3v2-chapters-1.0
        int idLength = data.IndexOf(0) + 1;
        Id = data.ToString(StringType.Latin1, 0, idLength - 1);
        var flags = (CTOCFlags)data[idLength]; // Flags %000000ab Entry count $xx (8-bit unsigned int)
        IsTopLevel = flags.HasFlag(CTOCFlags.TopLevel); // a
        IsOrdered = flags.HasFlag(CTOCFlags.Ordered); // b
        var chapterCount = data[idLength + 1]; // Entry count $xx (8-bit unsigned int)

        if (data.Count <= idLength + 2)
            return; // no chapter ids and no subframes

        int position = idLength + 2;

        for (int i = 0; i < chapterCount; i++)
        {
            int nextPosition = data.Find(0, position) + 1;
            ChapterIds.Add(data.Mid(position, nextPosition - position - 1).ToString(StringType.Latin1));
            position = nextPosition;
        }

        SubFrames = [];
        int frame_data_endposition = data.Count;

        while (position < frame_data_endposition)
        {
            Frame frame;

            try
            {
                frame = FrameFactory.CreateFrame(data, null, ref position, version, true /* ? */);
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

            if (frame.Size == 0) // Only add frames that contain data
                continue;

            SubFrames.Add(frame);
        }
    }

    protected override ByteVector RenderFields(byte version)
    {
        var data = ByteVector.FromString(Id, StringType.Latin1);
        data.Add(0);
        data.Add((byte)((IsTopLevel ? CTOCFlags.TopLevel : 0) | (IsOrdered ? CTOCFlags.Ordered : 0)));
        data.Add(ChapterIds.Count >= byte.MaxValue ? byte.MaxValue : (byte)ChapterIds.Count);

        foreach (var chap in ChapterIds)
        {
            data.Add(ByteVector.FromString(chap, StringType.Latin1));
            data.Add(0);
        }

        foreach (var f in SubFrames)
            data.Add(f.Render(version));

        return data;
    }

    public override Frame Clone()
    {
        var frame = new TableOfContentsFrame(Id)
        {
            IsTopLevel = IsTopLevel,
            IsOrdered = IsOrdered,
        };

        foreach (var c in ChapterIds)
            frame.ChapterIds.Add(c);

        foreach (var f in SubFrames)
            frame.SubFrames.Add(f.Clone());

        return frame;
    }

}

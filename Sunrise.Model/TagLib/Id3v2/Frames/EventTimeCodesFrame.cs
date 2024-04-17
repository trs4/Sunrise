namespace Sunrise.Model.TagLib.Id3v2;

public class EventTimeCodesFrame : Frame
{
    public EventTimeCodesFrame()
        : base(FrameType.ETCO, 4)
        => Flags = FrameFlags.FileAlterPreservation;

    public EventTimeCodesFrame(TimestampFormat timestampFormat)
        : base(FrameType.ETCO, 4)
    {
        TimestampFormat = timestampFormat;
        Flags = FrameFlags.FileAlterPreservation;
    }

    public EventTimeCodesFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    public EventTimeCodesFrame(FrameHeader frameHeader) : base(frameHeader) { }

    public EventTimeCodesFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public TimestampFormat TimestampFormat { get; set; }

    public List<EventTimeCode> Events { get; set; }

    public static EventTimeCodesFrame? Get(Tag tag, bool create)
    {
        EventTimeCodesFrame etco;

        foreach (Frame frame in tag)
        {
            etco = frame as EventTimeCodesFrame;

            if (etco is not null)
                return etco;
        }

        if (!create)
            return null;

        etco = new EventTimeCodesFrame();
        tag.AddFrame(etco);
        return etco;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        Events = [];
        TimestampFormat = (TimestampFormat)data.Data[0];
        var incomingEventsData = data.Mid(1);

        for (var i = 0; i < incomingEventsData.Count - 1; i++)
        {
            var eventType = (EventType)incomingEventsData.Data[i];
            i++;

            var timestampData = new ByteVector(incomingEventsData.Data[i],
                incomingEventsData.Data[i + 1],
                incomingEventsData.Data[i + 2],
                incomingEventsData.Data[i + 3]);

            i += 3;
            var timestamp = timestampData.ToInt();
            Events.Add(new EventTimeCode(eventType, timestamp));
        }
    }

    protected override ByteVector RenderFields(byte version)
    {
        var data = new List<byte> { (byte)TimestampFormat };

        foreach (var @event in Events)
        {
            data.Add((byte)@event.TypeOfEvent);
            var timeData = ByteVector.FromInt(@event.Time);
            data.AddRange(timeData.Data);
        }

        return new ByteVector([.. data]);
    }

    public override Frame Clone() => new EventTimeCodesFrame(_header)
    {
        TimestampFormat = TimestampFormat,
        Events = Events.ConvertAll(item => (EventTimeCode)item.Clone()),
    };
}

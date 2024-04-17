namespace Sunrise.Model.TagLib.Id3v2;

public class EventTimeCode : ICloneable
{
    public EventTimeCode(EventType typeOfEvent, int time)
    {
        TypeOfEvent = typeOfEvent;
        Time = time;
    }

    public EventType TypeOfEvent { get; set; }

    public int Time { get; set; }

    public static EventTimeCode CreateEmpty() => new EventTimeCode(EventType.Padding, 0);

    public object Clone() => new EventTimeCode(TypeOfEvent, Time);
}

namespace Sunrise.Model.TagLib.Id3v2;

public struct SynchedText
{
    public SynchedText(long time, string text)
    {
        Time = time;
        Text = text;
    }

    public long Time { get; set; }

    public string Text { get; set; }
}
